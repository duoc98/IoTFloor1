// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Azure;
using Azure.Core.Pipeline;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;


namespace Unreal
{
    public static class ADT
    {

        private static readonly string adtInstanceUrl = Environment.GetEnvironmentVariable("ADT_SERVICE_URL");
        private static readonly HttpClient httpClient = new HttpClient();

        [FunctionName("IoTHubIngest")]
        public static void Run([EventGridTrigger]EventGridEvent eventGridEvent, ILogger log)
        {
            if (adtInstanceUrl == null) log.LogError("Application setting \"ADT_SERVICE_URL\" not set");
            log.LogInformation($"ADT instance: {adtInstanceUrl} ");

            try
            {
                //Authenticate with Digital Twins
                DefaultAzureCredential cred = new DefaultAzureCredential();
                var token = cred.GetToken(new Azure.Core.TokenRequestContext(new string[] { "https://digitaltwins.azure.net" }));
                DigitalTwinsClient client = new DigitalTwinsClient(new Uri(adtInstanceUrl), cred, new DigitalTwinsClientOptions { Transport = new HttpClientTransport(httpClient) });
                log.LogInformation($"ADT service client connection created.");
                if (eventGridEvent != null && eventGridEvent.Data != null)
                {
                    log.LogInformation(eventGridEvent.Data.ToString());

                    JObject deviceMessage = (JObject)JsonConvert.DeserializeObject(eventGridEvent.Data.ToString());
                    string deviceId = (string)deviceMessage["systemProperties"]["iothub-connection-device-id"];
                    var updateTwinData = new JsonPatchDocument();
                    
                    #region HVAC, lighting, temperature sensor
                    if (deviceMessage.SelectToken("body.temperature", errorWhenNoMatch: false) != null)
                    {
                        log.LogInformation($"Device:{deviceId} contains temperature data");
                        updateTwinData.AppendAdd("/temperature", deviceMessage["body"]["temperature"].Value<double>());
                    }
                    if (deviceMessage.SelectToken("body.airflow", errorWhenNoMatch: false) != null)
                    {
                        log.LogInformation($"Device:{deviceId} contains airflow data");
                        updateTwinData.AppendAdd("/airflow", deviceMessage["body"]["airflow"].Value<string>());
                    }
                    if (deviceMessage.SelectToken("body.IsOccupied", errorWhenNoMatch: false) != null)
                    {
                        log.LogInformation($"Device:{deviceId} contains presence data");
                        updateTwinData.AppendAdd("/IsOccupied", deviceMessage["body"]["IsOccupied"].Value<bool>());
                    }
                    if (deviceMessage.SelectToken("body.State", errorWhenNoMatch: false) != null)
                    {
                        log.LogInformation($"Device:{deviceId} contains lighting data");
                        updateTwinData.AppendAdd("/State", deviceMessage["body"]["State"].Value<bool>());
                    }
                    #endregion

                    #region Computer Sensor
                    if (deviceMessage.SelectToken("body.isPCActive", errorWhenNoMatch: false) != null)
                    {
                        log.LogInformation($"Device:{deviceId} contains PC active data:");
                        updateTwinData.AppendAdd("/isPCActive", deviceMessage["body"]["isPCActive"].Value<bool>());
                    }

                    if (deviceMessage.SelectToken("body.pcName", errorWhenNoMatch: false) != null)
                    {
                        log.LogInformation($"Device:{deviceId} contains PC name data:");
                        updateTwinData.AppendAdd("/pcName", deviceMessage["body"]["pcName"].Value<string>());
                    }

                    if (deviceMessage.SelectToken("body.motherboardName", errorWhenNoMatch: false) != null)
                    {
                        log.LogInformation($"Device:{deviceId} contains motherboard name data:");
                        updateTwinData.AppendAdd("/motherboardName", deviceMessage["body"]["motherboardName"].Value<string>());
                    }

                    if (deviceMessage.SelectToken("body.motherboardTemperature", errorWhenNoMatch: false) != null)
                    {
                        log.LogInformation($"Device:{deviceId} contains motherboard temperature data:");
                        updateTwinData.AppendAdd("/motherboardTemperature", deviceMessage["body"]["motherboardTemperature"].Value<string>());
                    }

                    if (deviceMessage.SelectToken("body.cpuName", errorWhenNoMatch: false) != null)
                    {
                        log.LogInformation($"Device:{deviceId} contains CPU name data:");
                        updateTwinData.AppendAdd("/cpuName", deviceMessage["body"]["cpuName"].Value<string>());
                    }

                    if (deviceMessage.SelectToken("body.cpuUsage", errorWhenNoMatch: false) != null)
                    {
                        log.LogInformation($"Device:{deviceId} contains CPU usage data:");
                        updateTwinData.AppendAdd("/cpuUsage", deviceMessage["body"]["cpuUsage"].Value<string>());
                    }

                    if (deviceMessage.SelectToken("body.cpuTemperature", errorWhenNoMatch: false) != null)
                    {
                        log.LogInformation($"Device:{deviceId} contains CPU temperature data:");
                        updateTwinData.AppendAdd("/cpuTemperature", deviceMessage["body"]["cpuTemperature"].Value<string>());
                    }

                    if (deviceMessage.SelectToken("body.memoryUsage", errorWhenNoMatch: false) != null)
                    {
                        log.LogInformation($"Device:{deviceId} contains memory usage data:");
                        updateTwinData.AppendAdd("/memoryUsage", deviceMessage["body"]["memoryUsage"].Value<string>());
                    }

                    if (deviceMessage.SelectToken("body.gpuName", errorWhenNoMatch: false) != null)
                    {
                        log.LogInformation($"Device:{deviceId} contains GPU name data:");
                        updateTwinData.AppendAdd("/gpuName", deviceMessage["body"]["gpuName"].Value<string>());
                    }

                    if (deviceMessage.SelectToken("body.gpuUsage", errorWhenNoMatch: false) != null)
                    {
                        log.LogInformation($"Device:{deviceId} contains GPU usage data:");
                        updateTwinData.AppendAdd("/gpuUsage", deviceMessage["body"]["gpuUsage"].Value<string>());
                    }

                    if (deviceMessage.SelectToken("body.gpuTemperature", errorWhenNoMatch: false) != null)
                    {
                        log.LogInformation($"Device:{deviceId} contains GPU temperature data:");
                        updateTwinData.AppendAdd("/gpuTemperature", deviceMessage["body"]["gpuTemperature"].Value<string>());
                    }

                    if (deviceMessage.SelectToken("body.ssdUsage", errorWhenNoMatch: false) != null)
                    {
                        log.LogInformation($"Device:{deviceId} contains SSD usage data:");
                        updateTwinData.AppendAdd("/ssdUsage", deviceMessage["body"]["ssdUsage"].Value<string>());
                    }

                    if (deviceMessage.SelectToken("body.hddUsage", errorWhenNoMatch: false) != null)
                    {
                        log.LogInformation($"Device:{deviceId} contains HDD usage data:");
                        updateTwinData.AppendAdd("/hddUsage", deviceMessage["body"]["hddUsage"].Value<string>());
                    }

                    if (deviceMessage.SelectToken("body.runningProcess1", errorWhenNoMatch: false) != null)
                    {
                        log.LogInformation($"Device:{deviceId} contains running process 1 data:");
                        updateTwinData.AppendAdd("/runningProcess1", deviceMessage["body"]["runningProcess1"].Value<string>());
                    }

                    if (deviceMessage.SelectToken("body.runningProcess2", errorWhenNoMatch: false) != null)
                    {
                        log.LogInformation($"Device:{deviceId} contains running process 2 data:");
                        updateTwinData.AppendAdd("/runningProcess2", deviceMessage["body"]["runningProcess2"].Value<string>());
                    }
                    
                    if (deviceMessage.SelectToken("body.runningProcess3", errorWhenNoMatch: false) != null)
                    {
                        log.LogInformation($"Device:{deviceId} contains running process 3 data:");
                        updateTwinData.AppendAdd("/runningProcess3", deviceMessage["body"]["runningProcess3"].Value<string>());
                    }
                    #endregion
                    
                    log.LogInformation($"Sending patch document for device {deviceId}:  {updateTwinData.ToString()}");
                    client.UpdateDigitalTwinAsync(deviceId, updateTwinData).ConfigureAwait(true).GetAwaiter().GetResult();
                }
            }
            catch (Exception e)
            {
                log.LogError(e.ToString());
                throw e;
            }
        }
    }
}
