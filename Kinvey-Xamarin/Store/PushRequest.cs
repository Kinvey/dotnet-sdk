using System;
using System.Threading.Tasks;
using System.Collections.Generic;
namespace KinveyXamarin
{
	public class PushRequest <T> : WriteRequest<T, DataStoreResponse>
	{
		public PushRequest (AbstractClient client, string collection,  ICache<T> cache, ISyncQueue queue) : base (client, collection, cache, queue){
			
		}

		public override async Task <DataStoreResponse> ExecuteAsync ()
		{
			List<PendingWriteAction> pendingActions = await SyncQueue.GetAll ();
			DataStoreResponse response = new DataStoreResponse ();
			foreach (PendingWriteAction action in pendingActions) {
				try {
					NetworkRequest<T> request = action.toNetworkRequest<T> (this.Client);	
					await request.ExecuteAsync();
					response.Count++;
				} catch (Exception e){
					//Do nothing for now
					response.addError(new KinveyJsonError());	//TODO
				}
			}
			return response;

		}

		 public override Task<bool> Cancel() {
			throw new Exception ("not implemented");
		}

	}
}

