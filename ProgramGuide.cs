using System.Collections.Immutable;
using System.Xml.Linq;
using System.Xml.XPath;
using L = TelevisionSimulatorGuideData.ListingFileService;

namespace TelevisionSimulatorGuideData {
    /// <summary>
    /// Reworked program guide to use the new data structure.
    /// </summary>
    public class ProgramGuide {
        private const int MinutesInDay = 1440;

        /// <summary>
        /// Gets the current guide data in TVSL JSON format.
        /// </summary>
        /// <param name="now">Time to retrieve data for</param>
        /// <param name="numberOfTimeslots">Number of timeslots to provide info on</param>
        /// <param name="minutesPerTimeslot">Minutes per timeslot to determine min and max range for data</param>
        /// <param name="lowerChannelLimitInclusive">If specified, filter out channels below this number.</param>
        /// <param name="upperChannelLimitExclusive">If specified, filter out channels above this number.</param>
        /// <returns></returns>
        public List<ChannelData> GetData(DateTimeOffset? now = null, int numberOfTimeslots = 3, int minutesPerTimeslot = 30, int? lowerChannelLimitInclusive = null, int? upperChannelLimitExclusive = null)
        {
            if (numberOfTimeslots < 1) {
                throw new ArgumentOutOfRangeException(nameof(numberOfTimeslots), "Number of timeslots must be greater than 0.");
            }

            if (MinutesInDay % minutesPerTimeslot != 0) {
                throw new ArgumentOutOfRangeException(nameof(minutesPerTimeslot), "Minutes per timeslot must divide evenly into the number of minutes in a day (1440). Valid options are: 1, 2, 3, 4, 5, 6, 8, 9, 10, 12, 15, 16, 18, 20, 24, 30, 36, 40, 45, 48, 60, 72, 80, 90, 120, 144, 180, 240, 360, 720, 1440.");
            }

#if DEBUG
            now ??= DateTimeOffset.Parse("3/24/2024 12:15 AM"); // For testing purposes
#else
            now ??= DateTimeOffset.Now;
#endif
            if (null == L.ListingsDocument) {
                throw new InvalidOperationException("Listings document is not loaded yet. Ensure the file exists and retry the request.");
            }

            if (null == L.Channels) {
                throw new InvalidOperationException("Channel data is not loaded yet. Ensure the file exists and retry the request.");
            }

            var resolution = MinutesInDay / minutesPerTimeslot;
            var startingTimeslot = Enumerable.Range(0, resolution)
                .Select(p => now.Value.Date.AddMinutes(p * minutesPerTimeslot))
                .Last(p => p <= now);
            var endTime = startingTimeslot.AddMinutes(minutesPerTimeslot * numberOfTimeslots);

            Dictionary<string, ChannelData> channels;
            IEnumerable<XElement> programs;
            if (lowerChannelLimitInclusive.HasValue || upperChannelLimitExclusive.HasValue) {
                if (lowerChannelLimitInclusive.HasValue && upperChannelLimitExclusive.HasValue && lowerChannelLimitInclusive >= upperChannelLimitExclusive) {
                    throw new ArgumentOutOfRangeException(nameof(lowerChannelLimitInclusive), "Lower channel limit must be less than upper channel limit.");
                }

                channels = L.Channels.Where(kv =>
                    (!lowerChannelLimitInclusive.HasValue || kv.Value.Number >= lowerChannelLimitInclusive) &&
                    (!upperChannelLimitExclusive.HasValue || kv.Value.Number < upperChannelLimitExclusive))
                    .ToDictionary();

                programs = L.GetProgramsForTimeRange(startingTimeslot, endTime, channels.Select(kv => kv.Key).ToList());
            }
            else {
                channels = L.Channels;
                programs = L.GetProgramsForTimeRange(startingTimeslot, endTime);
            }
            
            var listings = programs.Select(p => {
                var category = p.Elements("category").FirstOrDefault(p =>
                        Metadata.ColorCodedCategories.Contains(p.Value, StringComparer.OrdinalIgnoreCase))?.Value
                    .ToLowerInvariant();
                var start = p.Attribute("start")?.Value;
                if (string.IsNullOrWhiteSpace(start)) {
                    throw new InvalidOperationException("Program start time is missing.");
                }

                var channel = p.Attribute("channel")?.Value;
                if (string.IsNullOrWhiteSpace(channel)) {
                    throw new InvalidOperationException("Program channel is missing.");
                }

                var spanInfo = GetSpan(start.ToDateTimeOffsetFromXmlTvTime(),
                    p.Attribute("stop").Value.ToDateTimeOffsetFromXmlTvTime(), startingTimeslot, endTime);

                return new ListingData {
                    ChannelId = channel,
                    Start = start,
                    Title = p.Element("title")?.Value,
                    Category = category,
                    IsStereo = p.XPathSelectElement("stereo")?.Value != null,
                    IsSubtitled = p.XPathSelectElement("subtitles")?.Value != null,
                    Rating = p.XPathSelectElement($"rating[@system='{(category == "movie" ? "MPAA" : "VCHIP")}']/value")?.Value,
                    Span = spanInfo.Span,
                    IsContinuedLeft = spanInfo.IsContinuedLeft,
                    IsContinuedRight = spanInfo.IsContinuedRight
                };
            }).Where(p => p.Span > 0).OrderBy(p => p.Start);

            foreach (var listing in listings) {
                if (channels.TryGetValue(listing.ChannelId, out var channelData)) {
                    channelData.Listings.Add(listing);
                }
            }

            return channels.Values.ToList();
        }

        /// <summary>
        /// Gets the remaining time in minutes for the current program.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="startingTimeslot"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private SpanInfo GetSpan(DateTimeOffset start, DateTimeOffset stop, DateTimeOffset startingTimeslot, DateTimeOffset endTime) {
            if (stop > endTime && start < startingTimeslot) {
                return new SpanInfo {
                    Span = (int)(endTime - startingTimeslot).TotalMinutes,
                    IsContinuedLeft = true,
                    IsContinuedRight = true
                };
            }

            var si = new SpanInfo();
            
            if (stop < startingTimeslot) {
                return si;
            }

            si.Span = (int)(stop - start).TotalMinutes;
            
            if (start < startingTimeslot) {
                si.Span -= (int)(startingTimeslot - start).TotalMinutes;
                si.IsContinuedLeft = true;
            }

            if (stop > endTime) {
                si.Span -= (int)(stop - endTime).TotalMinutes;
                si.IsContinuedRight = true;
            }

            return si;
        }
    }
}
