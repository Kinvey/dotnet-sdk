using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KinveyXamarin
{
	public class FindRequest<T> : ReadRequest<T, List<T>>
	{
		private List<string> entityIDs { get; }
		private KinveyQuery<T> Query { get; }

		public FindRequest(AbstractClient client, string collection, ICache<T> cache, ReadPolicy policy, List<string> listIDs, KinveyQuery<T> queryObj)
			: base(client, collection, cache, policy)
		{
			entityIDs = listIDs;
			Query = queryObj;
		}

		public override async Task<List<T>> ExecuteAsync()
		{
			List<T> listResult = default(List<T>);

			switch (Policy)
			{
				case ReadPolicy.FORCE_LOCAL:
					// sync
					if (entityIDs?.Count > 0)
					{
						listResult = Cache.FindByIDs(entityIDs);
					}
					else if (Query != null)
					{
						// TODO VRG implement
					}
					else
					{
						return Cache.FindAll();
					}
					break;

				case ReadPolicy.FORCE_NETWORK:
					// network
					if (entityIDs?.Count > 0)
					{
						listResult = new List<T>();

						foreach (string entityID in entityIDs)
						{
							T item = await Client.NetworkFactory.buildGetByIDRequest<T>(Collection, entityID).ExecuteAsync();
							listResult.Add(item);
						}
					}
					else if (Query != null)
					{
						// TODO implement
					}
					else
					{
						listResult = await Client.NetworkFactory.buildGetRequest<T>(Collection).ExecuteAsync();
					}
					break;

				case ReadPolicy.BOTH:
					// cache
					if (entityIDs?.Count > 0)
					{
						// TODO VRG implement
					}
					else if (Query != null)
					{
						// TODO VRG implement
					}
					else
					{
						// TODO VRG implement
					}
					break;

				default:
					throw new KinveyException(EnumErrorCode.ERROR_GENERAL, "Invalid read policy");
			}

			return listResult;
		}

		public override async Task<bool> Cancel()
		{
			throw new KinveyException(EnumErrorCode.ERROR_METHOD_NOT_IMPLEMENTED, "Cancel method on FindRequest not implemented.");
		}
	}
}

