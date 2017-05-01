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
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Kinvey
{
	public interface ISyncQueue
	{
		string Collection { get; }

		int Enqueue (PendingWriteAction pending);
		PendingWriteAction Peek ();
		PendingWriteAction Pop ();
		int Count(bool allCollections);

		List<PendingWriteAction> GetAll ();
		List<PendingWriteAction> GetFirstN(int limit, int offset);
		PendingWriteAction GetByID(string entityId);

		int Remove (string entityId);
		int Remove(List<string> entityIDs);
		int RemoveAll ();
	}
}
