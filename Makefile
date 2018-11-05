XMLSTARLET = xmlstarlet
VERSION = $(shell cat Kinvey.NuGet/Kinvey.NuGet.nuproj | grep PackageVersion | awk '{ print $1 }' | $(XMLSTARLET) sel -t -v PackageVersion)

version:
	@echo $(VERSION)

nuget-pack:
	cd Kinvey.NuGet; \
	msbuild /t:Pack /p:Configuration=Release; \
	cd bin/Release; \
	unzip -o Kinvey.$(VERSION).nupkg -d Kinvey.$(VERSION); \
	rm Kinvey.$(VERSION).nupkg; \
	cd Kinvey.$(VERSION); \
	cat Kinvey.nuspec | \
		grep -v '<dependency id="System.' | \
		grep -v '<dependency id="Microsoft.' | \
		grep -v '<dependency id="NETStandard.' | \
		grep -v '<dependency id="Portable.BouncyCastle"' | \
		grep -v '<dependency id="SQLitePCLRaw.core"' | \
		grep -v '<dependency id="SQLitePCLRaw.lib.' | \
		grep -v '<dependency id="SQLitePCLRaw.provider.' | \
		grep -v '<dependency id="Xamarin.Android.Support.v4"' | \
		grep -v '<dependency id="Xamarin.GooglePlayServices.Base"' | \
		awk '{ gsub("\"MonoAndroid9.0\"", "\"MonoAndroid10\""); print }' | \
		awk '{ gsub("\"Xamarin.iOS1.0\"", "\"Xamarin.iOS10\""); print }' \
		> Kinvey-changed.nuspec; \
	rm Kinvey.nuspec; \
	mv Kinvey-changed.nuspec Kinvey.nuspec; \
	rm lib/**/System.dll; \
	rm lib/**/System.*.dll; \
	rm lib/**/Microsoft.*.dll; \
	rm lib/**/mscorlib.dll; \
	rm lib/**/netstandard.dll; \
	zip -r ../Kinvey.$(VERSION).nupkg **
