xbuild Kinvey.Core/Kinvey.Core.csproj \
&& xbuild TestFramework/Tests.Integration/Tests.Integration.csproj \
&& mono TestFramework/Tools/Gaillard.SharpCover/bin/Debug/SharpCover.exe instrument config.json \
&& mono packages/NUnit.ConsoleRunner.3.4.1/tools/nunit3-console.exe TestFramework/Tests.Integration/bin/Debug/Tests.Integration.dll \
&& mono TestFramework/Tools/Gaillard.SharpCover/bin/Debug/SharpCover.exe check
