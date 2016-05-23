using System;

namespace KinveyXamarin
{
	public abstract class RemoveRequest <T, U> : Request <T, U>
	{
		public RemoveRequest(AbstractClient client) : base(client){}
	}
}