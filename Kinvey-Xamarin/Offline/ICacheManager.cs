using System;
using SQLite.Net.Interop;

namespace KinveyXamarin
{
	public interface ICacheManager
	{
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
		/// Gets the cache.
		/// </summary>
		/// <returns>The cache.</returns>
		/// <param name="collectionName">Collection name.</param>
		ICache<T> GetCache <T>(string collectionName);

		/// <summary>
		/// Clears the storage.
		/// </summary>
		void clearStorage();

	}
}

