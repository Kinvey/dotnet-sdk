// Copyright (c) 2016, Kinvey, Inc. All rights reserved.
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
using System.IO;
using System.Threading.Tasks;
using Kinvey;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kinvey.Tests
{
	[TestClass]
    public class FileIntegrationTests : BaseTestClass
	{
		private Client kinveyClient;

		private static string image_name = "TestFileUploadImage.png";
        private static string image_dir = Path.Combine(Environment.CurrentDirectory, "../../../../TestFramework/TestSupportFiles");
        private static string image_path = Path.Combine(image_dir, image_name);

		private static string downloadByteArrayFilePath = image_dir + "downloadByteArrayTest.png";
		private static string downloadStreamFilePath = image_dir + "downloadStreamTest.png";

        [TestInitialize]
        public override void Setup()
		{
            base.Setup();

			Client.Builder builder = ClientBuilder;
            if (MockData) builder.setBaseURL("http://localhost:8080");
			kinveyClient = builder.Build();
		}


        [TestCleanup]
        public override void Tear()
		{
			System.IO.File.Delete(downloadByteArrayFilePath);
			System.IO.File.Delete(downloadStreamFilePath);
			kinveyClient.ActiveUser?.Logout();

            base.Tear();
		}

		[TestMethod]
		public async Task TestFileUploadByteAsync()
		{
			// Setup
            if (MockData) MockResponses(4);
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			FileMetaData fileMetaData = new FileMetaData();
			fileMetaData.fileName = image_name;
			bool publicAccess = true;
			fileMetaData._public = publicAccess;
			byte[] content = System.IO.File.ReadAllBytes(image_path);
			int contentSize = (content.Length) * sizeof(byte);
			fileMetaData.size = contentSize;

			// Act
			FileMetaData fmd = await kinveyClient.File().uploadAsync(fileMetaData, content);

			// Assert
			Assert.IsNotNull(fmd);
			Assert.AreEqual(contentSize, fmd.size);
            Assert.IsFalse(string.IsNullOrEmpty(fmd.uploadUrl));
			Assert.AreEqual(publicAccess, fmd._public);

			// Teardown
			kinveyClient.ActiveUser.Logout();
		}

        [TestMethod]
		public async Task TestFileUploadByteSharedClientAsync()
		{
			// Setup
            if (MockData) MockResponses(4);
			await User.LoginAsync(TestSetup.user, TestSetup.pass);

			// Arrange
			FileMetaData fileMetaData = new FileMetaData();
			fileMetaData.fileName = image_name;
			bool publicAccess = true;
			fileMetaData._public = publicAccess;
			byte[] content = System.IO.File.ReadAllBytes(image_path);
			int contentSize = (content.Length) * sizeof(byte);
			fileMetaData.size = contentSize;

			// Act
			FileMetaData fmd = await Client.SharedClient.File().uploadAsync(fileMetaData, content);

			// Assert
			Assert.IsNotNull(fmd);
			Assert.AreEqual(contentSize, fmd.size);
            Assert.IsFalse(string.IsNullOrEmpty(fmd.uploadUrl));
			Assert.AreEqual(publicAccess, fmd._public);

			// Teardown
			Client.SharedClient.ActiveUser.Logout();
		}

        [TestMethod]
		public async Task TestFileUploadStreamAsync()
		{
			// Setup
            if (MockData) MockResponses(4);
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			FileMetaData fileMetaData = new FileMetaData();
			fileMetaData.fileName = image_name;
			bool publicAccess = true;
			fileMetaData._public = publicAccess;
			byte[] content = System.IO.File.ReadAllBytes(image_path);
			int contentSize = (content.Length) * sizeof(byte);
			fileMetaData.size = contentSize;

			MemoryStream streamContent = new MemoryStream(content);

			// Act
			FileMetaData fmd = await kinveyClient.File().uploadAsync(fileMetaData, streamContent);

			// Assert
			Assert.IsNotNull(fmd);
			Assert.AreEqual(contentSize, fmd.size);
            Assert.IsFalse(string.IsNullOrEmpty(fmd.uploadUrl));
			Assert.AreEqual(publicAccess, fmd._public);

			// Teardown
			kinveyClient.ActiveUser.Logout();
		}

        [TestMethod]
		public async Task TestFileUploadAsyncBad()
		{
			// Setup
            if (MockData) MockResponses(1);
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			FileMetaData fileMetaData = null;
			byte[] content = null;

			// Act
			// Assert
            await Assert.ThrowsExceptionAsync<NullReferenceException>(async delegate ()
			{
				await kinveyClient.File().uploadAsync(fileMetaData, content);
			});

			// Teardown
			kinveyClient.ActiveUser.Logout();
		}

        [TestMethod]
		public async Task TestFileUploadMetadataAsync()
		{
			// Setup
            if (MockData) MockResponses(5);
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			FileMetaData fileMetaData = new FileMetaData();
			fileMetaData.fileName = image_name;
			bool publicAccess = true;
			fileMetaData._public = publicAccess;
			byte[] content = System.IO.File.ReadAllBytes(image_path);
			int contentSize = (content.Length) * sizeof(byte);
			fileMetaData.size = contentSize;

			FileMetaData fmd = await kinveyClient.File().uploadAsync(fileMetaData, content);

			fmd._public = !publicAccess;
			fmd.fileName = "test.png";

			// Act
			FileMetaData fmdUpdate = await kinveyClient.File().uploadMetadataAsync(fmd);

			// Assert
			Assert.IsNotNull(fmdUpdate);
			Assert.AreEqual(fmdUpdate._public, fmd._public);

			// Teardown
			kinveyClient.ActiveUser.Logout();
		}

        [TestMethod]
		public async Task TestFileUploadMetadataAsyncBad()
		{
			// Setup
            if (MockData) MockResponses(1);
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			FileMetaData fileMetaData = new FileMetaData();
			bool publicAccess = false;
			fileMetaData.fileName = "test";

			// Act
			// Assert
            await Assert.ThrowsExceptionAsync<KinveyException>(async delegate ()
			{
				await kinveyClient.File().uploadMetadataAsync(fileMetaData);
			});

			// Teardown
			kinveyClient.ActiveUser.Logout();
		}

        [TestMethod]
		public async Task TestFileDownloadByteAsync()
		{
			// Setup
            if (MockData) MockResponses(7);
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			FileMetaData uploadMetaData = new FileMetaData();
			uploadMetaData.fileName = image_name;
			uploadMetaData._public = true;

			byte[] content = System.IO.File.ReadAllBytes(image_path);
			int contentSize = (content.Length) * sizeof(byte);
			uploadMetaData.size = contentSize;
			FileMetaData uploadFMD = await kinveyClient.File().uploadAsync(uploadMetaData, content);

			FileMetaData downloadMetaData = new FileMetaData();
			downloadMetaData = await kinveyClient.File().downloadMetadataAsync(uploadFMD.id);
			downloadMetaData.id = uploadFMD.id;
			byte[] downloadContent = new byte[downloadMetaData.size];

			// Act
			FileMetaData downloadFMD = await kinveyClient.File().downloadAsync(downloadMetaData, downloadContent);
			System.IO.File.WriteAllBytes(downloadByteArrayFilePath, content);

			// Assert
			Assert.IsNotNull(content);
            Assert.IsTrue(content.Length > 0);

			// Teardown
			kinveyClient.ActiveUser.Logout();
		}

        [TestMethod]
		public async Task TestFileDownloadStreamAsync()
		{
			// Setup
            if (MockData) MockResponses(7);
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			FileMetaData uploadMetaData = new FileMetaData();
			uploadMetaData.fileName = image_name;
			uploadMetaData._public = true;

			byte[] content = System.IO.File.ReadAllBytes(image_path);
			int contentSize = (content.Length) * sizeof(byte);
			uploadMetaData.size = contentSize;
			MemoryStream uploadStreamContent = new MemoryStream(content);
			FileMetaData uploadFMD = await kinveyClient.File().uploadAsync(uploadMetaData, uploadStreamContent);

			FileMetaData downloadMetaData = new FileMetaData();
			downloadMetaData = await kinveyClient.File().downloadMetadataAsync(uploadFMD.id);
			downloadMetaData.id = uploadFMD.id;
			MemoryStream downloadStreamContent = new MemoryStream();

			// Act
			FileMetaData downloadFMD = await kinveyClient.File().downloadAsync(downloadMetaData, downloadStreamContent);
			FileStream fs = new FileStream(downloadStreamFilePath, FileMode.Create);
			downloadStreamContent.WriteTo(fs);
			downloadStreamContent.Close();
			fs.Close();

			// Assert
			Assert.IsNotNull(content);
			Assert.IsTrue(content.Length > 0);

			// Teardown
			kinveyClient.ActiveUser.Logout();
		}

        [TestMethod]
		public async Task TestFileDownloadAsyncBad()
		{
            // Setup
            if (MockData) MockResponses(1);
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			FileMetaData fileMetaData = null;
			byte[] content = null;

			// Act
			// Assert
            await Assert.ThrowsExceptionAsync<KinveyException>(async delegate ()
			{
				await kinveyClient.File().downloadAsync(fileMetaData, content);
			});

			// Teardown
			kinveyClient.ActiveUser.Logout();
		}

        [TestMethod]
		public async Task TestFileDownloadMetadataAsync()
		{
			// Setup
            if (MockData) MockResponses(5);
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			FileMetaData uploadMetaData = new FileMetaData();
			uploadMetaData.fileName = image_name;
			uploadMetaData._public = true;

			byte[] content = System.IO.File.ReadAllBytes(image_path);
			int contentSize = (content.Length) * sizeof(byte);
			uploadMetaData.size = contentSize;
			FileMetaData uploadFMD = await kinveyClient.File().uploadAsync(uploadMetaData, content);

			// Act
			FileMetaData downloadMetaData = await kinveyClient.File().downloadMetadataAsync(uploadFMD.id);

			// Assert
			Assert.IsNotNull(downloadMetaData);
			Assert.AreEqual(downloadMetaData._public, uploadFMD._public);

			// Teardown
			kinveyClient.ActiveUser.Logout();
		}

        [TestMethod]
		public async Task TestFileDownloadMetadataAsyncBad()
		{
			// Setup
            if (MockData) MockResponses(1);
			await User.LoginAsync(TestSetup.user, TestSetup.pass, kinveyClient);

			// Arrange
			string fileID = null;

			// Act
			// Assert
            await Assert.ThrowsExceptionAsync<KinveyException>(async delegate ()
			{
				await kinveyClient.File().downloadMetadataAsync(fileID);
			});

			// Teardown
			kinveyClient.ActiveUser.Logout();
		}
	}
}
