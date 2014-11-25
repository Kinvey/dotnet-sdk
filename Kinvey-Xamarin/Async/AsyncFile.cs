using System;
using System.Threading.Tasks;
using System.IO;

namespace KinveyXamarin
{
	/// <summary>
	/// Async file.  This class allows access to Kinvey's File API asynchronously.  
	/// </summary>
	public class AsyncFile : File
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.AsyncFile"/> class.
		/// </summary>
		/// <param name="client">A configured instance of a Kinvey client.</param>
		public AsyncFile (AbstractClient client) : base(client)
		{
		}

		/// <summary>
		/// Download the File associated with the id of the provided metadata.  The file is streamed into the stream, with delegates returning either errors or the FileMetaData from Kinvey.
		/// </summary>
		/// <param name="metadata">The FileMetaData representing the file to download.  This must contain an id.</param>
		/// <param name="content">Where the contents of the file will be streamed.</param>
		/// <param name="delegates">Delegates for success or failure.</param>
		public void download(FileMetaData metadata, Stream content, KinveyDelegate<FileMetaData> delegates)
		{
			Task.Run (() => {
				try {
					FileMetaData entity = base.downloadBlocking (metadata).executeAndDownloadTo (content);
					delegates.onSuccess (entity);
				} catch (Exception e) {
					delegates.onError (e);
				}
			});

		}

		/// <summary>
		/// Download the File associated with the id of the provided metadata.  The file is copied into the byte[], with delegates returning either errors or the FileMetaData from Kinvey.
		/// </summary>
		/// <param name="metadata">The FileMetaData representing the file to download.  This must contain an id.</param>
		/// <param name="content">Content.</param>
		/// <param name="delegates">Delegates for success or failure.</param>
		public void download(FileMetaData metadata, byte[] content, KinveyDelegate<FileMetaData> delegates)
		{
			Task.Run (() => {
				try {
					FileMetaData entity = base.downloadBlocking (metadata).executeAndDownloadTo (content);
					delegates.onSuccess (entity);
				} catch (Exception e) {
					delegates.onError (e);
				}
			});

		}

		/// <summary>
		/// Upload the specified byte[] to Kinvey's file storage.  The FileMetaData contains extra data about the file.
		/// </summary>
		/// <param name="metadata">Metadata associated with the file, supports arbitrary key/value pairs</param>
		/// <param name="content">the actual bytes of the file to upload.</param>
		/// <param name="delegates">Delegates for success or failure.</param>
		public void upload(FileMetaData metadata, byte[] content, KinveyDelegate<FileMetaData> delegates)
		{
			Task.Run (() => {
				try {
					FileMetaData entity = base.uploadBlocking (metadata).executeAndUploadFrom (content);
					delegates.onSuccess (entity);
				} catch (Exception e) {
					delegates.onError (e);
				}
			});

		}

		/// <summary>
		/// Uploads metadata associated with a file, without changing the file itself.  Do not modify the id or filename using this method-- it's for any other key/value pairs.
		/// </summary>
		/// <param name="metadata">The updated FileMetaData to upload to Kinvey.</param>
		/// <param name="delegates">Delegates for success or failure.</param>
		public void uploadMetadata(FileMetaData metadata, KinveyDelegate<FileMetaData> delegates)
		{
			Task.Run (() => {
				try {
					FileMetaData entity = base.uploadMetadataBlocking (metadata).Execute();
					delegates.onSuccess (entity);
				} catch (Exception e) {
					delegates.onError (e);
				}
			});

		}

		/// <summary>
		/// Downloads the metadata of a File, without actually downloading the file.
		/// </summary>
		/// <param name="fileId">The _id of the file's metadata to download. </param>
		/// <param name="delegates">Delegates for success or failure.</param>
		public void downloadMetadata(string fileId, KinveyDelegate<FileMetaData> delegates)
		{
			Task.Run (() => {
				try {
					FileMetaData entity = base.downloadMetadataBlocking (fileId).Execute();
					delegates.onSuccess (entity);
				} catch (Exception e) {
					delegates.onError (e);
				}
			});

		}

		/// <summary>
		/// Delete the specified file.
		/// </summary>
		/// <param name="fileId">The _id of the file to delete.</param>
		/// <param name="delegates">Delegates for success or failure.</param>
		public void delete(string fileId, KinveyDelegate<KinveyDeleteResponse> delegates)
		{
			Task.Run (() => {
				try {
					KinveyDeleteResponse entity = base.deleteBlocking (fileId).Execute();
					delegates.onSuccess (entity);
				} catch (Exception e) {
					delegates.onError (e);
				}
			});

		}


	}
}