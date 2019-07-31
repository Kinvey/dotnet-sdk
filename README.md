<p align="left">
  <a href="https://www.progress.com/kinvey" style="display: inline-block;">
    <img src="logo-progresskinvey.png">
  </a>
</p>

# [Kinvey .NET SDK](https://devcenter.kinvey.com/dotnet)

![badge-nuget] ![badge-status] ![badge-coverage]

# Overview

[Kinvey](https://www.progress.com/kinvey) is a high-productivity serverless application development platform that provides developers tools to build robust, multi-channel applications utilizing a cloud backend and front-end SDKs. As a platform, Kinvey provides many solutions to common development needs, such as a data store, data integration, single sign-on integration, and file storage. With Kinvey, developers can focus on building what provides value for their app - the user experience (UX) and business logic of the application. This approach increases developer productivity and aims to enable higher quality apps by leveraging Kinvey's pre-built components.

# Features

The Kinvey .NET SDK repository represents the package that can be used to develop .NET and Xamarin applications on the Kinvey platform. The Kinvey SDK is developed as a .NET Standard library for various supported .NET runtimes, including Xamarin, with platform-specific features for iOS and Android.

## Contents

The following is a high-level overview of the most important projects in the solution:

* `Kinvey`: project for the .NET Standard 2.0, which contains most of the SDK functionality. This .NET Standard 2.0 project is also compatible with a majority of .NET environments. See [here](https://devcenter.kinvey.com/dotnet/guides/getting-started#PlatformCompatibility) for further details.
* `Kinvey.Android`: project which provides Android-specific functionality.
* `Kinvey.iOS`: project which provides iOS-specific functionality.
* `Kinvey.Tests`: test project for the Kinvey project.

## Using the SDK

Refer to the Kinvey [DevCenter](http://devcenter.kinvey.com/dotnet) for guides and documentation on using Kinvey.

# Releases

Versioning of the Kinvey SDK follows the guidelines stated in [Semantic Version 2.0.0](http://semver.org/).

# Build Instructions

This repository contains the solution file: `Kinvey.sln`.

Open the solution in Visual Studio. Once the solution is loaded, run `Build->Rebuild` to build the entire solution.

# Test Instructions

Build the `Kinvey.Tests` project to build all the Kinvey .NET SDK tests.

# API Reference

Docuemntation for using the Kinvey SDK as well as other parts of the Kinvey platform can be found in the Kinvey DevCenter [reference](https://devcenter.kinvey.com/dotnet/reference/) guide.

# Contribute

Feedback on our SDK is welcome and encouraged. Please, use [GitHub Issues](https://github.com/Kinvey/dotnet-sdk/issues) on this repository for reporting a bug or requesting a feature for this SDK. Please reference our [contribution guide](CONTRIBUTING.md) for more information.

We would also love to have your contributions! You may also reference our [contribution guide](CONTRIBUTING.md) for details on how to submit a pull request (PR) for this repository.

# License

See [LICENSE](LICENSE.txt) for details.

[badge-nuget]: https://img.shields.io/nuget/vpre/Kinvey.svg
[badge-status]: https://travis-ci.org/Kinvey/dotnet-sdk.svg?branch=master
[badge-coverage]: https://codecov.io/gh/Kinvey/dotnet-sdk/graph/badge.svg
