using System;
using System.IO;

namespace KinveyXamarin
{
	/// <summary>
	/// The Kinvey File Delegate class is used for the callback pattern when executing file specific requests asynchronously.  All Async* File methods will take one as a parameter.
	/// </summary>
	public class KinveyFileDelegate : KinveyDelegate<FileMetaData>
	{
		/// <summary>
		/// This Action is executed when the file is downloaded.
		/// </summary>
		public Action<Stream> onDownload;


	}
}

