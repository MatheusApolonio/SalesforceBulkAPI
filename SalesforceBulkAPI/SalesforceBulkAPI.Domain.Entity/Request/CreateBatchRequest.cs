using SalesforceBulkAPI.Domain.Entity.Enums;

namespace SalesforceBulkAPI.Domain.Entity.Request
{
    public class CreateBatchRequest
    {
        public string JobId { get; set; }
        public string BatchContents { get; set; }
        public BatchContentType? Type { get; set; }

        public string BatchContentHeader
        {
            get
            {
                switch (Type)
                {
                    case BatchContentType.CSV:
                        return "text/csv";
                    case BatchContentType.XML:
                        return "application/xml";
                    default:
                        return "text/csv";
                }
            }
        }
    }
}