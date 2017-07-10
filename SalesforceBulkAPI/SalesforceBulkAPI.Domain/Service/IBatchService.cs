using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalesforceBulkAPI.Domain.Entity;
using SalesforceBulkAPI.Domain.Entity.Request;

namespace SalesforceBulkAPI.Domain.Service
{
    public interface IBatchService
    {
        Batch CreateAttachmentBatch(AttachmentBatchRequest request);
        Batch CreateBatch(BatchRequest request);
        Batch GetBatch(string jobId, string batchId);
        IList<Batch> GetBatches(string jobId);
        string GetBatchRequest(string jobId, string batchId);
        string GetBatchResults(string jobId, string batchId);
        IList<string> GetResultIds(string queryBatchResultListXML);
        string GetBatchResult(string jobId, string batchId, string resultId);
    }
}