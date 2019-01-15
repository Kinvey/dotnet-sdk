using System;
using System.IO;
using System.Threading.Tasks;
using Kinvey;

namespace Kinvey.Tests
{
	public class TestSetup
	{
		private Client kinveyClient;

		public const string user = "roman.ogolikhin@softteco.com";
		public const string pass = "qwertYUIOP1z";

        public const string app_key = "kid_S112cy0jX";
		public const string app_secret = "eddf74fe0d554d94b5ce856437e20b93";

		public const string app_key_fake = "abcdefg";
		public const string app_secret_fake = "0123456789abcdef";

        public const string facebook_Access_Token_Fake = "4a156f9c-d734-487f-931e-401105c2a45d";

        public static string db_dir = Environment.CurrentDirectory;
        public static string SQLiteOfflineStoreFilePath = Path.Combine(db_dir, "kinveyOffline.sqlite");
        public static string SQLiteCredentialStoreFilePath = Path.Combine(db_dir, "kinvey_tokens.sqlite");

	}
}
