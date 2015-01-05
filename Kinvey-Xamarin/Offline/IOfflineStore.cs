using System;
using SQLite.Net.Interop;
using System.Threading.Tasks;

namespace KinveyXamarin
{

	/// <summary>
	/// This class defines the behaivor of an offline store, which mimics CRUD operations
	/// </summary>
	public interface IOfflineStore {

		/// <summary>
		/// Gets or sets the platform.
		/// </summary>
		/// <value>The platform.</value>
		ISQLitePlatform platform {get; set;}
		/// <summary>
		/// Gets or sets the dbpath.
		/// </summary>
		/// <value>The dbpath.</value>
		string dbpath{ get; set;}

		/// <summary>
		/// Executes a get request.
		/// </summary>
		/// <returns>The response object.</returns>
		/// <param name="client">Client.</param>
		/// <param name="appdata">Appdata.</param>
		/// <param name="request">Request.</param>
		/// <typeparam name="T">The type of the response.</typeparam>
		Task<object> executeGetAsync<T>(AbstractKinveyClient client, AppData<T> appdata, AbstractKinveyOfflineClientRequest<T> request);

		/// <summary>
		/// Executes a save request.
		/// </summary>
		/// <returns>The save.</returns>
		/// <param name="client">Client.</param>
		/// <param name="appdata">Appdata.</param>
		/// <param name="request">Request.</param>
		/// <typeparam name="T">The type of the response.</typeparam>
		Task<object> executeSaveAsync<T>(AbstractKinveyClient client, AppData<T> appdata, AbstractKinveyOfflineClientRequest<T> request);

		/// <summary>
		/// Executes a delete request.
		/// </summary>
		/// <returns>The delete.</returns>
		/// <param name="client">Client.</param>
		/// <param name="appdata">Appdata.</param>
		/// <param name="request">Request.</param>
		/// <typeparam name="T">The type of the response.</typeparam>
		Task<KinveyDeleteResponse> executeDeleteAsync<T>(AbstractKinveyClient client, AppData<T> appdata, AbstractKinveyOfflineClientRequest<T> request);

		/// <summary>
		/// Inserts an entity directly into the database.
		/// </summary>
		/// <param name="client">Client.</param>
		/// <param name="appdata">Appdata.</param>
		/// <param name="entity">Entity.</param>
		/// <typeparam name="T">The type of the response.</typeparam>
		Task<int> insertEntityAsync<T>(AbstractKinveyClient client, AppData<T> appdata, T entity);



		/// <summary>
		/// Clears the storage.
		/// </summary>
		void clearStorage();
	}
}

