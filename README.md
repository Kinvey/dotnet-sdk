Kinvey Xamarin Library
======

This is a Public Class Library (PCL) for various supported Xamarin runtimes.


##DISCLAIMER

This is an alpha release of the Kinvey-Xamarin Library.  There are many wrappers that need to be added, and the current implementation requires much more verbosity than will be required.  Also, future refactoring is planned for adding support for the async/await pattern instead of spawning threads.

## Build
Pre-requisites:

* RestSharp - portable
  * Download Restsharp.portable directory from the Portable branch of the RestSharp project on Github from here: https://github.com/restsharp/RestSharp/tree/portable/RestSharp.Portable
  * Import this into Xamarin studio as an existing project
  * Set it as a reference for the Kinvey-Xamarin project (this repo)
  
  
## Usage (from android-testdrive project) 

###create a client

    AbstractClient kinveyClient = (AbstractClient)new AbstractClient.Builder (new RestClient (), new Kinvey.DotNet.Framework.Core.KinveyClientRequestInitializer (appKey, appSecret, new KinveyHeaders ())).build ();
    
    
###access user operations (login)

    try{
        user = kinveyClient.KinveyUser ().Login ().Execute();
	}catch(Exception e){
		Console.WriteLine ("Uh oh! " + e);
		RunOnUiThread (() => {
			Toast.MakeText(this, "something went wrong: " + e.Message, ToastLength.Short).Show();
		});
		return;
    }
    


###access app data operations (with cache support)


			AppData<MyEntity> entityCollection = kinveyClient.AppData<MyEntity>(COLLECTION, typeof(MyEntity));
			try{
				MyEntity res = entityCollection.GetEntity (STABLE_ID).Execute ();
			}catch(Exception e){
				Console.WriteLine ("Uh oh! " + e);
				RunOnUiThread (() => {
					Toast.MakeText(this, "something went wrong: " + e.Message, ToastLength.Short).Show();
				});
				return;
			}	


## License

    Copyright 2014 Kinvey, Inc.

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.

