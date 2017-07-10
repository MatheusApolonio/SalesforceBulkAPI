using SalesforceBulkAPI.Domain.Entity.Enums;

namespace SalesforceBulkAPI.Domain.Entity.Request
{
    public class JobRequest
    {
        public JobContentType ContentType { get; set; }

        public string ContentTypeString
        {
            get
            {
                switch (ContentType)
                {
                    case JobContentType.CSV:
                        return "CSV";
                    case JobContentType.XML:
                        return "XML";
                    case JobContentType.ZIP_CSV:
                        return "ZIP_CSV";
                    default:
                        return "XML";
                }
            }
        }

        public JobOperation Operation { get; set; }

        public string OperationString {
            get
            {
                switch (Operation)
                {
                    case JobOperation.Insert:
                        return "insert";
                    case JobOperation.Update:
                        return "update";
                    case JobOperation.HardDelete:
                        return "hardDelete";
                    case JobOperation.Delete:
                        return "delete";
                    case JobOperation.Query:
                        return "query";
                    case JobOperation.Upsert:
                        return "upsert";
                    default:
                        return "upsert";
                }
            }
        }

        public string Object { get; set; }
        public string ExternalIdFieldName { get; set; }
    }
}