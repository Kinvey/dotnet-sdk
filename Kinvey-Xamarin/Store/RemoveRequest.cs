using System;
using System.Threading.Tasks;

namespace KinveyXamarin
{
	public class RemoveRequest <T> : WriteRequest <T, KinveyDeleteResponse>
	{
		private string entityID;

		public RemoveRequest(string entityID, AbstractClient client, string collection, ICache<T> cache, ISyncQueue sync, WritePolicy policy)
			: base(client, collection, cache, sync, policy)
		{
			this.entityID = entityID;
		}

		public override async Task<KinveyDeleteResponse> ExecuteAsync()
		{
			KinveyDeleteResponse kdr = default(KinveyDeleteResponse);

			NetworkRequest<KinveyDeleteResponse> request = Client.NetworkFactory.buildDeleteRequest<KinveyDeleteResponse>(Collection, entityID);

			switch (Policy)
			{
				case WritePolicy.FORCE_LOCAL:
					// sync
					kdr = Cache.DeleteByID(entityID);
					PendingWriteAction pendingAction = PendingWriteAction.buildFromRequest(request);
					SyncQueue.Enqueue(pendingAction);
					break;

				case WritePolicy.FORCE_NETWORK:
					// network
					kdr = await request.ExecuteAsync();
					break;

				case WritePolicy.NETWORK_THEN_LOCAL:
					// cache
					kdr = Cache.DeleteByID(entityID);
					kdr = await request.ExecuteAsync();
					break;

				default:
					throw new KinveyException(EnumErrorCode.ERROR_GENERAL, "Invalid write policy");
			}

			return kdr;
		}

		public override Task<bool> Cancel()
		{
			throw new KinveyException(EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED, "Cancel method on RemoveRequest not implemented.");
		}
	}
}