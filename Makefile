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

clean:
	rm -Rf api
	rm -Rf Android-Libtester/bin
	rm -Rf Kinvey-Xamarin/bin
	rm -Rf Kinvey-Xamarin-iOS/bin
	rm -Rf Kinvey-Xamarin-Android/bin
