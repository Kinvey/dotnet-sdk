using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace KinveyXamarin
{
	public interface ISyncQueue
	{
		string Collection { get; }
		Task<int> Enqueue (PendingWriteAction pending);
		Task<List<PendingWriteAction>> GetAll ();
		Task<PendingWriteAction> GetByID(string entityId);

		Task<PendingWriteAction> Peek ();
		Task<PendingWriteAction> Pop ();

		Task<int> Remove (string entityId);
		Task<int> RemoveAll ();

	}
}

