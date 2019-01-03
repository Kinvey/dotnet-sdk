// Copyright (c) 2018, Kinvey, Inc. All rights reserved.
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

namespace Kinvey
{
    /// <summary>
    /// Enum to filter LINQ clauses which should be used in a query for network request.
    /// </summary>
    internal enum VisitorClause
    {
        Order = 1,
        SkipTake = 2,
        Where = 4,
        Select = 8
    }
}
