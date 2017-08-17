@echo --------------------------------------------------------------------------
@echo Remember to update the release notes in /UniversalBeaconLibrary.nuspec
@echo --------------------------------------------------------------------------
nuget pack ./UniversalBeaconLibrary.nuspec -OutputDirectory ./nupkg -Build -Symbols -Prop Configuration=Release



