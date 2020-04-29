using System.Text.RegularExpressions;
using Microsoft.Azure.EventGrid.Models;
using Newtonsoft.Json.Linq;

namespace EventGridFunc
{
    /// <summary>
    /// Contains key properties extracted from the EventGrid event
    /// </summary>
    public class EventInfo
    {
        public string By { get; }
        public string ResourceGroup { get; }
        public string ResourceProvider { get; }
        public string Resource { get; }
        public string Subscription { get; }

        public EventInfo(EventGridEvent eventGridEvent)
        {
            const string subPattern  = "/subscriptions/(?<sub>.+?)/";
            const string rgPattern   = "resourceGroups/(?<rg>.+?)($|/)";
            const string provPattern = "providers/(?<prov>.+?)/";
            const string resPattern  = "providers/.+?/(?<res>.+)$";

            ResourceGroup    = RegexMatch(eventGridEvent.Subject, rgPattern, "rg");
            ResourceProvider = RegexMatch(eventGridEvent.Subject, provPattern, "prov");
            Resource         = RegexMatch(eventGridEvent.Subject, resPattern, "res");
            Subscription     = RegexMatch(eventGridEvent.Subject, subPattern, "sub");
            
            JObject data = eventGridEvent.Data as JObject;
            By = data?["claims"]?["name"]?.Value<string>();

            if (string.IsNullOrWhiteSpace(ResourceProvider))
            {
                ResourceProvider = data?["resourceProvider"]?.Value<string>();
            }
        }

        private static string RegexMatch(string value, string pattern, string group)
        {
            return Regex.Match(value, pattern, RegexOptions.IgnoreCase).Groups[group].Value;
        }
    }
}