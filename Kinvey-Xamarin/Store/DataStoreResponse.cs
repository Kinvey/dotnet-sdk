using System;
using System.Collections.Generic;

namespace KinveyXamarin
{
	public class DataStoreResponse
	{

		public List<KinveyJsonError> Errors { get; private set; }
		public List<KinveyException> KinveyExceptions { get; private set;}
	
		public int Count { get ; internal set; }

		public DataStoreResponse () {
			Errors = new List<KinveyJsonError> ();
			KinveyExceptions = new List<KinveyException> ();
		}

		internal void addKinveyException (KinveyException e) {
			KinveyExceptions.Add (e);
		}

		internal void addError (KinveyJsonError error){
			Errors.Add (error);
		}



	}
}

