using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SalesforceBulkAPI.Domain.Entity
{
    public class Batch
    {
        public string Id { get; set; }
        public string JobId { get; set; }
        public string State { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime SystemModStamp { get; set; }
        public int NumberRecordsProcessed { get; set; }
        public int NumberRecordsFailed { get; set; }
        public int TotalProcessingTime { get; set; }
        public int ApiActiveProcessingTime { get; set; }
        public int ApexProcessingTime { get; set; }

        public static Batch CreateBatch(string xml)
        {
            var document = XDocument.Parse(xml);
            var jobInfoChildElements = document.Root != null && document.Root.HasElements
                ? document.Root.Elements().ToList()
                : null;

            var batch = new Batch();
            if (jobInfoChildElements == null) return null;

            foreach (var e in jobInfoChildElements)
                switch (e.Name.LocalName)
                {
                    case "id":
                        batch.Id = e.Value;
                        break;
                    case "jobId":
                        batch.JobId = e.Value;
                        break;
                    case "createdDate":
                        batch.CreatedDate = DateTime.Parse(e.Value);
                        break;
                    case "systemModstamp":
                        batch.SystemModStamp = DateTime.Parse(e.Value);
                        break;
                    case "state":
                        batch.State = e.Value;
                        break;
                    case "numberRecordsProcessed":
                        batch.NumberRecordsProcessed = int.Parse(e.Value);
                        break;
                    case "numberRecordsFailed":
                        batch.NumberRecordsFailed = int.Parse(e.Value);
                        break;
                    case "totalProcessingTime":
                        batch.TotalProcessingTime = int.Parse(e.Value);
                        break;
                    case "apiActiveProcessingTime":
                        batch.ApiActiveProcessingTime = int.Parse(e.Value);
                        break;
                    case "apexProcessingTime":
                        batch.ApexProcessingTime = int.Parse(e.Value);
                        break;
                }

            return batch;
        }

        public static List<Batch> CreateBatches(string xml)
        {
            var doc = XDocument.Parse(xml);
            var batchInfoList = doc.Root;
            var batches = new List<Batch>();

            var batchInfoNodes = batchInfoList?.Nodes();

            if (batchInfoNodes == null) return batches;

            batches.AddRange(batchInfoNodes.Select(batchInfoNode => CreateBatch(batchInfoNode.ToString())));

            return batches;
        }
    }
}