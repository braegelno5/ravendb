﻿using System;
using System.Threading.Tasks;
using Raven.Server.Json;
using Raven.Server.Json.Parsing;
using Raven.Server.Routing;

namespace Raven.Server.Documents
{
    public class StatsHandler : DatabaseRequestHandler
    {
        [RavenAction("/databases/*/stats", "GET")]
        public void Stats()
        {
            RavenOperationContext context;
            using (ContextPool.AllocateOperationContext(out context))
            {
                context.Transaction = context.Environment.ReadTransaction();
                var writer = new BlittableJsonTextWriter(context, ResponseBodyStream());
                //TODO: Implement properly and split to dedicated endpoints
                //TODO: So we don't get so much stuff to ignore in the stats
                context.Write(writer, new DynamicJsonValue
                {
                    // storage
                    ["StorageEngine"] = "Voron 4.0",
                    ["DatabaseTransactionVersionSizeInMB"] = -1,

                    // indexing - should be in its /stats/indexing
                    ["CountOfIndexes"] = 0,
                    ["StaleIndexes"] = new DynamicJsonArray(),
                    ["CountOfIndexesExcludingDisabledAndAbandoned"] = 0,
                    ["CountOfResultTransformers"] = 0,
                    ["InMemoryIndexingQueueSizes"] = new DynamicJsonArray(),
                    ["ApproximateTaskCount"] = 0,
                    ["CurrentNumberOfParallelTasks"] = 1,
                    ["CurrentNumberOfItemsToIndexInSingleBatch"] = 1,
                    ["CurrentNumberOfItemsToReduceInSingleBatch"] = 1,

                    ["Errors"] = new DynamicJsonArray(),
                    ["Prefetches"] = new DynamicJsonArray(),

                    // documents
                    ["LastDocEtag"] = DocumentsStorage.ReadLastEtag(context.Transaction),
                    ["CountOfDocuments"] = Database.DocumentsStorage.GetNumberOfDocuments(context),
                    ["DatabaseId"] = Database.DocumentsStorage.Environment.DbId.ToString(),
                    ["Is64Bits"] = IntPtr.Size == sizeof (long)
                });
            }
        }
    }
}