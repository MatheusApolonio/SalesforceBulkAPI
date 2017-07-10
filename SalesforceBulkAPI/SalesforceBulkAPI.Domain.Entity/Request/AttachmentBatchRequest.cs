namespace SalesforceBulkAPI.Domain.Entity.Request
{
    public class AttachmentBatchRequest : BatchRequest
    {
        public string FilePath { get; set; }
        public string ParentId { get; set; }
    }
}