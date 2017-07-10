using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using SalesforceBulkAPI.Contract.SFEnterprise;
using SalesforceBulkAPI.Domain.Entity;
using SalesforceBulkAPI.Domain.Entity.Request;
using SalesforceBulkAPI.Domain.Service;

namespace SalesforceBulkAPI.Contract.Client
{
    public class BulkApiClient : IJobService, IBatchService, IDisposable
    {
        private readonly string _loginURL;
        private readonly string _password;
        private readonly string _token;
        private readonly string _userName;
        private string _baseRequestUrl;
        private LoginResult _loginResult;

        private SforceService _salesforceService;

        public BulkApiClient(string userName, string password, string token, string loginUrl)
        {
            _userName = userName;
            _password = password;
            _token = token;
            _loginURL = loginUrl;

            Login();
        }

        public Batch CreateAttachmentBatch(CreateAttachmentBatchRequest request)
        {
            var requestTxtFileCSVContents = "Name,ParentId,Body" + Environment.NewLine;
            requestTxtFileCSVContents += request.FilePath + "," + request.ParentId + ",#" + request.FilePath;

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var requestTxtFileCSVContentsBytes = Encoding.UTF8.GetBytes(requestTxtFileCSVContents);
                    var requestTxtFileInArchive = archive.CreateEntry("request.txt");
                    using (var entryStream = requestTxtFileInArchive.Open())
                    using (var fileToCompressStream = new MemoryStream(requestTxtFileCSVContentsBytes))
                    {
                        fileToCompressStream.CopyTo(entryStream);
                    }

                    var attachmentFileContentsBytes = File.ReadAllBytes(request.FilePath);
                    var attachmentFileInArchive = archive.CreateEntry(request.FilePath);
                    using (var attachmentEntryStream = attachmentFileInArchive.Open())
                    using (var attachmentFileToCompressStream = new MemoryStream(attachmentFileContentsBytes))
                    {
                        attachmentFileToCompressStream.CopyTo(attachmentEntryStream);
                    }

                    var zipFileBytes = memoryStream.ToArray();

                    var requestUrl = _baseRequestUrl + request.JobId + "/batch";

                    var responseBytes = InvokeRestAPI(requestUrl, zipFileBytes, "POST", "zip/csv");

                    var resultXML = Encoding.UTF8.GetString(responseBytes);

                    return Batch.CreateBatch(resultXML);
                }
            }
        }

        public Batch CreateBatch(CreateBatchRequest createBatchRequest)
        {
            var requestUrl = _baseRequestUrl + createBatchRequest.JobId + "/batch";

            var requestXML = createBatchRequest.BatchContents;

            var contentType = string.Empty;

            if (createBatchRequest.Type.HasValue)
                contentType = createBatchRequest.BatchContentHeader;

            var resultXML = InvokeRestAPI(requestUrl, requestXML, "Post", contentType);

            return Batch.CreateBatch(resultXML);
        }

        public Batch GetBatch(string jobId, string batchId)
        {
            var requestUrl = _baseRequestUrl + jobId + "/batch/" + batchId;

            var resultXML = InvokeRestAPI(requestUrl);

            return Batch.CreateBatch(resultXML);
        }

        public IList<Batch> GetBatches(string jobId)
        {
            var requestUrl = _baseRequestUrl + jobId + "/batch/";

            var resultXML = InvokeRestAPI(requestUrl);

            return Batch.CreateBatches(resultXML);
        }

        public string GetBatchRequest(string jobId, string batchId)
        {
            var requestUrl = _baseRequestUrl + jobId + "/batch/" + batchId + "/request";

            var resultXML = InvokeRestAPI(requestUrl);

            return resultXML;
        }

        public string GetBatchResults(string jobId, string batchId)
        {
            var requestUrl = _baseRequestUrl + jobId + "/batch/" + batchId + "/result";

            var resultXML = InvokeRestAPI(requestUrl);

            return resultXML;
        }

        public IList<string> GetResultIds(string queryBatchResultListXML)
        {
            var doc = XDocument.Parse(queryBatchResultListXML);

            var resultListElement = doc.Root;

            return resultListElement?.Elements().Select(resultElement => resultElement.Value).ToList();
        }

        public string GetBatchResult(string jobId, string batchId, string resultId)
        {
            var requestUrl = _baseRequestUrl + jobId + "/batch/" + batchId + "/result/" + resultId;

            var resultXML = InvokeRestAPI(requestUrl);

            return resultXML;
        }

        public Job CreateJob(CreateJobRequest createJobRequest)
        {
            var jobRequestXML =
                @"<?xml version=""1.0"" encoding=""UTF-8""?>
             <jobInfo xmlns=""http://www.force.com/2009/06/asyncapi/dataload"">
               <operation>{0}</operation>
               <object>{1}</object>
               {3}
               <contentType>{2}</contentType>
             </jobInfo>";

            var externalField = string.Empty;

            if (string.IsNullOrWhiteSpace(createJobRequest.ExternalIdFieldName) == false)
                externalField = "<externalIdFieldName>" + createJobRequest.ExternalIdFieldName +
                                "</externalIdFieldName>";

            jobRequestXML = string.Format(jobRequestXML,
                createJobRequest.OperationString,
                createJobRequest.Object,
                createJobRequest.ContentTypeString,
                externalField);

            var createJobUrl = _baseRequestUrl.Substring(_baseRequestUrl.Length - 1);

            var resultXML = InvokeRestAPI(createJobUrl, jobRequestXML);

            return Job.CreateJob(resultXML);
        }

        public Job CloseJob(string jobId)
        {
            var closeJobUrl = BuildSpecificJobUrl(jobId);
            var closeRequestXML =
                @"<?xml version=""1.0"" encoding=""UTF-8""?>" + Environment.NewLine +
                @"<jobInfo xmlns=""http://www.force.com/2009/06/asyncapi/dataload"">" + Environment.NewLine +
                "<state>Closed</state>" + Environment.NewLine +
                "</jobInfo>";

            var resultXML = InvokeRestAPI(closeJobUrl, closeRequestXML);

            return Job.CreateJob(resultXML);
        }

        public Job GetJob(string jobId)
        {
            var getJobUrl = BuildSpecificJobUrl(jobId);

            var resultXML = InvokeRestAPI(getJobUrl);

            return Job.CreateJob(resultXML);
        }

        public Job GetCompletedJob(string jobId)
        {
            var job = GetJob(jobId);

            while (job.IsDone == false)
            {
                Thread.Sleep(2000);
                job = GetJob(jobId);
            }

            return job;
        }

        private string BuildSpecificJobUrl(string jobId) => _baseRequestUrl + jobId;

        private void Login()
        {
            _salesforceService = new SforceService {Url = _loginURL};

            _loginResult = _salesforceService.login(_userName, string.Concat(_password, _token));
            _salesforceService.Url = _loginResult.serverUrl;
            _baseRequestUrl = string.Concat(@"https://", _loginResult.sandbox ? "test" : "login",
                ".salesforce.com/services/async/31.0/job/");
        }

        private string InvokeRestAPI(string endpointURL)
        {
            var wc = BuildWebClient();

            return wc.DownloadString(endpointURL);
        }

        private string InvokeRestAPI(string endpointURL, string postData)
        {
            return InvokeRestAPI(endpointURL, postData, "Post", string.Empty);
        }

        private string InvokeRestAPI(string endpointURL, string postData, string httpVerb, string contentType)
        {
            var postDataBytes = Encoding.UTF8.GetBytes(postData);

            var response = InvokeRestAPI(endpointURL, postDataBytes, httpVerb, contentType);

            return Encoding.UTF8.GetString(response);
        }

        private byte[] InvokeRestAPI(string endpointURL, byte[] postData, string httpVerb, string contentType)
        {
            var wc = BuildWebClient();

            if (string.IsNullOrWhiteSpace(contentType) == false)
                wc.Headers.Add("Content-Type: " + contentType);

            try
            {
                return wc.UploadData(endpointURL, httpVerb, postData);
            }
            catch (WebException webEx)
            {
                if (webEx.Response == null) throw;

                using (var errorResponse = (HttpWebResponse) webEx.Response)
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                    {
                        reader.ReadToEnd();
                    }
                }

                throw;
            }
        }

        private WebClient BuildWebClient()
        {
            var wc = new WebClient {Encoding = Encoding.UTF8};
            wc.Headers.Add("X-SFDC-Session: " + _loginResult.sessionId);

            return wc;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}