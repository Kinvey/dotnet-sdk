// Copyright (c) 2019, Kinvey, Inc. All rights reserved.
//
// This software is licensed to you under the Kinvey terms of service located at
// http://www.kinvey.com/terms-of-use. By downloading, accessing and/or using this
// software, you hereby accept such terms of service  (and any agreement referenced
// therein) and agree that you have read, understand and agree to be bound by such
// terms of service and are of legal age to agree to such terms with Kinvey.
//
// This software contains valuable confidential and proprietary information of
// KINVEY, INC and is subject to applicable licensing agreements.
// Unauthorized reproduction, transmission or distribution of this file and its
// contents is a violation of applicable laws.

using System;
namespace Kinvey
{
    /// <summary>
    /// The class for logging data.
    /// </summary>
    public static class Logger
    {
#pragma warning disable IDE1006 // Naming Styles
        /// <summary>
        /// Determines if a logger initialized.
        /// </summary>
        /// <value>If set to <c>true</c> then a logger initialized.</value>
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

        /// <summary>
        /// This action is executed when <see cref="Logger.Log(string)"/> or <see cref="Logger.Log(object)" /> or 
        /// <see cref="Logger.Log(Exception)"/> methods have been called.
        /// </summary>
        /// <value>The action having the string as the parameter.</value>
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

        /// <summary>
        /// Initializes a logger.
        /// </summary>
        /// <param name="logAction"> The action is being used for the callback pattern when some logging method was called. </param>
        [Obsolete("This method has been deprecated. Please use Initialize() instead.")]
        public static void initialize(Action<string> logAction)
        {
            Initialize(logAction);
        }
#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// Determines if a logger initialized.
        /// </summary>
        /// <value>If set to <c>true</c> then a logger initialized.</value>
        public static bool Initialized { get; set; }

        /// <summary>
        /// This action is executed when <see cref="Logger.Log(string)"/> or <see cref="Logger.Log(object)" /> or 
        /// <see cref="Logger.Log(Exception)"/> methods have been called.
        /// </summary>
        /// <value>The action having the string as the parameter.</value>
        public static Action<string> LogIt { get; set; }

        /// <summary>
        /// Initializes a logger.
        /// </summary>
        /// <param name="logAction"> The action is being used for the callback pattern when some logging method was called. </param>
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

        /// <summary>
        /// Logs a text string.
        /// </summary>
        /// <param name="message"> The text string. </param>
        public static void Log(String message)
        {
            if (!initialized)
            {
                return;
            }
            logIt(message);
        }

        /// <summary>
        /// Logs <see cref="System.Object"/> instance.
        /// </summary>
        /// <param name="message"> The object. </param>
        public static void Log(object message)
        {
            Log(message.ToString());
        }

        /// <summary>
        /// Logs <see cref="Exception"/> instance.
        /// </summary>
        /// <param name="e"> The exception. </param>
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
