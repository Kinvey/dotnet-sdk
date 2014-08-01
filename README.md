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
  * SQLite.Net.Platform.* (where * is the runtime for the current application)
  * LinqExtender
  
  
  
##Usage and Concepts

### The Client
The Client acts as the point of interaction for all things Kinvey.  It manages the current users credentials, and provides a handful of factory methods for accessing features.  For example, to access all datastore operations, use `myKinveyClient.AppData(...)` and to access user operations use `myKinveyClient.User()`.


### Async vs Sync (*Blocking)
This library is implemented with a clean separation between blocking synchronous functionality and async functionality with delegates for results.  Note that there are two versions of each factory, for example there is a `User` class containing `LoginBlocking`, and a `AsyncUser` class containing `Login`.  The blocking variation requires a call to `Execute()` on the request object, and will block the current thread until it completes.  The Async Variations will spawn a new thread which executes the blocking variation, passing through any parameters.  Async methods also take an instance of a `KinveyDelegate`, a simple abstract class which provides on `Action` for `onSuccess` and `onFailure`, dependent on the results of the async execution.  
  
  
## Usage (from android-testdrive project) 

###create a client

    Client kinveyClient = new Client.Builder(appKey, appSecret).build();
    
##### add offline to the client builder
Note the addition of two new builder methods to add the local database path location as well as a platform specific implementation of the sqlite.net pcl.

    kinveyClient = new Client.Builder(appKey, appSecret)
		.setFilePath(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal))
		.setOfflinePlatform(new SQLitePlatformAndroid()).build();

    
    
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
	
### Querying

Querying can be performed through standard LINQ operators, where an instance of `AppData` can be used as the queryable object.  The Kinvey library provides a subset of LINQ which includes most of the core functionality.  For example, you cannot perform Join operations due to the nature of a nosql datastore.  The library does support Lambda operations, as well as sorting, where clauses, and logical operators.

For example, a query to get all Entites where a field called `last name` is equal to `smith` can be written as follows:

	var query = from cust in testData
		where cust.lastname == "smith"
		select cust;
		

More complex queries are also supported, such as:

	var query = from cust in testData
		where (cust.ID == "10" && cust.Name == "Billy") || (cust.ID == "1" && cust.Name == "Charlie")
		select cust;

 

### Caching and Offline
Caching functionality is provided as in-memory implementation, meaning that cached data will only be available per use of the application.  Offline functionality utilizes SQLite to maintain a local copy of entities, as well as a queue of pending requests.  This data is persisted between uses of the application, and will perform any queued up requests once any request has been successfully completed.  

Both features are accessed through very similar methods on an Appdata instance, essentially only requiring an implementation of a `Store`, as well as a Policy to use.  See below for details about the provided caching and offline policies.

Both caching and offline can be used at the same time, allowing for very quick responses to commonly repeated requests, and a fail-over in case a network connection is lost.

####Enabling Caching and Offline

The Client.Builder will require both a reference to the local filesystem, as well as a platform specific implementation of the SQLite.Net-PCL.  For Android, the following will store the database in private local storage:

    kinveyClient = new Client.Builder(appKey, appSecret)
		.setFilePath(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal))
		.setOfflinePlatform(new SQLitePlatformAndroid()).build();



To enable caching:

	//resuse this cache object!
    InMemoryCache<MyEntity> myCache = new InMemoryCache<MyEntity>();
    
    //get a reference to AppData
    AsyncAppData<MyEntity> entityCollection = kinveyClient.AppData<MyEntity>("MyCollection", typeof(MyEntity));

	//enable caching
    entityCollection.setCache (myCache, CachePolicy.CACHE_FIRST);
    

To enable offline:

    //get a reference to AppData
    AsyncAppData<MyEntity> entityCollection = kinveyClient.AppData<MyEntity>("MyCollection", typeof(MyEntity));

	//enable offline, no need to reuse SQLiteOfflineStore, all instances share a database
    entityCollection.setOffline(new SQLiteOfflineStore<MyEntity>(), OfflinePolicy.LOCAL_FIRST);


### Cache Policies

* `CachePolicy.NOCACHE` - This policy will not use any caching, and will execute every request online.

Use this policy if your application is dependant on data that is shared between multiple users and always needs to be up to date.


* `CachePolicy.CACHEONLY` - This policy will only retrieve data from the cache, and will not use any network connection.

Use this policy in combination with another policy, to allow for quick response times without requiring a network connection for specific operations.


* `CachePolicy.CACHEFIRST` - This policy will first attempt to retrieve data from the cache.  If the data has been cached, it will be returned.  If the data does not exist in the cache, the data will be retrieved from Kinvey's Backend and the cache will be updated.

Use this policy if your application can display data that doesn't change very often but you still want local updates.


* `CachePolicy.CACHEFIRST_NOREFRESH` - This policy will first attempt to retrieve data from the cache.  If the data has been cached, it will be returned.  If the data does not exist in the cache, the data will be retrieved from Kinvey's Backend but the cache will not be updated with the new results.

Use this policy if you want to set default results, however if a request is made that cannot return these defaults a live request will be made (without modifying those default values)


* `CachePolicy.NETWORKFIRST` - This policy will execute the request on the network, and will store the result in the cache.  If the online execution fails, the results will be pulled from the cache.

Use this policy if you application wants the latest data but you still want responsiveness if a connection is lost



###Offline Policies


* `OfflinePolicy.ONLINE_FIRST` - This policy will attempt to execute the request online first, and if that is successful will update the local store with the results.  If the request fails due to connectivity issues, then the request will be executed against the local store.  If it fails for any other reason such as an Authorization Error, the onFailure callback will be called.

Use this policy if your application's data is constantly changing on the backend, but you want to support offline mode.

 
* `OfflinePolicy.LOCAL_FIRST` - This policy will attempt to execute the request against the local store first.  If the request is a Get, and the data cannot be found in the local store, then an online request will be attempted.  If that suceeds, the store will be updated, and onSuccess will be called.  If that fails, then onFailure will be called.  For save requests, the local store will be updated and the entity will be returned through the onSuccess callback.

Use this policy if each user has their own data, and updates are not constantly required from your backend.


* `OfflinePolicy.ALWAYS_ONLINE` - This policy will not use any local storage or queueing, and will execute every request online.  If no network connection is available, errors will be returned through the onFailure callback.

Use this policy if your application is fully dependant on data in the backend, and data cannot be stored locally.




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

