using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.EventGrid.Models;

namespace EventGridFunc
{
    public class AuditService
    {
        private readonly string _storageConnnectionString;

        public AuditService(string storageConnnectionString)
        {
            _storageConnnectionString = storageConnnectionString;
        }

        public async Task Record(EventGridEvent eventGridEvent)
        {
            string rowKey       = ParseRowKey(eventGridEvent.Subject);
            var    eventInfo    = new EventInfo(eventGridEvent);
            string partitionKey = eventInfo.ResourceGroup;

            CloudTable table = await GetAuditTable(_storageConnnectionString);
            AuditEntity auditEntity = await GetResource(table, partitionKey, rowKey);
            auditEntity = PopulateAuditEntity(eventGridEvent, rowKey, eventInfo, partitionKey, auditEntity);
            
            var insertMergeOp = TableOperation.InsertOrMerge(auditEntity);
            await table.ExecuteAsync(insertMergeOp);
        }

        private static async Task<CloudTable> GetAuditTable(string connectionString)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient    tableClient    = storageAccount.CreateCloudTableClient();
            CloudTable          table          = tableClient.GetTableReference("Resource");

            await table.CreateIfNotExistsAsync();

            return table;
        }

        private static AuditEntity PopulateAuditEntity(
            EventGridEvent eventGridEvent,
             string rowKey, 
             EventInfo eventInfo, 
             string partitionKey, 
             AuditEntity auditEntity)
        {
            if (auditEntity == null)
            {
                auditEntity = new AuditEntity(partitionKey, rowKey);

                if (!eventGridEvent.EventType.Contains("Delete"))
                {
                    auditEntity.CreatedBy = eventInfo.By;
                    auditEntity.CreatedDt = eventGridEvent.EventTime;
                }
            }
            else
            {
                if (!eventGridEvent.EventType.Contains("Delete"))
                {
                    auditEntity.UpdatedBy = eventInfo.By;
                    auditEntity.UpdatedDt = eventGridEvent.EventTime;
                    auditEntity.NoOfUpdates++;
                }
            }

            auditEntity.ResourceGroup = eventInfo.ResourceGroup;
            auditEntity.Provider      = eventInfo.ResourceProvider;
            auditEntity.Subscription  = eventInfo.Subscription;
            auditEntity.Subject       = eventGridEvent.Subject;
            auditEntity.Resource      = eventInfo.Resource;
            auditEntity.EventType     = eventGridEvent.EventType;

            if (eventGridEvent.EventType.Contains("Delete"))
            {
                auditEntity.DeletedBy = eventInfo.By;
                auditEntity.DeletedDt = eventGridEvent.EventTime;
            }            

            return auditEntity;
        }

        private static string ParseRowKey(string subject)
        {
            string rk = subject.ToLower(); // Azure sending difference values
            rk = rk.Replace("/", "|"); // Table Storage cannot query on /
            return rk;
        }

        private static async Task<AuditEntity> GetResource(CloudTable table, string partitionKey, string rowKey)
        {
            var retrieveOp  = TableOperation.Retrieve<AuditEntity>(partitionKey, rowKey);
            TableResult result = await table.ExecuteAsync(retrieveOp);
            var auditEntity = result.Result as AuditEntity;
            
            return auditEntity;
        }
    }
}