using System;
using System.Threading.Tasks;

namespace KinveyXamarin
{
	public class AsyncCustomEndpoint<I, O> : CustomEndpoint<I, O>
	{
		public AsyncCustomEndpoint (AbstractClient client): base(client)
		{
		}

		/// <summary>
		/// Executes the custom endpoint, expecting a single result
		/// </summary>
		/// <param name="endpoint">Endpoint name.</param>
		/// <param name="input">Input object.</param>
		/// <param name="delegates">Delegates for success or failure.</param>
		public void ExecuteCustomEndpoint(string endpoint, I input, KinveyDelegate<O> delegates)
		{
			Task.Run (() => {
				try{
					O result = base.executeCustomEndpointBlocking(endpoint, input).Execute();
					delegates.onSuccess(result);
				}catch(Exception e){
					delegates.onError(e);
				}
			});
		}

		/// <summary>
		/// Executes the custom endpoint, expecting an array of results
		/// </summary>
		/// <param name="endpoint">Endpoint name.</param>
		/// <param name="input">Input object.</param>
		/// <param name="delegates">Delegates for success or failure.</param>
		public void ExecuteCustomEndpoint(string endpoint, I input, KinveyDelegate<O[]> delegates)
		{
			Task.Run (() => {
				try{
					O[] result = base.executeCustomEndpointArrayBlocking(endpoint, input).Execute();
					delegates.onSuccess(result);
				}catch(Exception e){
					delegates.onError(e);
				}
			});
		}
	}
}

