xbuild /p:Configuration=Release Kinvey.Core/Kinvey.Core.csproj \
&& xbuild /p:Configuration=Release TestFramework/Tests.Integration/Tests.Integration.csproj \
&& mono TestFramework/Tools/Gaillard.SharpCover/bin/Debug/SharpCover.exe instrument TestFramework/Tools/config.json \
&& mono packages/NUnit.ConsoleRunner.3.4.1/tools/nunit3-console.exe TestFramework/Tests.Integration/bin/Release/Tests.Integration.dll \
&& mono Gaillard.SharpCover/bin/Debug/SharpCover.exe check
