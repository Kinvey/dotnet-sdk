using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace KinveyXamarin
{
	public interface ISyncQueue
	{
		string Collection { get; }
		int Enqueue (PendingWriteAction pending);
		List<PendingWriteAction> GetAll ();
		PendingWriteAction GetByID(string entityId);

		PendingWriteAction Peek ();
		PendingWriteAction Pop ();

		int Remove (string entityId);
		int RemoveAll ();

	}
}

