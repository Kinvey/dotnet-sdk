// Copyright (c) 2019, Kinvey, Inc. All rights reserved.
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

using System.Collections.Generic;

namespace Kinvey
{
    /// <summary>
    /// Interface for the synchronization queue.
    /// </summary>
	public interface ISyncQueue
	{
        /// <summary>
		/// Collection name.
		/// </summary>
		/// <returns> Collection name. </returns>
		string Collection { get; }

        /// <summary>
        /// Enqueues a pending write action item.
        /// </summary>
        /// <returns>The count of enqueued pending write action items.</returns>
        /// <param name="pending">Pending write action item.</param>
        int Enqueue (PendingWriteAction pending);

        /// <summary>
        /// Fetches the first pending write action item.
        /// </summary>
        /// <returns> Received pending write action item from a source. </returns>
        PendingWriteAction Peek ();

        /// <summary>
        /// Fetches and deletes the first pending write action item.
        /// </summary>
        /// <returns> Received pending write action item from a source. </returns>
        PendingWriteAction Pop ();

        /// <summary>
        /// Gets count of pending write action items.
        /// </summary>
        /// <returns> The count of pending write action items. </returns>
        /// <param name="allCollections"><c>True</c> if the count for all collections; otherwise the count for the specific collection.</param>
        int Count(bool allCollections);

        /// <summary>
        /// Gets all pending write action items for specific collection.
        /// </summary>
        /// <returns> The list of pending write action items. </returns>
        List<PendingWriteAction> GetAll ();

        /// <summary>
        /// Gets pending write action items according to limit and offset.
        /// </summary>
        /// <param name="limit"> Limit. </param>
        /// <param name="offset"> Offset. </param>
        /// <returns> The list of pending write action items. </returns>
        List<PendingWriteAction> GetFirstN(int limit, int offset);

        /// <summary>
        /// Gets pending write action item.
        /// </summary>
        /// <param name="entityId"> The identifier of entity. </param>
        /// <returns> The pending write action item. </returns>
        PendingWriteAction GetByID(string entityId);

        /// <summary>
        /// Removes pending write action item.
        /// </summary>
        /// <param name="pending"> Pending write action item. </param>
        /// <returns>The count of removed pending write action items.</returns>
		int Remove(PendingWriteAction pending);

        /// <summary>
        /// Removes pending write action items.
        /// </summary>
        /// <param name="pendings"> Pending write action items. </param>
        /// <returns>The count of removed pending write action items.</returns>
        int Remove(IEnumerable<PendingWriteAction> pendings);

        /// <summary>
        /// Removes all pending write action items.
        /// </summary>
        /// <returns>The count of removed pending write action items.</returns>
        int RemoveAll ();

        /// <summary>
        /// Gets pending write action items according to limit, offset and action.
        /// </summary>
        /// <param name="limit"> Limit. </param>
        /// <param name="offset"> Offset. </param>
        /// <param name="action"> Action. </param>
        /// <returns> The list of pending write action items. </returns>
        List<PendingWriteAction> GetFirstN(int limit, int offset, string action);
    }
}
