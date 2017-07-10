using System;
using System.Linq;
using System.Xml.Linq;

namespace SalesforceBulkAPI.Domain.Entity
{
    public class Job
    {
        public string Id { get; set; }
        public string Operation { get; set; }
        public string Object { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime SystemModStamp { get; set; }
        public string State { get; set; }
        public string ConcurrencyMode { get; set; }
        public string ContentType { get; set; }
        public int NumberBatchesQueued { get; set; }
        public int NumberBatchesInProgress { get; set; }
        public int NumberBatchesCompleted { get; set; }
        public int NumberBatchesFailed { get; set; }
        public int NumberBatchesTotal { get; set; }
        public int NumberRecordsProcessed { get; set; }
        public int NumberRecordsFailed { get; set; }
        public int NumberRetries { get; set; }
        public int TotalProcessingTime { get; set; }
        public int ApiActiveProcessingTime { get; set; }
        public int ApexProcessingTime { get; set; }

        public bool IsDone => NumberBatchesTotal == NumberBatchesCompleted + NumberBatchesFailed ||
                              State.ToUpper().Equals("ABORTED");

        public static Job CreateJob(string xml)
        {
            var document = XDocument.Parse(xml);
            var jobInfoChildElements = document.Root != null && document.Root.HasElements
                ? document.Root.Elements().ToList()
                : null;

            var job = new Job();

            if (jobInfoChildElements == null) return null;

            foreach (var element in jobInfoChildElements)
            {
                var value = element.Value;
                switch (element.Name.LocalName)
                {
                    case "id":
                        job.Id = value;
                        break;
                    case "operation":
                        job.Operation = value;
                        break;
                    case "object":
                        job.Object = value;
                        break;
                    case "createdById":
                        job.CreatedById = value;
                        break;
                    case "createdDate":
                        job.CreatedDate = DateTime.Parse(value);
                        break;
                    case "systemModstamp":
                        job.SystemModStamp = DateTime.Parse(value);
                        break;
                    case "state":
                        job.State = value;
                        break;
                    case "concurrencyMode":
                        job.ConcurrencyMode = value;
                        break;
                    case "contentType":
                        job.ContentType = value;
                        break;
                    case "numberBatchesQueued":
                        job.NumberBatchesQueued = int.Parse(value);
                        break;
                    case "numberBatchesInProgress":
                        job.NumberBatchesInProgress = int.Parse(value);
                        break;
                    case "numberBatchesCompleted":
                        job.NumberBatchesCompleted = int.Parse(value);
                        break;
                    case "numberBatchesFailed":
                        job.NumberBatchesFailed = int.Parse(value);
                        break;
                    case "numberBatchesTotal":
                        job.NumberBatchesTotal = int.Parse(value);
                        break;
                    case "numberRecordsProcessed":
                        job.NumberRecordsProcessed = int.Parse(value);
                        break;
                    case "numberRetries":
                        job.NumberRetries = int.Parse(value);
                        break;
                    case "numberRecordsFailed":
                        job.NumberRecordsFailed = int.Parse(value);
                        break;
                    case "totalProcessingTime":
                        job.TotalProcessingTime = int.Parse(value);
                        break;
                    case "apiActiveProcessingTime":
                        job.ApiActiveProcessingTime = int.Parse(value);
                        break;
                    case "apexProcessingTime":
                        job.ApexProcessingTime = int.Parse(value);
                        break;
                }
            }

            return job;
        }
    }
}