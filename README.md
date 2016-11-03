# Kinvey Xamarin SDK

The Kinvey Xamarin SDK is a package that can be used to develop Xamarin applications on the Kinvey platform. The Kinvey Xamarin SDK is a Public Class Library (PCL) for various supported Xamarin runtimes which provides a bulk of the features, as well as a package for Xamarin-iOS and a package for Xamarin-Android for platform-specific features.

The following is a high-level overview of the most important projects in the solution:

* `Kinvey-Xamarin`: project for the PCL, which contains most of the SDK functionality
* `Kinvey-Xamarin-Android`: project which provides specific Android functionality
* `Kinvey-Xamarin-iOS`: project which provides specific iOS functionality
* `UnitTestFramework`: test project for the Kinvey-Xamarin project

Refer to the Kinvey [DevCenter](http://devcenter.kinvey.com/) for documentation on using Kinvey.

## Build
This repository contains a solution file (`Kinvey-Xamarin.sln`) that is compatible with Xamarin Studio.  Open Xamarin Studio, and then open this solution.  Once the solution is loaded, run `Build->Rebuild All` to build the entire solution.

## Test
Build the `UnitTestFramework` project to build all the Kinvey Xamarin SDK tests, which can then be run from Xamarin Studio Unit Tests window.

## License
See [LICENSE.txt](LICENSE) for details.

## Contributing
See [CONTRIBUTING.md](CONTRIBUTING.md) for details on reporting bugs and making contributions.
