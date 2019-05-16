using System;
namespace Kinvey
{
    public static class Logger
    {
#pragma warning disable IDE1006 // Naming Styles
        [Obsolete("This property has been deprecated. Please use Initialized instead.")]
        public static bool initialized
        {
            get
            {
                return Initialized;
            }
            set
            {
                Initialized = value;
            }
        }

        [Obsolete("This property has been deprecated. Please use LogIt instead.")]
        public static Action<string> logIt
        {
            get
            {
                return LogIt;
            }
            set
            {
                LogIt = value;
            }
        }

        [Obsolete("This method has been deprecated. Please use Initialize() instead.")]
        public static void initialize(Action<string> logAction)
        {
            Initialize(logAction);
        }
#pragma warning restore IDE1006 // Naming Styles

        public static bool Initialized { get; set; }

        public static Action<string> LogIt { get; set; }

        public static void Initialize(Action<string> logAction)
        {
            if (logAction != null)
            {
                logIt = logAction;
                initialized = true;
            }
            else
            {
                initialized = false;
            }
        }

        public static void Log(String message)
        {
            if (!initialized)
            {
                return;
            }
            logIt(message);
        }

        public static void Log(object message)
        {
            Log(message.ToString());
        }


        public static void Log(Exception e)
        {
            if (!initialized)
            {
                return;
            }
            logIt(e.ToString());
        }

    }
}
