﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KinveyXamarin
{
	/// <summary>
	/// Request operation for pulling all records for a collection during a sync, and refreshing the cache with the
	/// updated data.
	/// </summary>
	public class PullRequest<T> : ReadRequest<T, List<T>>
	{
		public PullRequest(AbstractClient client, string collection, ICache<T> cache, ReadPolicy policy)
			: base(client, collection, cache, policy)
		{
		}

		public override async Task<List<T>> ExecuteAsync()
		{
			List<T> listResults = default(List<T>);

			listResults = await Client.NetworkFactory.buildGetRequest<T>(Collection).ExecuteAsync();

			Cache.RefreshCache(listResults);

			return listResults;
		}

		public override Task<bool> Cancel()
		{
			throw new KinveyException(EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED, "Cancel method on PullRequest not implemented.");
		}
	}
}
