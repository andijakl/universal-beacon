nuget.exe setApiKey [apikey]
nuget push ./nupkg/UniversalBeaconLibrary.2.0.0.nupkg -Source https://www.nuget.org/api/v2/package
rem nuget push ./nupkg/UniversalBeaconLibrary.2.0.0.symbols.nupkg -source https://nuget.smbsrc.net/