// Copyright (c) 2017, Kinvey, Inc. All rights reserved.
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
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Kinvey;

namespace TestFramework
{
    [TestFixture]
    public class FileUnitTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void Tear()
        {
            System.IO.File.Delete(TestSetup.SQLiteOfflineStoreFilePath);
            System.IO.File.Delete(TestSetup.SQLiteCredentialStoreFilePath);
        }

        [Test]
        public async Task TestFileCheckResumableStatus()
        {
            // Arrange
            var moqRestClient = new Mock<RestSharp.IRestClient>();

            var response = new Mock<System.Net.WebResponse>();
            //response.Setup(r => (r as System.Net.HttpWebResponse).StatusCode).Returns((System.Net.HttpStatusCode)308);
            var innerException = new System.Net.WebException("MOCK EXCEPTION", null, System.Net.WebExceptionStatus.ProtocolError, response.Object);
            var ex = new Exception("MOCK RESUMABLE EXCEPTION", innerException);
            moqRestClient.Setup(m => m.ExecuteAsync(It.IsAny<RestSharp.IRestRequest>())).ThrowsAsync(ex);

            Client.Builder cb = new Client.Builder(TestSetup.app_key, TestSetup.app_secret)
				.SetRestClient(moqRestClient.Object);

            Client c = cb.Build();

            // Act
            var kfr = new KinveyFileRequest(c, null, null, null, null);
            int startByte = await kfr.CheckResumableStateAsync("https://www.fakeurl.edu", 1000);

            // Assert
            Assert.That(startByte == 0);
        }

        [Test]
        public void TestDetermineStartByteFromRange()
        {
            // Arrange
            string rangeHeader = "bytes=0-42";
            int startByte = 0;

            // Act
            startByte = KinveyFileRequest.DetermineStartByteFromRange(rangeHeader);

            // Assert
            Assert.That(startByte == 43);
        }
    }
}
