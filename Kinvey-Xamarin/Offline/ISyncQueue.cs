using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace KinveyXamarin
{
	public interface ISyncQueue
	{
		string Collection { get; }
		Task<bool> Enqueue (PendingWriteAction pending);
		Task<List<PendingWriteAction>> GetAll ();
		Task<PendingWriteAction> GetByID(string entityID);

		Task<PendingWriteAction> Peek ();
		Task<PendingWriteAction> Pop ();

		Task<bool> Remove (string entityID);
		Task<bool> RemoveAll ();

	}
}

