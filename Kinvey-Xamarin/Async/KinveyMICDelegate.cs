using System;

namespace KinveyXamarin
{
	public class KinveyMICDelegate<T> : KinveyDelegate<T>
	{
		/// <summary>
		/// This Action is executed when the MIC login page is ready to be rendered.
		/// </summary>
		public Action<string> OnReadyToRender;
	}
}

