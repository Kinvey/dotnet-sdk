using System;

namespace KinveyUtils
{
	public class Logger
	{
		public static bool initialized {get; set;} = false;

		public static Action<string> logIt {get; set;}

		public static void initialize(Action<string> logAction){
			logIt = logAction;
			initialized = true;
		}


		public static void Log(String message){
			if (!initialized) {
				return;
			}
			logIt (message);
		}


		public static void Log(Exception e){
			if (!initialized) {
				return;
			}
			logIt (e.ToString());
		}

	}
}

