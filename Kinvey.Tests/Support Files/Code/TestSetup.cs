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

        public const string user_without_permissions = "testuserwithoutpermissions";
        public const string pass_for_user_without_permissions = "testuserwithoutpermissions";

        public const string user_with_corrupted_auth_token = "userwithcorruptedauthtoken";
        public const string pass_for_user_with_corrupted_auth_token = "userwithcorruptedauthtoken";

        public const string app_key = "kid_Zy0JOYPKkZ";
		public const string app_secret = "d83de70e64d540e49acd6cfce31415df";

		public const string app_key_fake = "abcdefg";
		public const string app_secret_fake = "0123456789abcdef";

        public const string facebook_access_token_fake = "2422694a-37ed-4664-b290-55b5672e48a3";
        public const string google_access_token_fake = "de2bc80e-145d-458b-b9d8-2f695df8f3a5";
        public const string twitter_access_token_fake = "aee41d86-c738-4372-9fdd-9ff8494bc416";
        public const string linkedin_access_token_fake = "1bcd22f6-2bf8-4227-9272-98e1c4cb6e15";
        public const string salesforce_access_token_fake = "8cd71dc8-846a-49f2-8cf1-c0598bbce5ec";
        public const string access_token_for_401_response_fake = "0065cb37-a1ed-4c8b-98fc-91c312683275";
        public const string auth_token_for_401_response_fake = "eda5d4bc-6a47-46d2-9637-07dec479bf9c";
        public const string auth_token_insufficient_credentials_for_401_response_fake = "acc16614-35e0-4a98-b84a-c03dadfa8463";
        public const string auth_token_corrupted_for_401_response_fake = "f7991119-f9e6-4c6e-9b38-9659c2b06cea";
        public const string refresh_token_for_401_response_fake = "0f550503-f033-44ee-8c2d-ae8f9773b70a";

        public const string entity_name_for_400_response_error = "Entity name for 400 response error";
        public const string entity_name_for_403_response_error = "Entity name for 403 response error";
        public const string entity_name_for_500_response_error = "Entity name for 500 response error";

        public const string mic_id_fake = "ade8db71f61c46a69c91910d8fbf3994";

        public static string db_dir = Environment.CurrentDirectory;
        public static string SQLiteOfflineStoreFilePath = Path.Combine(db_dir, "kinveyOffline.sqlite");
        public static string SQLiteCredentialStoreFilePath = Path.Combine(db_dir, "kinvey_tokens.sqlite");

        public const string id_for_400_error_response_fake = "612d66fc-c975-4729-afe1-e8f7750782a5";
        public const string id_for_403_error_response_fake = "2bf79cc2-b289-4699-af10-a0868ce733c4";
        public const string id_for_500_error_response_fake = "9a547181-63f9-4f7e-b789-d8c4f07889b8";
    }
}
