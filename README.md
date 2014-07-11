Kinvey Xamarin Library
======

This is a Public Class Library (PCL) for various supported Xamarin runtimes.


##DISCLAIMER

This is an alpha release of the Kinvey-Xamarin Library.  There are many wrappers that need to be added, and the current implementation requires much more verbosity than will be required.  Also, future refactoring is planned which might break backwards compatibility.

Please report any issues on the issues section of this project, thank you!

Also check out the index branch for current features being developed

## Build
Pre-requisites:

* take a copy of this project's source code, and add it to your solution
* Add this project as a dependency to your application
* via nu-get, install the following packages:
  * Newtonsoft.json
  * SQLite.Net-PCL
  * SQLite.Net.Platform.* where * is the runtime for the current application
  
  
  
##Usage and Concepts

### The Client
The Client acts as the point of interaction for all things Kinvey.  It manages the current users credentials, and provides a handful of factory methods for accessing features.  For example, to access all datastore operations, use `myKinveyClient.AppData(...)` and to access user operations use `myKinveyClient.User()`.


### Async vs Sync (*Blocking)
This library is implemented with a clean separation between blocking synchronous functionality and async functionality with delegates for results.  Note that there are two versions of each factory, for example there is a `User` class containing `LoginBlocking`, and a `AsyncUser` class containing `Login`.  The blocking variation requires a call to `Execute()` on the request object, and will block the current thread until it completes.  The Async Variations will spawn a new thread which executes the blocking variation, passing through any parameters.  Async methods also take an instance of a `KinveyDelegate`, a simple abstract class which provides on `Action` for `onSuccess` and `onFailure`, dependent on the results of the async execution.  
  
  
## Usage (from android-testdrive project) 

###create a client

    Client kinveyClient = new Client.Builder(appKey, appSecret).build();
    
##### add offline to the client builder
Note the addition of two new builder methods (which will be refactored and simplified) to add the local database path location as well as a platform specific implementation of the sqlite.net pcl.

    kinveyClient = new Client.Builder(appKey, appSecret)
                   .setFilePath(Android.OS.Environment.ExternalStorageDirectory.ToString ())
                   .setOfflinePlatform(new SQLitePlatformAndroid())
                   .build();

    
    
###access user operations async (login)

	kinveyClient.User ().Login (new KinveyDelegate<User>{ 
		onSuccess =  (user) => { 
			RunOnUiThread (() => {
				Toast.MakeText(this, "logged in as: " + user.Id, ToastLength.Short).Show();
			});
		},
		onError = (error) => {
			RunOnUiThread (() => {
				Toast.MakeText(this, "something went wrong: " + error.Message, ToastLength.Short).Show();
			});
		}
	});
    


###access app data operations (save with cache support)


	//get a reference to AsyncAppData
	AsyncAppData<MyEntity> entityCollection = kinveyClient.AppData<MyEntity>(COLLECTION, typeof(MyEntity));

	//Create an entity to save	
	MyEntity ent = new MyEntity();
	ent.ID = STABLE_ID;
	ent.Email = "test@tester.com";
	ent.Name = "James Dean";
	
	//-----
	//This line here enables Caching, used for retrieving only
	entityCollection.setCache (myCache, CachePolicy.CACHE_FIRST);
	//-----
	
	
	//call save and pass delegates
	entityCollection.Save (ent, new KinveyDelegate<MyEntity> { 
		onSuccess = (entity) => { 
			RunOnUiThread (() => {
				Toast.MakeText (this, "logged in as: " + entity.Name, ToastLength.Short).Show ();
			});
		},
		onError = (error) => {
			RunOnUiThread (() => {
				Toast.MakeText (this, "something went wrong: " + error.Message, ToastLength.Short).Show ();
			});
		}
	});


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

