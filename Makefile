all: clean build doc
	
build:
	xbuild /p:Configuration=Release Android-Libtester/Android-Libtester.csproj
	xbuild /p:Configuration=Release Kinvey-Xamarin/Kinvey-Xamarin.csproj
	xbuild /p:Configuration=Release Kinvey-Xamarin-iOS/Kinvey-Xamarin-iOS.csproj
	xbuild /p:Configuration=Release Kinvey-Xamarin-Android/Kinvey-Xamarin-Android.csproj
	
doc:
	mdoc update \
		-L packages/Microsoft.Bcl.1.1.10/lib/net40 \
		-L Android-Libtester/obj/Release/assemblies \
		-o api/reference/doc \
		-i Kinvey-Xamarin/bin/Release/Kinvey-Xamarin.xml Kinvey-Xamarin/bin/Release/Kinvey-Xamarin.dll \
		-i Kinvey-Xamarin-iOS/bin/Release/Kinvey-Xamarin-iOS.xml Kinvey-Xamarin-iOS/bin/Release/Kinvey-Xamarin-iOS.dll \
		-i Kinvey-Xamarin-Android/bin/Release/Kinvey-Xamarin-Android.xml Kinvey-Xamarin-Android/bin/Release/Kinvey-Xamarin-Android.dll
	
	xml ed -u Overview/Title -v "Kinvey SDK" api/reference/doc/index.xml > api/reference/doc/index-modified.xml
	xml ed -u Namespace/Docs/summary -v "Classes in common for Android and iOS" api/reference/doc/ns-KinveyXamarin.xml > api/reference/doc/ns-KinveyXamarin-modified.xml
	xml ed -u Namespace/Docs/summary -v "Classes specific for iOS" api/reference/doc/ns-KinveyXamariniOS.xml > api/reference/doc/ns-KinveyXamariniOS-modified.xml
	xml ed -u Namespace/Docs/summary -v "Classes specific for Android" api/reference/doc/ns-KinveyXamarinAndroid.xml > api/reference/doc/ns-KinveyXamarinAndroid-modified.xml
	
	rm api/reference/doc/index.xml
	rm api/reference/doc/ns-KinveyXamarin.xml
	rm api/reference/doc/ns-KinveyXamariniOS.xml
	rm api/reference/doc/ns-KinveyXamarinAndroid.xml
	
	mv api/reference/doc/index-modified.xml api/reference/doc/index.xml
	mv api/reference/doc/ns-KinveyXamarin-modified.xml api/reference/doc/ns-KinveyXamarin.xml
	mv api/reference/doc/ns-KinveyXamariniOS-modified.xml api/reference/doc/ns-KinveyXamariniOS.xml
	mv api/reference/doc/ns-KinveyXamarinAndroid-modified.xml api/reference/doc/ns-KinveyXamarinAndroid.xml
	
	mdoc export-html api/reference/doc -o api/reference/html
	
	find ./api/reference/html/ -name "*.html" | xargs sed -i -e 's/Documentation for this section has not yet been entered.//g'
	find ./api/reference/html/ -name "*.html" | xargs sed -i -e 's/To be added.//g'
	
nuget:
	nuget pack Kinvey-Xamarin.nuspec
	nuget pack Kinvey-Xamarin-iOS.nuspec
	nuget pack Kinvey-Xamarin-Android.nuspec
	
show-version:
	@cat Kinvey-Xamarin/Core/KinveyHeaders.cs | grep 'public static string VERSION = "\d.\d.\d";' | awk '{$$1=$$1;print}' | awk {'print $$6'} | sed s/[\"\;]//g | xargs echo 'KinveyHeaders.cs      '
	@xml sel -t -v package/metadata/version Kinvey-Xamarin.nuspec | xargs echo 'Kinvey-Xamarin        '
	@xml sel -t -v package/metadata/version Kinvey-Xamarin-iOS.nuspec | xargs -I version echo 'Kinvey-Xamarin-iOS    ' version '-> Kinvey-Xamarin Dependency:' $(shell xml sel -t -v "package/metadata/dependencies/dependency[@id='Kinvey']/@version" Kinvey-Xamarin-iOS.nuspec)
	@xml sel -t -v package/metadata/version Kinvey-Xamarin-Android.nuspec | xargs -I version echo Kinvey-Xamarin-Android version '-> Kinvey-Xamarin Dependency:' $(shell xml sel -t -v "package/metadata/dependencies/dependency[@id='Kinvey']/@version" Kinvey-Xamarin-Android.nuspec)
	
set-version:
	@cat Kinvey-Xamarin/Core/KinveyHeaders.cs | sed 's/public static string VERSION = \"[0-9]*.[0-9]*.[0-9]*\"\;/public static string VERSION = \"$(filter-out $@,$(MAKECMDGOALS))\";/g' > Kinvey-Xamarin/Core/KinveyHeaders-new.cs
	@rm Kinvey-Xamarin/Core/KinveyHeaders.cs
	@mv Kinvey-Xamarin/Core/KinveyHeaders-new.cs Kinvey-Xamarin/Core/KinveyHeaders.cs
	
	@xml ed -u package/metadata/version -v $(filter-out $@,$(MAKECMDGOALS)) Kinvey-Xamarin.nuspec > Kinvey-Xamarin-new.nuspec
	@rm Kinvey-Xamarin.nuspec
	@mv Kinvey-Xamarin-new.nuspec Kinvey-Xamarin.nuspec
	
	@xml ed -u package/metadata/version -v $(filter-out $@,$(MAKECMDGOALS)) Kinvey-Xamarin-iOS.nuspec > Kinvey-Xamarin-iOS-new.nuspec
	@rm Kinvey-Xamarin-iOS.nuspec
	@mv Kinvey-Xamarin-iOS-new.nuspec Kinvey-Xamarin-iOS.nuspec
	
	@xml ed -u "package/metadata/dependencies/dependency[@id='Kinvey']/@version" -v $(filter-out $@,$(MAKECMDGOALS)) Kinvey-Xamarin-iOS.nuspec > Kinvey-Xamarin-iOS-new.nuspec
	@rm Kinvey-Xamarin-iOS.nuspec
	@mv Kinvey-Xamarin-iOS-new.nuspec Kinvey-Xamarin-iOS.nuspec
	
	@xml ed -u package/metadata/version -v $(filter-out $@,$(MAKECMDGOALS)) Kinvey-Xamarin-Android.nuspec > Kinvey-Xamarin-Android-new.nuspec
	@rm Kinvey-Xamarin-Android.nuspec
	@mv Kinvey-Xamarin-Android-new.nuspec Kinvey-Xamarin-Android.nuspec
	
	@xml ed -u "package/metadata/dependencies/dependency[@id='Kinvey']/@version" -v $(filter-out $@,$(MAKECMDGOALS)) Kinvey-Xamarin-Android.nuspec > Kinvey-Xamarin-Android-new.nuspec
	@rm Kinvey-Xamarin-Android.nuspec
	@mv Kinvey-Xamarin-Android-new.nuspec Kinvey-Xamarin-Android.nuspec
	
	@$(MAKE) show-version
	
%:
	@:

clean:
	rm -Rf api
	rm -Rf Android-Libtester/bin
	rm -Rf Kinvey-Xamarin/bin
	rm -Rf Kinvey-Xamarin-iOS/bin
	rm -Rf Kinvey-Xamarin-Android/bin
