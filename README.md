Kinvey Xamarin Library
======

This is a Public Class Library (PCL) for various supported Xamarin runtimes.


##Release Process

1.  Ensure tests are passing
2.  Update the version number in `Kinvey-Xamarin/core/KinveyHeaders.cs`
3.  Set target to `Release`, and kick off a build.
4.  Copy __Kinvey-Utils/obj/Release/Kinvey-Utils.dll__, __Kinvey-Xamarin/obj/Release/Kinvey-Xamarin.dll__, and __Restsharp.Portable/obj/Release/RestSharp.portable.dll__ into the __release/kinvey-xamarin-x.x__ directory
5.  Zip up the __release/kinvey-xamarin-x.x.__ directory
6.  Check everything into github, and tag a release.
7.  Upload the zip to amazon
8.  on windows, do a git pull
9.  Ensure the project builds on windows
10.  run `nuget pack Kinvey-Xamarin.csproj -IncludeReferencedProjects`
11. Edit the file `Kinvey-Xamarin.nuspec` to up the version number and changelog (this is an xml file) 
12.  run `nuget push Kinvey-Xamarin.nupkg`
13.  pull the devcenter
14.  modify the changelog and download.json, and deploy.



##new release process
###Kinvey
2.  Update the version number in `Kinvey-Xamarin/core/KinveyHeaders.cs`
3.  Set target to `Release`, and kick off a build.
4.  Open up the __Kinvey-Xamarin__ project in finder  
5.  Copy __Kinvey-Utils/obj/Release/Kinvey-Utils.dll__, __Kinvey-Xamarin/obj/Release/Kinvey-Xamarin.dll__, and __Restsharp.Portable/obj/Release/RestSharp.portable.dll__ into the __libs__ directory
6.  Open up `Kinvey-Xamarin.nuspec`
7.  Modify version and release notes
8.  nuget pack kinvey-xamarin

###Android
1.  Open up the __Kinvey-Xamarin-Android__ project in finder
2.  Copy __/obj/release/Kinvey-Xamarin-Android.dll__ into the __libs__ directory
3.  Open up `Kinvey-Xamarin-Android.nuspec`
4.  Modify version and release notes
5.  nuget pack kinvey-xamarin-android

###iOS
1.  Open up the __Kinvey-Xamarin-iOS project in finder
2.  Copy __/obj/release/Kinvey-Xamarin-iOS.dll into the __libs__ directory
3.  Open up `Kinvey-Xamarin-iOS.nuspec`
4.  Modify version and release notes
5.  nuget pack kinvey-xamarin-ios

