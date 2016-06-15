VERSION=$(shell xml sel -t -v package/metadata/version Kinvey-Xamarin.nuspec)

all: clean build doc
	
build:
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
	find ./api/reference/html/ -name "*-e" | xargs rm
	
pack:
	rm -Rf release/kinvey-xamarin-$(VERSION)
	mkdir release/kinvey-xamarin-$(VERSION)
	cp Kinvey-Utils/bin/Release/Kinvey-Utils.dll release/kinvey-xamarin-$(VERSION)
	cp Kinvey-Xamarin/bin/Release/Kinvey-Xamarin.dll release/kinvey-xamarin-$(VERSION)
	cp Kinvey-Xamarin/bin/Release/RestSharp.Portable.dll release/kinvey-xamarin-$(VERSION)
	cp LICENSE.txt release/kinvey-xamarin-$(VERSION)
	cp README.txt release/kinvey-xamarin-$(VERSION)
	cd release; zip -r kinvey-xamarin-$(VERSION).zip kinvey-xamarin-$(VERSION)
	
nuget-pack:
	nuget pack Kinvey-Xamarin.nuspec
	nuget pack Kinvey-Xamarin-iOS.nuspec
	nuget pack Kinvey-Xamarin-Android.nuspec
	
nuget-push:
	nuget setApiKey fd40b430-eb17-443f-b41a-b12391b86eca
	nuget push Kinvey.$(shell xml sel -t -v package/metadata/version Kinvey-Xamarin.nuspec).nupkg
	nuget push Kinvey-ios.$(shell xml sel -t -v package/metadata/version Kinvey-Xamarin-iOS.nuspec).nupkg
	nuget push Kinvey-Android.$(shell xml sel -t -v package/metadata/version Kinvey-Xamarin-Android.nuspec).nupkg
	
show-version:
	@cat Kinvey-Xamarin/Core/KinveyHeaders.cs | grep 'public static string VERSION = "\d.\d.\d";' | awk '{$$1=$$1;print}' | awk {'print $$6'} | sed s/[\"\;]//g | xargs echo 'KinveyHeaders.cs      '
	@xml sel -t -v package/metadata/version Kinvey-Xamarin.nuspec | xargs echo 'Kinvey-Xamarin        '
	@xml sel -t -v package/metadata/version Kinvey-Xamarin-iOS.nuspec | xargs -I version echo 'Kinvey-Xamarin-iOS    ' version '-> Kinvey-Xamarin Dependency:' $(shell xml sel -t -v "package/metadata/dependencies/dependency[@id='Kinvey']/@version" Kinvey-Xamarin-iOS.nuspec)
	@xml sel -t -v package/metadata/version Kinvey-Xamarin-Android.nuspec | xargs -I version echo Kinvey-Xamarin-Android version '-> Kinvey-Xamarin Dependency:' $(shell xml sel -t -v "package/metadata/dependencies/dependency[@id='Kinvey']/@version" Kinvey-Xamarin-Android.nuspec)
	
set-version:
	@sed -i -e 's/public static string VERSION = \"[0-9]*.[0-9]*.[0-9]*\"\;/public static string VERSION = \"$(filter-out $@,$(MAKECMDGOALS))\";/g' Kinvey-Xamarin/Core/KinveyHeaders.cs
	@rm Kinvey-Xamarin/Core/KinveyHeaders.cs-e
	
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
	
deploy-reference:
	rm -Rf devcenter
	git clone git@github.com:Kinvey/devcenter.git
	cd devcenter; \
	git remote add staging git@heroku.com:v3yk1n-devcenter.git; \
	git remote add production git@heroku.com:kinvey-devcenter-prod.git
	rm -R devcenter/content/reference/xamarin/api
	cp -R api/reference/html devcenter/content/reference/xamarin
	mv devcenter/content/reference/xamarin/html devcenter/content/reference/xamarin/api
	cd devcenter; \
	git add content/reference/xamarin/*/*; \
	git commit -m "Xamarin Release Version $(VERSION)"; \
	git push origin master; \
	git push staging master; \
	git push production master

clean:
	rm -Rf api
	rm -Rf Kinvey-Xamarin/bin
	rm -Rf Kinvey-Xamarin-iOS/bin
	rm -Rf Kinvey-Xamarin-Android/bin
