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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using Kinvey;
using System.Net.Http;

namespace Kinvey.Tests
{
    [TestClass]
    public class FileUnitTests
    {
        [TestMethod]
        public void TestDetermineStartByteFromRange()
        {
            // Arrange
            string rangeHeader = "bytes=0-42";
            int startByte = 0;

            // Act
            startByte = KinveyFileRequest.DetermineStartByteFromRange(rangeHeader);

            // Assert
            Assert.IsTrue(startByte == 43);
        }

        [TestMethod]
        public void TestDetermineStartByteFromRangeNullHeaderValue()
        {
            // Arrange
            string rangeHeader = null;
            int startByte = 0;

            // Act
            startByte = KinveyFileRequest.DetermineStartByteFromRange(rangeHeader);

            // Assert
            Assert.IsTrue(startByte == 0);
        }
    }
}
