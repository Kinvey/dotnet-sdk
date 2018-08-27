using System;
using System.IO;
using System.Threading.Tasks;
using Kinvey;

namespace Kinvey.Tests
{
	public class TestSetup
	{
		private Client kinveyClient;

		public const string user = "testuser";
		public const string pass = "testpass";

        public const string app_key = "kid_Zy0JOYPKkZ";
		public const string app_secret = "d83de70e64d540e49acd6cfce31415df";

		public const string app_key_fake = "abcdefg";
		public const string app_secret_fake = "0123456789abcdef";

        public static string db_dir = Environment.CurrentDirectory;
        public static string SQLiteOfflineStoreFilePath = Path.Combine(db_dir, "kinveyOffline.sqlite");
        public static string SQLiteCredentialStoreFilePath = Path.Combine(db_dir, "kinvey_tokens.sqlite");

	}
}
