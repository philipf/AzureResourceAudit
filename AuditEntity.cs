using System;
using Microsoft.Azure.Cosmos.Table;

namespace EventGridFunc
{
    public class AuditEntity : TableEntity
    {

        public string Subscription { get; set; }

        public string ResourceGroup { get; set; }
        public string Resource { get; set; }

        public string Provider { get; set; }

        public string CreatedBy { get; set; }
        public DateTime? CreatedDt { get; set; } 
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDt { get; set; }
        public int NoOfUpdates { get; set; }
        public string DeletedBy { get;  set; }
        public DateTime? DeletedDt { get;  set; }
        public string Subject { get; set; }

        public string EventType { get; set; }

        public AuditEntity() {}

        public AuditEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

    }
}