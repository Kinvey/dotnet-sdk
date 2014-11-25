using System;

namespace KinveyXamarin
{

	/// <summary>
	/// The Kinvey Delegate class is used for the callback pattern when executing requests asynchronously.  All Async* methods will take one as a parameter.
	/// </summary>
	public class KinveyDelegate<T>
	{
		/// <summary>
		/// This Action is executed when an asynchronously operation completes successfully.  T represents the expected response type.
		/// </summary>
		public Action<T> onSuccess;
		/// <summary>
		/// This Action is executed when an error occurs, either on the device itself, or returned from the service.
		/// </summary>
		public Action<Exception> onError;

	}
}

