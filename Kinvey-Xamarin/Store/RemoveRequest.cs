// Copyright (c) 2016, Kinvey, Inc. All rights reserved.
//
// This software is licensed to you under the Kinvey terms of service located at
// http://www.kinvey.com/terms-of-use. By downloading, accessing and/or using this
// software, you hereby accept such terms of service  (and any agreement referenced
// therein) and agree that you have read, understand and agree to be bound by such
// terms of service and are of legal age to agree to such terms with Kinvey.
//
// This software contains valuable confidential and proprietary information of
// KINVEY, INC and is subject to applicable licensing agreements.
// Unauthorized reproduction, transmission or distribution of this file and its
// contents is a violation of applicable laws.

using System;
using System.Threading.Tasks;

namespace Kinvey
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
					throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_GENERAL, "Invalid write policy");
			}

			return kdr;
		}

		public override Task<bool> Cancel()
		{
			throw new KinveyException(EnumErrorCategory.ERROR_GENERAL, EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED, "Cancel method on RemoveRequest not implemented.");
		}
	}
}
