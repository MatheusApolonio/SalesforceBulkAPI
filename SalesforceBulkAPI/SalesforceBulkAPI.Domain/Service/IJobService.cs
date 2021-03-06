﻿using SalesforceBulkAPI.Domain.Entity;
using SalesforceBulkAPI.Domain.Entity.Request;

namespace SalesforceBulkAPI.Domain.Service
{
    public interface IJobService
    {
        Job CreateJob(JobRequest request);
        Job CloseJob(string id);
        Job GetJob(string id);
        Job GetCompletedJob(string id);
    }
}