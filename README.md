# Kinvey .NET SDK

![badge-status] ![badge-nuget]

[Kinvey](http://www.kinvey.com) (pronounced Kin-vey, like convey) makes it ridiculously easy for developers to setup, use and operate a cloud backend for their mobile apps. You don't have to worry about connecting to various cloud services, setting up servers for your backend, or maintaining and scaling them.

The Kinvey .NET SDK repo represents the packages that can be used to develop .NET and Xamarin applications on the Kinvey platform. The Kinvey SDK is developed as a Portable Class Library (PCL) for various supported .NET runtimes, including Xamarin. Platform-specific features for iOS and Android are supported through the Xamarin-iOS and Xamarin-Android packages respectively.

The following is a high-level overview of the most important projects in the solution:

* `Kinvey-Xamarin`: project for the PCL, which contains most of the SDK functionality. This PCL project is also compatible with a majority of .NET environments. [Details](http://devcenter.kinvey.com/dotnet-v3.0/guides/getting-started#PlatformCompatibility).
* `Kinvey-Xamarin-Android`: project which provides Android-specific functionality
* `Kinvey-Xamarin-iOS`: project which provides iOS-specific functionality
* `UnitTestFramework`: test project for the Kinvey-Xamarin project

Refer to the Kinvey [DevCenter](http://devcenter.kinvey.com/) for documentation on using Kinvey.

## Build

This repository contains the solution file: `Kinvey-Xamarin.sln`.  

For .NET open the solution in Visual Studio and use the PCL project `Kinvey-Xamarin`.
For Xamarin, open the solution in Xamarin Studio. Once the solution is loaded, run `Build->Rebuild All` to build the entire solution.

## Test

Build the `UnitTestFramework` project to build all the Kinvey Xamarin SDK tests, which can then be run from Xamarin Studio Unit Tests window.

We are working on adding support to run the unit tests in Visual Studio.

## License
See [LICENSE](LICENSE.txt) for details.

## Contributing
See [CONTRIBUTING.md](CONTRIBUTING.md) for details on reporting bugs and making contributions.

[badge-status]: https://travis-ci.org/Kinvey/dotnet-sdk.svg?branch=develop
[badge-nuget]: https://img.shields.io/nuget/vpre/Kinvey.svg
