# XMLTV to TVSL Conversion Server example

This is a barebones API that allows you to convert `XMLTV` files to Television Simulator Scheduling Language, a JSON schema that allows external listings to be loaded in Television Simulator '99.

This will work with TVS 2.0 (November update) and later. A known limitation of this program and the TVS program guide is that we need to coalesce XMLTV listing times to fit in a half-hour grid arrangement, meaning that some program times are rounded down or up.

You need to provide your own XMLTV listings to make this work. Start the app with `appsettings.json` pointing to your listing XML file and call `/guide` in your browser to confirm it's working.
