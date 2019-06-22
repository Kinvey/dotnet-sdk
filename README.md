# Kinvey .NET SDK

![badge-status] ![badge-nuget]

[Kinvey](http://www.kinvey.com) (pronounced Kin-vey, like convey) makes it ridiculously easy for developers to setup, use and operate a cloud backend for their mobile apps. You don't have to worry about connecting to various cloud services, setting up servers for your backend, or maintaining and scaling them.

The Kinvey .NET SDK repo represents the package that can be used to develop .NET and Xamarin applications on the Kinvey platform. The Kinvey SDK is developed as a .NET Standard library for various supported .NET runtimes, including Xamarin, with platform-specific features for iOS and Android.

The following is a high-level overview of the most important projects in the solution:

* `Kinvey`: project for the .NET Standard 2.0, which contains most of the SDK functionality. This .NET Standard 2.0 project is also compatible with a majority of .NET environments. [Details](http://devcenter.kinvey.com/dotnet-v3.0/guides/getting-started#PlatformCompatibility).
* `Kinvey.Android`: project which provides Android-specific functionality
* `Kinvey.iOS`: project which provides iOS-specific functionality
* `Kinvey.Tests`: test project for the Kinvey project

Refer to the Kinvey [DevCenter](http://devcenter.kinvey.com/) for documentation on using Kinvey.

## Build

This repository contains the solution file: `Kinvey.sln`.  

Open the solution in Visual Studio. Once the solution is loaded, run `Build->Rebuild` to build the entire solution.

## Test

Build the `Kinvey.Tests` project to build all the Kinvey .NET SDK tests.

## License
See [LICENSE](LICENSE.txt) for details.

## Contributing
See [CONTRIBUTING.md](CONTRIBUTING.md) for details on reporting bugs and making contributions.

[badge-status]: https://travis-ci.org/Kinvey/dotnet-sdk.svg?branch=master
[badge-nuget]: https://img.shields.io/nuget/vpre/Kinvey.svg
