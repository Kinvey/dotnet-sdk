using System;

namespace KinveyUtils
{
	public class Logger
	{
		public static bool initialized {get; set;}

		public static Action<string> logIt {get; set;}

		public static void initialize(Action<string> logAction){
			if (logAction != null) {
				logIt = logAction;
				initialized = true;
			} else {
				initialized = false;
			}
		}


		public static void Log(String message){
			if (!initialized) {
				return;
			}
			logIt (message);
		}

		public static void Log(object message){
			Log (message.ToString ());
		}


		public static void Log(Exception e){
			if (!initialized) {
				return;
			}
			logIt (e.ToString());
		}

	}
}

