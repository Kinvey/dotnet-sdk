using System;
using Kinvey.DotNet.Framework;
using Kinvey.DotNet.Framework.Core;
using System.Threading.Tasks;

namespace KinveyXamarin
{


	public class AsyncAppData<T> : AppData<T>
	{
		public AsyncAppData (string collectionName, Type myClass, AbstractClient client): base(collectionName, myClass, client)
		{
		}
			
		public void GetEntity(string entityId, KinveyDelegate<T> delegates)
		{
			Task.Factory.StartNew (() => {
				try {
					T entity = base.GetEntityBlocking (entityId).Execute ();
					delegates.onSuccess (entity);
				} catch (Exception e) {
					delegates.onError (e);
				}
			});

		}
			
		public void Get(KinveyDelegate<T> delegates)
		{
			Task.Factory.StartNew (() => {
				try {
					T entity = base.GetBlocking ().Execute ();
					delegates.onSuccess (entity);
				} catch (Exception e) {
					delegates.onError (e);
				}
			});
		}

		public void Save(T entity, KinveyDelegate<T> delegates)
		{
			Task.Factory.StartNew (() => {
				try {
					T saved = base.SaveBlocking (entity).Execute ();
					delegates.onSuccess (saved);
				} catch (Exception e) {
					delegates.onError (e);
				}
			});
		}
	}
}

