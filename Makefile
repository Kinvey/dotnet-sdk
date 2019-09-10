XMLSTARLET = xmlstarlet
VERSION = $(shell cat Kinvey.NuGet/Kinvey.NuGet.nuproj | grep PackageVersion | awk '{ print $1 }' | $(XMLSTARLET) sel -t -v PackageVersion)
NUSPEC_XMLNS = "http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd"

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
		awk '{ gsub("\"MonoAndroid9.0\"", "\"MonoAndroid0.0\""); print }' \
		> Kinvey-changed.nuspec; \
	rm Kinvey.nuspec; \
	mv Kinvey-changed.nuspec Kinvey.nuspec; \
	$(XMLSTARLET) ed -N x=$(NUSPEC_XMLNS) \
	                 -i "//x:licenseUrl" \
					 -t elem \
					 -n "license" \
					 -v "Apache-2.0" \
					 Kinvey.nuspec | \
					$(XMLSTARLET) ed -N x=$(NUSPEC_XMLNS) \
					 -i "//x:license" \
					 -t attr \
					 -n "type" \
					 -v "expression" \
					 > Kinvey-changed.nuspec; \
	rm Kinvey.nuspec; \
	mv Kinvey-changed.nuspec Kinvey.nuspec; \
	mv lib/monoandroid90 lib/MonoAndroid; \
	mv lib/xamarinios10 lib/Xamarin.iOS10; \
	rm lib/MonoAndroid/monoandroid90; \
	rm lib/Xamarin.iOS10/xamarinios10; \
	rm lib/**/System.dll; \
	rm lib/**/System.*.dll; \
	rm lib/**/Microsoft.*.dll; \
	rm lib/**/mscorlib.dll; \
	rm lib/**/netstandard.dll; \
	zip -r ../Kinvey.$(VERSION).nupkg **; \
	cp -R lib ../kinvey-xamarin-$(VERSION); \
	cd ..; \
	zip -r kinvey-xamarin-$(VERSION).zip kinvey-xamarin-$(VERSION)

doc:
	mdoc update \
		-L packages/Newtonsoft.Json.11.0.2/lib/netstandard2.0 \
		-L packages/Remotion.Linq.2.2.0/lib/netstandard1.0 \
		-L /Library/Frameworks/Xamarin.Android.framework/Versions/Current/lib/mandroid/platforms/android-28 \
		-L /Library/Frameworks/Xamarin.Android.framework/Versions/Current/lib/xamarin.android/xbuild/Xamarin/Android \
		-o api/reference/doc/netstandard2.0 \
		-i Kinvey/bin/Release/netstandard2.0/Kinvey.xml Kinvey/bin/Release/netstandard2.0/Kinvey.dll \
		--debug

	mdoc update \
		-L packages/Newtonsoft.Json.11.0.2/lib/netstandard2.0 \
		-L packages/Remotion.Linq.2.2.0/lib/netstandard1.0 \
		-L /Library/Frameworks/Xamarin.Android.framework/Versions/Current/lib/mandroid/platforms/android-28 \
		-L /Library/Frameworks/Xamarin.Android.framework/Versions/Current/lib/xamarin.android/xbuild/Xamarin/Android \
		-o api/reference/doc/ios \
		-i Kinvey.iOS/bin/Release/Kinvey.xml Kinvey.iOS/bin/Release/Kinvey.dll \
		--debug
	
	mdoc update \
		-L packages/Newtonsoft.Json.11.0.2/lib/netstandard2.0 \
		-L packages/Remotion.Linq.2.2.0/lib/netstandard1.0 \
		-L /Library/Frameworks/Xamarin.Android.framework/Versions/Current/lib/mandroid/platforms/android-28 \
		-L /Library/Frameworks/Xamarin.Android.framework/Versions/Current/lib/xamarin.android/xbuild/Xamarin/Android \
		-o api/reference/doc/android \
		-i Kinvey.Android/bin/Release/Kinvey.xml Kinvey.Android/bin/Release/Kinvey.dll \
		--debug
	
	xml ed -u Overview/Title -v "Kinvey SDK" api/reference/doc/netstandard2.0/index.xml > api/reference/doc/netstandard2.0/index-modified.xml
	xml ed -u Overview/Title -v "Kinvey SDK" api/reference/doc/ios/index.xml > api/reference/doc/ios/index-modified.xml
	xml ed -u Overview/Title -v "Kinvey SDK" api/reference/doc/android/index.xml > api/reference/doc/android/index-modified.xml

	xml ed -u Namespace/Docs/summary -v ".NET Standard 2.0" api/reference/doc/netstandard2.0/ns-Kinvey.xml > api/reference/doc/netstandard2.0/ns-Kinvey-modified.xml
	xml ed -u Namespace/Docs/summary -v "Xamarin iOS" api/reference/doc/ios/ns-Kinvey.xml > api/reference/doc/ios/ns-Kinvey-modified.xml
	xml ed -u Namespace/Docs/summary -v "Xamarin Android" api/reference/doc/android/ns-Kinvey.xml > api/reference/doc/android/ns-Kinvey-modified.xml
	
	rm api/reference/doc/netstandard2.0/index.xml
	rm api/reference/doc/ios/index.xml
	rm api/reference/doc/android/index.xml

	rm api/reference/doc/netstandard2.0/ns-Kinvey.xml
	rm api/reference/doc/ios/ns-Kinvey.xml
	rm api/reference/doc/android/ns-Kinvey.xml
	
	mv api/reference/doc/netstandard2.0/index-modified.xml api/reference/doc/netstandard2.0/index.xml
	mv api/reference/doc/ios/index-modified.xml api/reference/doc/ios/index.xml
	mv api/reference/doc/android/index-modified.xml api/reference/doc/android/index.xml

	mv api/reference/doc/netstandard2.0/ns-Kinvey-modified.xml api/reference/doc/netstandard2.0/ns-Kinvey.xml
	mv api/reference/doc/ios/ns-Kinvey-modified.xml api/reference/doc/ios/ns-Kinvey.xml
	mv api/reference/doc/android/ns-Kinvey-modified.xml api/reference/doc/android/ns-Kinvey.xml
	
	mdoc export-html api/reference/doc/netstandard2.0 -o api/reference/html/netstandard2.0
	mdoc export-html api/reference/doc/ios -o api/reference/html/ios
	mdoc export-html api/reference/doc/android -o api/reference/html/android
	
	find ./api/reference/html/ -name "*.html" | xargs sed -i -e 's/Documentation for this section has not yet been entered.//g'
	find ./api/reference/html/ -name "*.html" | xargs sed -i -e 's/To be added.//g'
	find ./api/reference/html/ -name "*-e" | xargs rm

docfx-documentation:
	rm -rf  Kinvey/_site; \
	rm -rf  Kinvey.Android/_site; \
	rm -rf  Kinvey.iOS/_site; \
	mono "packages/docfx.console.2.40.10/build/../tools/docfx.exe" Kinvey/docfx.json
	mono "packages/docfx.console.2.40.10/build/../tools/docfx.exe" Kinvey.Android/docfx.json
	mono "packages/docfx.console.2.40.10/build/../tools/docfx.exe" Kinvey.iOS/docfx.json