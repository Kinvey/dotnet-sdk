using Newtonsoft.Json.Linq;

namespace Kinvey
{
    internal static class CustomExtension
    {
        internal static string GetValidValue(this JObject jobject, string propertyName)
        {
            var propertyValue = jobject[propertyName];
            if (propertyValue != null)
            {
                var newRefreshToken = propertyValue.ToString();
                if (!string.IsNullOrEmpty(newRefreshToken) && !newRefreshToken.ToLower().Equals("null"))
                {
                    return newRefreshToken;
                }
            }
            return null;
        }
    }
}
