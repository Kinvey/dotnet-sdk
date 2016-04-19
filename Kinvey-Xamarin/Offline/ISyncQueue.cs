using System;
using System.Collections.Generic;

namespace KinveyXamarin
{
	public interface ISyncQueue
	{
		bool Enqueue (PendingOperation pending);
		List<PendingOperation> GetAll ();
		List<PendingOperation> GetByCollection (string collection);
		PendingOperation GetByID(string entityID);

		PendingOperation Peek ();
		PendingOperation Pop ();

		bool Remove (string entityID);
		bool RemoveAll ();

	}
}

