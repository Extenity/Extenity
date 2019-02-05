Wwise implementation codes of Beyond Audio should unfortunately be placed outside of 
assembly definitions (because Wwise does not have assembly definitions that we can add
as a reference in Extenity assembly definitions) and should be placed outside of Plugins 
directory (because Wwise codes are not in plugins directory).

The only clean way seems to be putting Wwise implementation codes into each project's 
Subsystems directory.
