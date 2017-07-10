namespace SalesforceBulkAPI.Domain.Entity.Request
{
    public class CreateAttachmentBatchRequest : CreateBatchRequest
    {
        public string FilePath { get; set; }
        public string ParentId { get; set; }
    }
}