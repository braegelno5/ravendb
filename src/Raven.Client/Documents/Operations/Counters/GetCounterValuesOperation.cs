﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using Raven.Client.Documents.Conventions;
using Raven.Client.Http;
using Sparrow.Json;

namespace Raven.Client.Documents.Operations.Counters
{
    public class GetCounterValuesOperation : IOperation<Dictionary<Guid, long>>
    {
        private readonly string _documentId;
        private readonly string _name;

        public GetCounterValuesOperation(string documentId, string name)
        {
            _documentId = documentId;
            _name = name;
        }

        public RavenCommand<Dictionary<Guid, long>> GetCommand(IDocumentStore store, DocumentConventions conventions, JsonOperationContext context, HttpCache cache)
        {
            return new GetCounterValuesCommand(_documentId, _name);
        }

        private class GetCounterValuesCommand : RavenCommand<Dictionary<Guid, long>>
        {
            private readonly string _documentId;
            private readonly string _name;

            public GetCounterValuesCommand(string documentId, string name)
            {
                if (string.IsNullOrWhiteSpace(documentId))
                    throw new ArgumentNullException(nameof(documentId));
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentNullException(nameof(name));

                _documentId = documentId;
                _name = name;
            }

            public override HttpRequestMessage CreateRequest(JsonOperationContext ctx, ServerNode node, out string url)
            {
                url = $"{node.Url}/databases/{node.Database}/counters/getValues?doc={_documentId}&name={_name}";

                return new HttpRequestMessage
                {
                    Method = HttpMethod.Get
                };
            }

            public override void SetResponse(JsonOperationContext context, BlittableJsonReaderObject response, bool fromCache)
            {
                if (!(response["Values"] is BlittableJsonReaderArray values))
                    return;

                Result = new Dictionary<Guid, long>();

                foreach (var v in values)
                {
                    if (!(v is BlittableJsonReaderObject bjro))
                        return;

                    var key = bjro["DbId"];
                    var val = bjro["Value"];
                    Result[new Guid(key.ToString())] = (long)val;
                }

            }

            public override bool IsReadRequest => true;
        }
    }
}