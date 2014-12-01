using System;

namespace KinveyXamarin
{
	/// <summary>
	/// Kinvey utility for assertions
	/// </summary>
	public class KAssert
	{

		/// <summary>
		/// Asserts that a string is not null
		/// </summary>
		/// <returns>the string, if it's not null.  If it is, an exception will be thrown.</returns>
		/// <param name="toAssert">The string to check for a null value.</param>
		/// <param name="message">the message to use in the exception if the string is null.</param>
		public static String notNull(string toAssert, string message){
			if (toAssert == null) {
				throw new ArgumentNullException (message);
			}
			return toAssert;
		}
	}
}

