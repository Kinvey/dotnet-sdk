using System;
using System.Threading.Tasks;
using System.IO;

namespace KinveyXamarin
{
	public class AsyncFile : File
	{
		public AsyncFile (AbstractClient client) : base(client)
		{
		}

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