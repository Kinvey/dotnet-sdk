using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Globalization;
using System.Diagnostics;

namespace Kinvey.Tests
{
    public abstract class BaseTestClass
    {
        public static Client.Builder ClientBuilder
        {
            get
            {
                return new Client.Builder(AppKey, AppSecret);
            }
        }

        public static Client.Builder ClientBuilderFake
        {
            get
            {
                return new Client.Builder("_fake_", "fake");
            }
        }

        public static string AppKey
        {
            get
            {
                return EnvironmentVariable.AppKey ?? "_kid_";
            }
        }

        public static string AppSecret
        {
            get
            {
                return EnvironmentVariable.AppSecret ?? "appSecret";
            }
        }


        public static bool MockData
        {
            get
            {
                return string.IsNullOrEmpty(EnvironmentVariable.AppKey) && string.IsNullOrEmpty(EnvironmentVariable.AppSecret);
            }
        }

        public static class EnvironmentVariable
        {
            public static string AppKey => Environment.GetEnvironmentVariable("KINVEY_APP_KEY");

            public static string AppSecret => Environment.GetEnvironmentVariable("KINVEY_APP_SECRET");
        }

        private static readonly string REQUEST_START_HEADER = "X-Kinvey-Request-Start";
        private static readonly string DATE_FORMAT = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffK";

        protected static HttpListener httpListener;

        public void Delete(string fileName)
        {
            var fileInfo = new FileInfo(fileName);
            while (fileInfo.Exists)
            {
                try
                {
                    fileInfo.Delete();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    fileInfo.Refresh();
                }
            }
        }

        private void Logout()
        {
            try
            {
                if (Client._sharedClient != null)
                {
                    using (var client = Client._sharedClient)
                    {
                        var user = client.ActiveUser;
                        if (user != null)
                        {
                            user.Logout();
                        }
                    }
                }
            }
            finally
            {
                Client.SharedClient = null;
            }
        }

        [TestInitialize]
        public virtual void Setup()
        {
            Logout();

            Delete(TestSetup.SQLiteOfflineStoreFilePath);
            Delete(TestSetup.SQLiteCredentialStoreFilePath);

            if (httpListener != null && httpListener.IsListening)
            {
                httpListener.Close();
            }
        }

        [TestCleanup]
        public virtual void Tear()
        {
            Logout();

            Delete(TestSetup.SQLiteOfflineStoreFilePath);
            Delete(TestSetup.SQLiteCredentialStoreFilePath);

            if (httpListener != null && httpListener.IsListening)
            {
                httpListener.Close();
            }
        }

        protected static void MockUserLogin(HttpListenerContext context, IEnumerable<JObject> users, IDictionary<string, JObject> signedUsers)
        {
            Assert.AreEqual(context.Request.HttpMethod, "POST");
            var json = Read<JObject>(context);
            if (json.TryGetValue("_socialIdentity", out JToken socialIdentityJson))
            {
                Assert.AreEqual(socialIdentityJson.Children().Count(), 1);
                var socialIdentityJsonChild = socialIdentityJson.First();
                Assert.AreEqual(socialIdentityJsonChild.Children().Count(), 1);
                var accessToken = default(string);

                switch (socialIdentityJsonChild.Path)
                {
                    case "_socialIdentity.kinveyAuth":
                        accessToken = socialIdentityJsonChild.First().Value<string>("access_token");
                        Assert.IsNotNull(accessToken);
                        break;
                    case "_socialIdentity.facebook":
                        accessToken = socialIdentityJsonChild.First().Value<string>("access_token");
                        Assert.IsNotNull(accessToken);
                        break;
                    case "_socialIdentity.google":
                        accessToken = socialIdentityJsonChild.First().Value<string>("access_token");
                        Assert.IsNotNull(accessToken);
                        break;
                    case "_socialIdentity.twitter":
                        accessToken = socialIdentityJsonChild.First().Value<string>("access_token");
                        Assert.IsNotNull(accessToken);
                        Assert.IsNotNull(socialIdentityJsonChild.First().Value<string>("access_token_secret"));
                        Assert.IsNotNull(socialIdentityJsonChild.First().Value<string>("consumer_key"));
                        Assert.IsNotNull(socialIdentityJsonChild.First().Value<string>("consumer_secret"));
                        break;
                    case "_socialIdentity.linkedin":
                        accessToken = socialIdentityJsonChild.First().Value<string>("access_token");
                        Assert.IsNotNull(accessToken);
                        Assert.IsNotNull(socialIdentityJsonChild.First().Value<string>("access_token_secret"));
                        Assert.IsNotNull(socialIdentityJsonChild.First().Value<string>("consumer_key"));
                        Assert.IsNotNull(socialIdentityJsonChild.First().Value<string>("consumer_secret"));
                        break;
                    case "_socialIdentity.salesforce":
                        accessToken = socialIdentityJsonChild.First().Value<string>("access_token");
                        Assert.IsNotNull(accessToken);
                        Assert.IsNotNull(socialIdentityJsonChild.First().Value<string>("refresh_token"));
                        Assert.IsNotNull(socialIdentityJsonChild.First().Value<string>("id"));
                        Assert.IsNotNull(socialIdentityJsonChild.First().Value<string>("client_id"));
                        break;
                    default:
                        Assert.Fail("Incorrect json data.");
                        break;
                }

                if (signedUsers.ContainsKey(accessToken))
                {
                    var responseJson = signedUsers[accessToken];

                    var authtoken = accessToken.Equals(TestSetup.access_token_for_401_response_fake) ? TestSetup.auth_token_for_401_response_fake : Guid.NewGuid().ToString();

                    responseJson["_kmd"] = new JObject
                    {
                        ["authtoken"] = authtoken
                    };

                    Write(context, responseJson);
                }              
                else
                {
                    Assert.Fail("Incorrect access token.");
                }
            }
            else
            {
                var username = json["username"];
                var password = json["password"];
                var user = users.SingleOrDefault(x =>
                {
                    return username.Equals(x["username"]) && password.Equals(x["password"]);
                });

                if (user != null)
                {
                    if (user["username"].ToString().Equals(TestSetup.user_without_permissions) && user["password"].ToString().Equals(TestSetup.pass_for_user_without_permissions))
                    {
                        user["_kmd"] = new JObject
                        {
                            ["authtoken"] = TestSetup.auth_token_for_401_response_fake
                        };
                    }
                    else
                    {
                        user["_kmd"] = new JObject
                        {
                            ["authtoken"] = Guid.NewGuid().ToString()
                        };
                    }

                    var clone = new JObject(user);
                    clone.Remove("password");

                    Write(context, clone);
                }
                else
                {
                    context.Response.StatusCode = 401;
                    Write(context, new
                    {
                        error = "InvalidCredentials",
                        description = "Invalid credentials. Please retry your request with correct credentials.",
                        debug = "",
                    });
                }
            }
        }

