using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace KinveyXamarin
{
	public interface ISyncQueue
	{
		string Collection { get; }

		int Enqueue (PendingWriteAction pending);
		PendingWriteAction Peek ();
		PendingWriteAction Pop ();

		List<PendingWriteAction> GetAll ();
		List<PendingWriteAction> GetFirstN(int limit, int offset);
		PendingWriteAction GetByID(string entityId);

		int Remove (string entityId);
		int RemoveAll ();
	}
}

