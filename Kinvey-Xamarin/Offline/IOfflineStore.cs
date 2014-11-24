using System;
using SQLite.Net.Interop;

namespace KinveyXamarin
{

	/// <summary>
	/// This class defines the behaivor of an offline store, which mimics CRUD operations
	/// </summary>
	public interface IOfflineStore {

		ISQLitePlatform platform {get; set;}
		string dbpath{ get; set;}

		object executeGet<T>(AbstractKinveyClient client, AppData<T> appdata, AbstractKinveyOfflineClientRequest<T> request);

		object executeSave<T>(AbstractKinveyClient client, AppData<T> appdata, AbstractKinveyOfflineClientRequest<T> request);

		KinveyDeleteResponse executeDelete<T>(AbstractKinveyClient client, AppData<T> appdata, AbstractKinveyOfflineClientRequest<T> request);

		void insertEntity<T>(AbstractKinveyClient client, AppData<T> appdata, T entity);

		void clearStorage();
	}
}

