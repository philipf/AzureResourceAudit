// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName=AuditSubscriptionEvents
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;

namespace EventGridFunc
{
    /// <summary>
    /// Azure Function that subscribes to Azure Subscription events and records audit information
    /// to Table Storage
    /// </summary>
    public static class AuditSubscriptionEvents
    {
        [FunctionName("AuditSubscriptionEvents")]
        public static async void Run([EventGridTrigger]EventGridEvent eventGridEvent, ILogger log)
        {
            log.LogDebug(eventGridEvent.Data?.ToString());

            try
            {
                string azureStorageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                var service = new AuditService(azureStorageConnectionString);
                await service.Record(eventGridEvent);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