        protected static void Write(HttpListenerContext context, byte[] data)
        {
            using (var outputStream = context.Response.OutputStream)
            {
                outputStream.Write(data, 0, data.Length);
            }
        }

        protected static void Write(HttpListenerContext context, string str)
        {
            using (var outputWriter = new StreamWriter(context.Response.OutputStream))
            {
                outputWriter.Write(str);
            }
        }

        protected static void Write(HttpListenerContext context, object jsonObject)
        {
            context.Response.Headers["Content-Type"] = "application/json; charset=utf-8";
            Write(context, JsonConvert.SerializeObject(jsonObject));
        }

        protected static T Read<T>(HttpListenerContext context)
        {
            using (var streamReader = new StreamReader(context.Request.InputStream))
            {
                return JsonConvert.DeserializeObject<T>(streamReader.ReadToEnd());
            }
        }

        protected static byte[] Read(HttpListenerContext context)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var inputStream = context.Request.InputStream)
                {
                    inputStream.CopyTo(memoryStream);
                }
                return memoryStream.ToArray();
            }
        }

        protected static void MockNotFound(HttpListenerContext context)
        {
            var response = context.Response;
            response.StatusCode = 404;
            Write(context, "Not Found");
        }

        private static bool Filter(JObject item, string key, JToken jToken)
        {
            switch (jToken.Type)
            {
                case JTokenType.Array:
                    {
                        var results = new List<bool>();
                        foreach (var queryItem in jToken)
                        {
                            var result = Filter(item, null, queryItem);
                            results.Add(result);
                        }
                        if ("$or".Equals(key))
                        {
                            return results.Aggregate((b1, b2) => b1 || b2);
                        }
                        else
                        {
                            return results.Aggregate((b1, b2) => b1 && b2);
                        }
                    }
                case JTokenType.Object:
                    {
                        var results = new List<bool>();
                        foreach (var property in jToken)
                        {
                            var result = Filter(item, key, property);
                            results.Add(result);
                        }
                        return results.Aggregate((b1, b2) => b1 && b2);
                    }
                case JTokenType.Property:
                    {
                        var property = jToken.Value<JProperty>();
                        switch (property.Name)
                        {
                            case "$regex":
                                var regex = new Regex(property.Value.Value<string>());
                                return regex.IsMatch(item[key].Value<string>());
                            case "$gt":
                                return property.Value.Value<IComparable>().CompareTo(item[key].Value<IComparable>()) < 0;
                            case "$gte":
                                return property.Value.Value<IComparable>().CompareTo(item[key].Value<IComparable>()) <= 0;
                            case "$lt":
                                return property.Value.Value<IComparable>().CompareTo(item[key].Value<IComparable>()) > 0;
                            case "$lte":
                                return property.Value.Value<IComparable>().CompareTo(item[key].Value<IComparable>()) >= 0;
                            default:
                                return Filter(item, property.Name, property.Value);
                        }
                    }
                default:
                    return item[key].Value<object>().Equals(jToken);
            }
        }

        private static bool Filter(JObject item, KeyValuePair<string, JToken> queryItem)
        {
            return Filter(item, queryItem.Key, queryItem.Value);
        }

        private static IEnumerable<JObject> Filter(IEnumerable<JObject> objects, KeyValuePair<string, JToken> queryItem)
        {
            return objects.Where((item) => Filter(item, queryItem));
        }

        protected static void MockAppDataPost(HttpListenerContext context, List<JObject> items, Client client)
        {
            var obj = Read<JObject>(context);
            MockAppDataPost(context, obj, items, client);
        }

        protected static void MockAppDataPost(HttpListenerContext context, JObject obj, List<JObject> items, Client client)
        {
            if (obj["_id"] == null || obj["_id"].Type == JTokenType.Null)
            {
                obj["_id"] = Guid.NewGuid().ToString();
            }

            if (!(obj["_acl"] is JObject acl))
            {
                acl = new JObject();
                obj["_acl"] = acl;
            }
            acl["creator"] = client.ActiveUser.Id;

            var kmd = new JObject();
            var date = DateTime.UtcNow.ToString(DATE_FORMAT);
            kmd["ect"] = date;
            kmd["lmt"] = date;
            obj["_kmd"] = kmd;

            items.Add(obj);
            Write(context, obj);
        }

        protected static void MockAppDataGet(HttpListenerContext context, List<JObject> items, Client client)
        {
            var results = FilterByQuery(context, items, client);
            Write(context, results);
        }

        protected static void MockAppDataDelete(HttpListenerContext context, List<JObject> items, Client client)
        {
            var results = FilterByQuery(context, items, client);
            var resultsIds = results.Select(item => item["_id"].Value<string>());
            var count = items.RemoveAll(obj=> resultsIds.Contains(obj["_id"].Value<string>()));

            var jsonObject = new JObject
            {
                ["count"] = count
            };

            Write(context, jsonObject);
        }

        protected static IEnumerable<JObject> FilterByQuery(HttpListenerContext context, List<JObject> items, Client client)
        {
            var queryItems = HttpUtility.ParseQueryString(context.Request.Url.Query);
            var results = items as IEnumerable<JObject>;
            var query = queryItems["query"];
            if (query != null)
            {
                var queryObj = JsonConvert.DeserializeObject<JObject>(query);
                foreach (var queryItem in queryObj)
                {
                    results = Filter(results, queryItem);
                }
            }
            var sort = queryItems["sort"];
            if (sort != null)
            {
                var sortObject = JsonConvert.DeserializeObject<Dictionary<string, int>>(sort);
                foreach (var sortKeyValue in sortObject)
                {
                    if (sortKeyValue.Value > 0)
                    {
                        results = results.OrderBy((x) => x[sortKeyValue.Key]);
                    }
                    else
                    {
                        results = results.OrderByDescending((x) => x[sortKeyValue.Key]);
                    }
                }
            }
            var skip = queryItems["skip"];
            if (skip != null)
            {
                var skipValue = int.Parse(skip);
                results = results.Skip(skipValue);
            }
            var limit = queryItems["limit"];
            if (limit != null)
            {
                var limitValue = int.Parse(limit);
                results = results.Take(limitValue);
            }
            var fields = queryItems["fields"];
            if (fields != null)
            {
                var fieldsArray = fields.Split(",");
                results = results.Select((item) =>
                {
                    var _obj = new JObject();
                    foreach (var field in fieldsArray)
                    {
                        _obj[field] = item[field];
                    }
                    return _obj;
                });
            }
            AddRequestStartHeader(context);

            return results;
        }

            protected static void MockAppDataPut(HttpListenerContext context, List<JObject> items, string id, Client client)
        {
            var obj = Read<JObject>(context);
            var index = items.FindIndex((x) => id.Equals(x["_id"].Value<string>()));

            if(index == -1)
            {
                MockAppDataPost(context, obj, items, client);
                return;
            }

            var item = items[index];
            obj["_id"] = id;
            var acl = obj["_acl"];
            if (acl == null || acl.Type == JTokenType.Null)
            {
                obj["_acl"] = acl;
            }

            obj["_kmd"] = item["_kmd"];
            var kmd = obj["_kmd"];
            kmd["lmt"] = DateTime.UtcNow.ToString(DATE_FORMAT);

            items[index] = obj;
            Write(context, obj);
        }

        protected static void MockAppData(HttpListenerContext context, List<JObject> items, Client client)
        {
            switch (context.Request.HttpMethod)
            {
                case "GET":
                    MockAppDataGet(context, items, client);
                    break;
                case "POST":
                    MockAppDataPost(context, items, client);
                    break;
                case "DELETE":
                    MockAppDataDelete(context, items, client);
                    break;
                default:
                    Assert.Fail(context.Request.RawUrl);
                    MockNotFound(context);
                    break;
            }
        }

        protected static void AddRequestStartHeader(HttpListenerContext context)
        {
            context.Response.Headers[REQUEST_START_HEADER] = DateTime.UtcNow.ToString(DATE_FORMAT);
        }

        protected static void MockAppDataGroup(HttpListenerContext context, IEnumerable<JObject> items)
        {
            Assert.AreEqual("POST", context.Request.HttpMethod);
            var json = Read<JObject>(context);
            var key = json["key"].Value<JObject>();
            var initial = json["initial"].Value<JObject>();
            var reduce = json["reduce"].Value<string>();
            var condition = json["condition"]?.Value<JObject>();

            if (condition != null)
            {
                foreach (var filter in condition)
                {
                    items = items.Where((x) => {
                        return x[filter.Key].Equals(filter.Value);
                    });
                }
            }

            var avgRegex = new Regex(@"function\(doc,out\){ out.result = \(\(\(out.result \* out.count\) \+ doc\.([^\s()-]*)\) / \(out.count \+= 1\)\); }");
            var avgMatch = avgRegex.Match(reduce);
            if (avgMatch != null && avgMatch.Groups.Count == 2)
            {
                var field = avgMatch.Groups[1].Value;
                var results = items.Select((x) => x[field].Value<int>());
                var obj = new
                {
                    result = results.Average(),
                    count = results.Count(),
                };
                Write(context, new object[] { obj });
                return;
            }

            var maxRegex = new Regex(@"function\(doc,out\){ out.result = Math\.max\(out.result, doc\.([^\s()-]*)\); }");
            var maxMatch = maxRegex.Match(reduce);
            if (maxMatch != null && maxMatch.Groups.Count == 2)
            {
                var field = maxMatch.Groups[1].Value;
                var results = items.Select((x) => x[field].Value<int>());
                var obj = new
                {
                    result = results.Max(),
                };
                Write(context, new object[] { obj });
                return;
            }

            var minRegex = new Regex(@"function\(doc,out\){ out.result = Math\.min\(out.result, doc\.([^\s()-]*)\); }");
            var minMatch = minRegex.Match(reduce);
            if (minMatch != null && minMatch.Groups.Count == 2)
            {
                var field = minMatch.Groups[1].Value;
                var results = items.Select((x) => x[field].Value<int>());
                var obj = new
                {
                    result = results.Min(),
                };
                Write(context, new object[] { obj });
                return;
            }

            var sumRegex = new Regex(@"function\(doc,out\){ out.result \+= doc\.([^\s()-]*); }");
            var sumMatch = sumRegex.Match(reduce);
            if (sumMatch != null && sumMatch.Groups.Count == 2)
            {
                var field = sumMatch.Groups[1].Value;

                items = items.Select((x) => {
                    var obj = new JObject();
                    foreach (var _key in key)
                    {
                        if (_key.Value.Value<bool>())
                        {
                            obj[_key.Key] = x[_key.Key];
                        }
                    }
                    obj[field] = x[field];
                    return obj;
                });
                var groupBy = items.GroupBy(x => {
                    var obj = new JObject();
                    foreach (var _key in key)
                    {
                        if (_key.Value.Value<bool>())
                        {
                            obj[_key.Key] = x[_key.Key];
                        }
                    }
                    return JsonConvert.SerializeObject(obj);
                });
                items = groupBy.Select(x => {
                    var obj = JsonConvert.DeserializeObject<JObject>(x.Key);
                    obj["result"] = x.Sum(a => a[field].Value<int>());
                    return obj;
                });
                Write(context, items);
                return;
            }

            MockNotFound(context);
        }

        protected static void MockAppDataDelete(HttpListenerContext context, List<JObject> items, List<JObject> deletedItems, string id)
        {
            var todoIndex = items.FindIndex((obj) => id.Equals(obj["_id"].Value<string>()));
            var jsonObject = new JObject();
            if (todoIndex != -1)
            {
                jsonObject["count"] = 1;
                var deletedItem = items[todoIndex];
                items.RemoveAt(todoIndex);
                deletedItem["_kmd"]["lmt"] = DateTime.UtcNow.ToString(DATE_FORMAT);
                deletedItems.Add(deletedItem);
            }
            else
            {
                jsonObject["count"] = 0;
            }
            Write(context, jsonObject);
        }

        protected static void MockAppDataDeltaSet(HttpListenerContext context, IEnumerable<JObject> items, IEnumerable<JObject> deletedItems)
        {
            var since = context.Request.QueryString["since"];
            var changed = items.Where(item => {
                var lmt = item["_kmd"]["lmt"].Value<string>();
                return string.Compare(lmt, since, StringComparison.InvariantCulture) > 0;
            });
            var deleted = deletedItems.Where(item => {
                var lmt = item["_kmd"]["lmt"].Value<string>();
                return string.Compare(lmt, since, StringComparison.InvariantCulture) > 0;
            });
            AddRequestStartHeader(context);
            Write(context, new { changed, deleted });
        }

        protected static void MockResponses(uint? expectedRequests = null, Client client = null)
        {
            if (httpListener != null && httpListener.IsListening)
            {
                httpListener.Close();
            }
            httpListener = new HttpListener();
            if (client == null)
            {
                client = Client.SharedClient;
            }
            var appKey = (client.RequestInitializer as KinveyClientRequestInitializer).AppKey;
            var appSecret = (client.RequestInitializer as KinveyClientRequestInitializer).AppSecret;
            var appKeySecretAuthorization = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{appKey}:{appSecret}"))}";
            httpListener.Prefixes.Add(client.BaseUrl);
            if (new Uri(client.MICHostName).Scheme.ToLower().Equals("http"))
            {
                httpListener.Prefixes.Add(client.MICHostName);
            }
            httpListener.Start();
            var thread = new Thread(new ThreadStart(() => {
                try
                {
                    #region Existing users

                    var users = new Dictionary<string, JObject>();

                    var testUserId = Guid.NewGuid().ToString();
                    users[testUserId] = new JObject
                    {
                        ["_id"] = testUserId,
                        ["username"] = TestSetup.user,
                        ["password"] = TestSetup.pass,
                        ["email"] = $"{Guid.NewGuid().ToString()}@kinvey.com",
                        ["_acl"] = new JObject()
                        {
                            ["creator"] = testUserId,
                        },
                    };

                    var userId1 = Guid.NewGuid().ToString();
                    users[userId1] = new JObject
                    {
                        ["_id"] = userId1,
                        ["username"] = Guid.NewGuid().ToString(),
                        ["password"] = Guid.NewGuid().ToString(),
                        ["email"] = $"{Guid.NewGuid().ToString()}@kinvey.com",
                        ["first_name"] = "George",
                    };

                    var userId2 = Guid.NewGuid().ToString();
                    users[userId2] = new JObject
                    {
                        ["_id"] = userId2,
                        ["username"] = Guid.NewGuid().ToString(),
                        ["password"] = Guid.NewGuid().ToString(),
                        ["email"] = $"{Guid.NewGuid().ToString()}@kinvey.com",
                        ["first_name"] = "George",
                    };

                    var userId3 = Guid.NewGuid().ToString();
                    users[userId3] = new JObject
                    {
                        ["_id"] = userId3,
                        ["username"] = Guid.NewGuid().ToString(),
                        ["password"] = Guid.NewGuid().ToString(),
                        ["email"] = $"{Guid.NewGuid().ToString()}@kinvey.com",
                        ["first_name"] = "George",
                    };

                    var deletedSoftUserId = "5808de04e87d27107142f686";
                    users[deletedSoftUserId] = new JObject
                    {
                        ["_id"] = deletedSoftUserId,
                        ["username"] = Guid.NewGuid().ToString(),
                        ["password"] = Guid.NewGuid().ToString(),
                        ["email"] = $"{Guid.NewGuid().ToString()}@kinvey.com",
                        ["_kmd"] = new JObject()
                        {
                            ["status"] = new JObject()
                            {
                                ["val"] = "disabled",
                                ["lastChange"] = DateTime.UtcNow.ToString(DATE_FORMAT),
                            }
                        }
                    };

                    var userWithoutPermissionsId = Guid.NewGuid().ToString();
                    users[userWithoutPermissionsId] = new JObject
                    {
                        ["_id"] = userWithoutPermissionsId,
                        ["username"] = TestSetup.user_without_permissions,
                        ["password"] = TestSetup.pass_for_user_without_permissions,
                        ["email"] = $"{Guid.NewGuid().ToString()}@kinvey.com",
                        ["_acl"] = new JObject()
                        {
                            ["creator"] = userWithoutPermissionsId,
                        },
                    };

                    #endregion Existing users

                    #region Social networks users

                    var signedUsers = new Dictionary<string, JObject>();

                    signedUsers[TestSetup.facebook_access_token_fake] = new JObject
                    {
                        ["_id"] = Guid.NewGuid().ToString(),
                        ["username"] = "facebook_user",
                        ["_socialIdentity"] = new JObject()
                        {
                            ["facebook"] = new JObject()
                            {
                                ["data"] = Guid.NewGuid().ToString(),
                            },
                        },
                    };

                    signedUsers[TestSetup.google_access_token_fake] = new JObject
                    {
                        ["_id"] = Guid.NewGuid().ToString(),
                        ["username"] = "google_user",
                        ["_socialIdentity"] = new JObject()
                        {
                            ["google"] = new JObject()
                            {
                                ["data"] = Guid.NewGuid().ToString(),
                            },
                        },
                    };

                    signedUsers[TestSetup.twitter_access_token_fake] = new JObject
                    {
                        ["_id"] = Guid.NewGuid().ToString(),
                        ["username"] = "twitter_user",
                        ["_socialIdentity"] = new JObject()
                        {
                            ["twitter"] = new JObject()
                            {
                                ["data"] = Guid.NewGuid().ToString(),
                            },
                        },
                    };

                    signedUsers[TestSetup.linkedin_access_token_fake] = new JObject
                    {
                        ["_id"] = Guid.NewGuid().ToString(),
                        ["username"] = "linkedin_user",
                        ["_socialIdentity"] = new JObject()
                        {
                            ["linkedin"] = new JObject()
                            {
                                ["data"] = Guid.NewGuid().ToString(),
                            },
                        },
                    };

                    signedUsers[TestSetup.salesforce_access_token_fake] = new JObject
                    {
                        ["_id"] = Guid.NewGuid().ToString(),
                        ["username"] = "salesforce_user",
                        ["_socialIdentity"] = new JObject()
                        {
                            ["salesforce"] = new JObject()
                            {
                                ["data"] = Guid.NewGuid().ToString(),
                            },
                        },
                    };
                    #endregion Social networks users

                    #region MIC services

                    var micServices = new Dictionary<string, IEnumerable<JObject>>();
                    var micId1 = "_kid_" + Constants.MIC_ID_SEPARATOR + TestSetup.mic_id_fake;                                       
                    var micUser1 = new JObject
                    {
                        ["_id"] = Guid.NewGuid().ToString(),
                        ["username"] = "test",
                        ["password"] = "test"
                    };

                    var micUser2 =  new JObject
                    {
                        ["_id"] = Guid.NewGuid().ToString(),
                        ["username"] = Guid.NewGuid().ToString(),
                        ["password"] = Guid.NewGuid().ToString()
                    };

                    var micUser3 = new JObject
                    {
                        ["_id"] = Guid.NewGuid().ToString(),
                        ["username"] = "test3",
                        ["password"] = "test3"
                    };

                    micServices[micId1] = new List<JObject> { micUser1, micUser2, micUser3 };

                    var micId2 = "_kid_" + Constants.MIC_ID_SEPARATOR + "46a68b99c6284c32b4ab7d54328a06ab";
                    var micUser4 = new JObject
                    {
                        ["_id"] = Guid.NewGuid().ToString(),
                        ["username"] = Guid.NewGuid().ToString(),
                        ["password"] = Guid.NewGuid().ToString()
                    };

                    var micUser5 = new JObject
                    {
                        ["_id"] = Guid.NewGuid().ToString(),
                        ["username"] = Guid.NewGuid().ToString(),
                        ["password"] = Guid.NewGuid().ToString()
                    };

                    micServices[micId2] = new List<JObject> { micUser4, micUser5 };

                    #endregion MIC services

                    var blobs = new Dictionary<string, JObject>();
                    var files = new Dictionary<string, byte[]>();
                    var memory = new
                    {
                        Todo = new List<JObject>(),
                        Person = new List<JObject>(),
                        FlashCard = new List<JObject>(),
                    };
                    var deleted = new
                    {
                        Todo = new List<JObject>(),
                        Person = new List<JObject>(),
                        FlashCard = new List<JObject>(),
                    };
                    var count = 0u;
                    while (
                        (expectedRequests == null && httpListener.IsListening) ||
                        (expectedRequests != null && count < expectedRequests)
                    ) {
                        HttpListenerContext context;
                        try
                        {
                            context = httpListener.GetContext();
                        }
                        catch (HttpListenerException)
                        {
                            continue;
                        }

                        count++;
                        Console.WriteLine($"{count}");

                        var authorization = context.Request.Headers["Authorization"];
                        if (!context.Request.Url.LocalPath.StartsWith("/_uploadURL/", StringComparison.Ordinal) && !context.Request.Url.LocalPath.StartsWith("/_downloadURL/", StringComparison.Ordinal))
                        {
                            Assert.IsNotNull(authorization);
                            Assert.IsFalse(string.IsNullOrEmpty(authorization));
                            if (authorization.Contains(TestSetup.auth_token_for_401_response_fake, StringComparison.Ordinal))
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                                Write(context, "Unauthorized");
                                continue;
                            }
                        }
                        switch (context.Request.Url.LocalPath)
                        {
                            case "/rpc/_kid_/custom/test_bad":
                                Assert.AreEqual("POST", context.Request.HttpMethod);
                                MockNotFound(context);
                                break;
                            case "/rpc/_kid_/check-username-exists":
                                MockCheckUsernameExists(context, users.Values);
                                break;
                            case "/rpc/_kid_/user-forgot-username":
                                context.Response.StatusCode = 204;
                                Write(context, "");
                                break;
                            case "/rpc/_kid_/custom/test":
                                {
                                    var reader = new StreamReader(context.Request.InputStream);
                                    var json = JsonConvert.DeserializeObject<Dictionary<string, int>>(reader.ReadToEnd());
                                    var input = json["input"];
                                    Assert.AreEqual(input, 1);
                                    var response = context.Response;
                                    var jsonObject = new List<Dictionary<string, int>>()
                                        {
                                            new Dictionary<string, int>() { { "due_date", 2 } },
                                            new Dictionary<string, int>() { { "due_date", 3 } },
                                        };
                                    var jsonString = JsonConvert.SerializeObject(jsonObject);
                                    var writer = new StreamWriter(response.OutputStream);
                                    writer.Write(jsonString);
                                    writer.Close();
                                    break;
                                }
                            case "/appdata/_kid_":
                                Assert.AreEqual("GET", context.Request.HttpMethod);
                                Assert.AreEqual(appKeySecretAuthorization, authorization);
                                Write(context, new
                                {
                                    kinvey = "hello Kinvey",
                                    version = "1.0.0",
                                });
                                break;
                            case "/appdata/_fake_":
                            case "/user/_fake_":
                            case "/user/_fake_/":
                            case "/user/_fake_/login":
                                context.Response.StatusCode = 404;
                                Write(context, new
                                {
                                    error = "This app backend not found.",
                                });
                                break;
                            case "/user/_kid_/_lookup":
                                MockUserLookup(context, users.Values);
                                break;
                            case "/user/_kid_/login":
                                MockUserLogin(context, users.Values, signedUsers);
                                break;
                            case "/user/_kid_":
                            case "/user/_kid_/":
                                MockUserSignUp(context, users);
                                break;
                            case "/oauth/token":
                                MockOauthToken(context, micServices, signedUsers);
                                break;                               
                            case "/appdata/_kid_/person":
                                MockAppData(context, memory.Person, client);
                                break;
                            case "/appdata/_kid_/person/_group":
                                MockAppDataGroup(context, memory.Person);
                                break;
                            case "/appdata/_kid_/ToDos":
                                MockAppData(context, memory.Todo, client);
                                break;
                            case "/appdata/_kid_/FlashCard":
                                MockAppData(context, memory.FlashCard, client);
                                break;
                            case "/appdata/_kid_/FlashCard/_deltaset":
                                MockAppDataDeltaSet(context, memory.FlashCard, deleted.FlashCard);
                                break;
                            case "/appdata/_kid_/ToDos/_count":
                                {
                                    Assert.AreEqual("GET", context.Request.HttpMethod);
                                    var results = FilterByQuery(context, memory.Todo, client);
                                    var jsonObject = new JObject();
                                    jsonObject["count"] = results.Count();
                                    Write(context, jsonObject);
                                    break;
                                }
                            case "/appdata/_kid_/ToDos/_subscribe":
                                {
                                    Assert.AreEqual("POST", context.Request.HttpMethod);
                                    var jsonObject = new JObject();
                                    Write(context, jsonObject);
                                    break;
                                }
                            case "/blob/_kid_/":
                                MockBlob(context, blobs);
                                break;
                            case "/push/_kid_/register-device":
                            case "/push/_kid_/unregister-device":
                                Assert.AreEqual("POST", context.Request.HttpMethod);

                                var pushObj = Read<JObject>(context);
                                var platform = pushObj["platform"].ToString();
                                var deviceId = pushObj["deviceId"].ToString();
                                var service = pushObj["service"].ToString();                               

                                if (string.IsNullOrEmpty(platform) || string.IsNullOrEmpty(deviceId) || (platform.Equals("android") && !service.Equals("firebase")) || (platform.Equals("ios") && !string.IsNullOrEmpty(service)))
                                {
                                    string content = null;
                                    Write(context, content);                                    
                                }
                                else
                                {
                                    Write(context, pushObj);
                                }

                                break;
                            default:
                                {
                                    var regex = new Regex(@"/([^/]*)/([^/]*)/([^/]*)/?([^/]*)");
                                    var match = regex.Match(context.Request.RawUrl);
                                    if (match != null && match.Groups.Count == 5 && match.Groups[1].Value.Equals("appdata") && match.Groups[2].Value.Equals("_kid_"))
                                    {
                                        var collectionName = match.Groups[3].Value;
                                        var id = match.Groups[4].Value;
                                        Assert.IsFalse(id.StartsWith("_"));
                                        List<JObject> items;
                                        List<JObject> deletedItems;
                                        switch (collectionName)
                                        {
                                            case "ToDos":
                                                items = memory.Todo;
                                                deletedItems = deleted.Todo;
                                                break;
                                            case "person":
                                                items = memory.Person;
                                                deletedItems = deleted.Person;
                                                break;
                                            case "FlashCard":
                                                items = memory.FlashCard;
                                                deletedItems = deleted.FlashCard;
                                                break;
                                            default:
                                                items = null;
                                                deletedItems = null;
                                                break;
                                        }
                                        Assert.IsNotNull(items);
                                        Assert.IsNotNull(deletedItems);
                                        switch (context.Request.HttpMethod)
                                        {
                                            case "DELETE":
                                                MockAppDataDelete(context, items, deletedItems, id);
                                                break;
                                            case "GET":
                                                {
                                                    var item = items.Find((obj) => id.Equals(obj["_id"].Value<string>()));
                                                    Write(context, item);
                                                }
                                                break;
                                            case "PUT":
                                                MockAppDataPut(context, items, id, client);
                                                break;
                                            default:
                                                Assert.Fail(context.Request.RawUrl);
                                                MockNotFound(context);
                                                break;
                                        }
                                    }
                                    else if (match != null && match.Groups.Count == 5 && match.Groups[1].Value.Equals("_uploadURL") && match.Groups[2].Value.Equals("_kid_"))
                                    {
                                        var id = match.Groups[3].Value;
                                        switch (context.Request.HttpMethod)
                                        {
                                            case "PUT":
                                                files[id] = Read(context);
                                                blobs[id]["_downloadURL"] = $"http://localhost:8080/_downloadURL/_kid_/{id}";
                                                Write(context, "");
                                                break;
                                            default:
                                                Assert.Fail(context.Request.RawUrl);
                                                MockNotFound(context);
                                                break;
                                        }
                                    }
                                    else if (match != null && match.Groups.Count == 5 && match.Groups[1].Value.Equals("_downloadURL") && match.Groups[2].Value.Equals("_kid_"))
                                    {
                                        var id = match.Groups[3].Value;
                                        switch (context.Request.HttpMethod)
                                        {
                                            case "GET":
                                                Write(context, files[id]);
                                                break;
                                            default:
                                                Assert.Fail(context.Request.RawUrl);
                                                MockNotFound(context);
                                                break;
                                        }
                                    }
                                    else if (match != null && match.Groups.Count == 5 && match.Groups[1].Value.Equals("blob") && match.Groups[2].Value.Equals("_kid_"))
                                    {
                                        var id = match.Groups[3].Value;
                                        switch (context.Request.HttpMethod)
                                        {
                                            case "GET":
                                                Write(context, blobs[id]);
                                                break;
                                            case "PUT":
                                                MockBlobPut(context, blobs, id);
                                                break;
                                            default:
                                                Assert.Fail(context.Request.RawUrl);
                                                MockNotFound(context);
                                                break;
                                        }
                                    }
                                    else if (match != null && match.Groups.Count == 5 && match.Groups[1].Value.Equals("user") && match.Groups[2].Value.Equals("_kid_") && match.Groups[4].Value.Equals("register-realtime"))
                                    {
                                        var id = match.Groups[3].Value;
                                        switch (context.Request.HttpMethod)
                                        {
                                            case "POST":
                                                Write(context, new JObject()
                                                {
                                                    ["subscribeKey"] = Guid.NewGuid().ToString(),
                                                    ["publishKey"] = Guid.NewGuid().ToString(),
                                                    ["userChannelGroup"] = Guid.NewGuid().ToString(),
                                                });
                                                break;
                                            default:
                                                Assert.Fail(context.Request.RawUrl);
                                                MockNotFound(context);
                                                break;
                                        }
                                    }
                                    else if (match != null && match.Groups.Count == 5 && match.Groups[1].Value.Equals("user") && match.Groups[2].Value.Equals("_kid_") && match.Groups[4].Value.Equals("unregister-realtime"))
                                    {
                                        var id = match.Groups[3].Value;
                                        switch (context.Request.HttpMethod)
                                        {
                                            case "POST":
                                                Write(context, "");
                                                break;
                                            default:
                                                Assert.Fail(context.Request.RawUrl);
                                                MockNotFound(context);
                                                break;
                                        }
                                    }
                                    else if (match != null && match.Groups.Count == 5 && match.Groups[1].Value.Equals("user") && match.Groups[2].Value.Equals("_kid_"))
                                    {
                                        var id = match.Groups[3].Value;
                                        switch (context.Request.HttpMethod)
                                        {
                                            case "GET":
                                                if (users.ContainsKey(id))
                                                {
                                                    Write(context, users[id]);
                                                }
                                                else
                                                {
                                                    MockNotFound(context);
                                                }
                                                
                                                break;
                                            case "PUT":
                                                MockUserUpdate(context, users, id);
                                                break;
                                            case "DELETE":
                                                MockUserDelete(context, users, id);
                                                break;                                                
                                            default:
                                                Assert.Fail(context.Request.RawUrl);
                                                MockNotFound(context);
                                                break;
                                        }
                                    }
                                    else if (match != null && match.Groups.Count == 5 && match.Groups[1].Value.Equals("rpc") && match.Groups[2].Value.Equals("_kid_") && match.Groups[4].Value.Equals("user-password-reset-initiate"))
                                    {
                                        var email = Uri.UnescapeDataString(match.Groups[3].Value);
                                        switch (context.Request.HttpMethod)
                                        {
                                            case "POST":
                                                context.Response.StatusCode = 204;
                                                Write(context, "");
                                                break;
                                            default:
                                                Assert.Fail(context.Request.RawUrl);
                                                MockNotFound(context);
                                                break;
                                        }
                                    }
                                    else if (match != null && match.Groups.Count == 5 && match.Groups[1].Value.Equals("rpc") && match.Groups[2].Value.Equals("_kid_") && match.Groups[4].Value.Equals("user-email-verification-initiate"))
                                    {                                       
                                        switch (context.Request.HttpMethod)
                                        {
                                            case "POST":
                                                var user = users[match.Groups[3].ToString()];
                                                Write(context, user);
                                                break;
                                            default:
                                                Assert.Fail(context.Request.RawUrl);
                                                MockNotFound(context);
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        Assert.Fail(context.Request.RawUrl);
                                        MockNotFound(context);
                                    }
                                    break;
                                }
                        }
                    }
                }
                finally
                {
                    if (expectedRequests == null && httpListener != null && httpListener.IsListening)
                    {
                        httpListener.Stop();
                    }
                }
            }))
            {
                Name = "HttpListener"
            };
            thread.Start();
        }

        private static void MockUserUpdate(HttpListenerContext context, Dictionary<string, JObject> users, string id)
        {
            var user = Read<JObject>(context);
            users[id] = user;
            Write(context, user);
        }

        private static void MockUserDelete(HttpListenerContext context, Dictionary<string, JObject> users, string query)
        {
            var id = query.Split('?')[0];
            var isRemoved = users.Remove(id);
            var kinveyDeleteResponse = new KinveyDeleteResponse
            {
                count = isRemoved ? 1 : 0
            };
            Write(context, kinveyDeleteResponse);
        }

        private static void MockUserLookup(HttpListenerContext context, IEnumerable<JObject> users)
        {
            Assert.AreEqual("POST", context.Request.HttpMethod);
            var lookup = Read<JObject>(context);
            users = users.Where(x =>
            {
                return lookup["first_name"].Equals(x["first_name"]);
            }).Select(x =>
            {
                return new JObject()
                {
                    ["_id"] = x["_id"],
                    ["username"] = x["username"],
                    ["_acl"] = x["_acl"],
                    ["email"] = x["email"],
                };
            });
            Write(context, users);
        }

        private static void MockUserSignUp(HttpListenerContext context, Dictionary<string, JObject> users)
        {
            Assert.AreEqual("POST", context.Request.HttpMethod);
            var user = Read<JObject>(context) ?? new JObject();

            var userId = Guid.NewGuid().ToString();
            user["_id"] = userId;
            user["_kmd"] = new JObject
            {
                ["authtoken"] = Guid.NewGuid().ToString()
            };

            users[userId] = user;

            Write(context, user);
        }

        private static void MockOauthToken(HttpListenerContext context, IDictionary<string, IEnumerable<JObject>> micServices, IDictionary<string, JObject> signedUsers)
        {
            Assert.AreEqual("POST", context.Request.HttpMethod);
            var data = Read(context);
            var micData = Encoding.Default.GetString(data);
            var micUserData = micData.Split('&');

            var splitedMicData = new Dictionary<string, string>();

            foreach (var micUserItem in micUserData)
            {
                var item = micUserItem.Split('=');
                splitedMicData.Add(item[0], item[1]);
            }
                      
            var clientId = splitedMicData["client_id"];
            var noMicServiceIdProvided = clientId == "_kid_";

            if (micServices.ContainsKey(clientId) || noMicServiceIdProvided)
            {
                string accessToken;
                var existingUsers = noMicServiceIdProvided ? micServices[micServices.Keys.FirstOrDefault()] : micServices[clientId];

                if (splitedMicData["grant_type"] == "refresh_token")
                {
                    if (splitedMicData["refresh_token"].Equals(TestSetup.refresh_token_for_401_response_fake))
                    {
                        accessToken = TestSetup.access_token_for_401_response_fake;
                    }
                    else
                    {
                        signedUsers.Remove(TestSetup.access_token_for_401_response_fake);

                        var existingUser = existingUsers.FirstOrDefault(x => x["username"].ToString().Equals("test3") && x["password"].ToString().Equals("test3"));

                        Assert.IsNotNull(existingUser);

                        accessToken = Guid.NewGuid().ToString();
                        signedUsers.Add(accessToken, existingUser);
                    }                   
                }
                else
                {
                    var micUserName = splitedMicData["username"];
                    var micPassword = splitedMicData["password"];

                    var existingUser = existingUsers.FirstOrDefault(x => micUserName.Equals(x["username"].ToString()) && micPassword.Equals(x["password"].ToString()));

                    Assert.IsNotNull(existingUser);

                    accessToken = micUserName.Equals("test3") && micPassword.Equals("test3") ? TestSetup.access_token_for_401_response_fake : Guid.NewGuid().ToString();
                    signedUsers.Add(accessToken, existingUser);
                }

                var responseJson = new JObject
                {
                    ["access_token"] = accessToken,
                    ["refresh_token"] = Guid.NewGuid().ToString()
                };

                Write(context, responseJson);
            }
            else
            {
                Assert.Fail("A user not found.");
            }
            
        }

        private static void MockCheckUsernameExists(HttpListenerContext context, IEnumerable<JObject> users)
        {
            var json = Read<JObject>(context);
            var username = json["username"];
            var user = users.SingleOrDefault(x => username.Equals(x["username"]));
            Write(context, new
            {
                usernameExists = user != null && user.Type != JTokenType.Null
            });
        }

        private static void MockBlob(HttpListenerContext context, Dictionary<string, JObject> blobs)
        {
            switch (context.Request.HttpMethod)
            {
                case "POST":
                    MockBlobPost(context, blobs);
                    break;
                default:
                    Assert.Fail(context.Request.HttpMethod);
                    MockNotFound(context);
                    break;
            }
        }

        private static void MockBlobPost(HttpListenerContext context, Dictionary<string, JObject> blobs)
        {
            MockBlobPut(context, blobs, Guid.NewGuid().ToString());
        }

        private static void MockBlobPut(HttpListenerContext context, Dictionary<string, JObject> blobs, string id)
        {
            var blob = Read<JObject>(context);
            blob["_id"] = id;
            if (blob["_filename"] == null || blob["_filename"].Type == JTokenType.Null)
            {
                blob["_filename"] = Guid.NewGuid().ToString();
            }
            blob["_uploadURL"] = $"http://localhost:8080/_uploadURL/_kid_/{id}";
            blob["_requiredHeaders"] = new JObject();
            blobs[id] = blob;
            Write(context, blob);
        }
    }
}
