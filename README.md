Kinvey Xamarin Library
======

This is a Public Class Library (PCL) for various supported Xamarin runtimes.


##Release Process

1.  Ensure tests are passing
2.  Set target to `Release`, and kick off a build.
3.  Copy __Kinvey-Utils/obj/Release/Kinvey-Utils.dll__, __Kinvey-Xamarin/obj/Release/Kinvey-Xamarin.dll__, and __Restsharp.Portable/obj/Release/RestSharp.portable.dll__ into the __release/kinvey-xamarin-x.x__ directory
4.  Zip up the __release/kinvey-xamarin-x.x.__ directory
5.  Check everything into github, and tag a release.
6.  Upload the zip to amazon
7.  on windows, do a git pull
8.  Ensure the project builds on windows
9.  run `nuget pack Kinvey-Xamarin.csproj -IncludeReferencedProjects`
10. Edit the file `Kinvey-Xamarin.nuspec` to up the version number and changelog (this is an xml file) 
11.  run `nuget push Kinvey-Xamarin.nupkg`
12.  pull the devcenter
13.  modify the changelog and download.json, and deploy.