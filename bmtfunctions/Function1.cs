using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Azure.Management.DataFactory;
using Microsoft.Azure.Management.DataFactory.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;

namespace bmtfunctions
{
    public static class Function1
    {
        [FunctionName("TransferBMTFiletoEDL")]
        public static HttpResponseMessage Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "TransferBMTFiletoEDL/{BusinessFunction}")]  HttpRequestMessage Request, string BusinessFunction,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");


            string tenantID = "1e4b35fb-01bb-4622-846e-c42e297d421f";
            string applicationId = "ff59dcea-49fd-4c5f-ab96-7889ba0906e9";
            string authenticationKey = "IFEGIA7JY=zN?m9E31D-zXyBu?US2fAv";
            string subscriptionId = "85c66763-9f02-4ebc-8447-35ba364edeb0";

            string resourceGroup = "TesserProdEnvironment";
            string dataFactoryName = "TesserBackgroundJobs";
            string pipelineName = "CopyFilestoEDL";

            // Authenticate and create a data factory management client
            var context = new AuthenticationContext("https://login.windows.net/" + tenantID);
            ClientCredential cc = new ClientCredential(applicationId, authenticationKey);
            AuthenticationResult result = context.AcquireTokenAsync(
                "https://management.azure.com/", cc).Result;
            ServiceClientCredentials cred = new TokenCredentials(result.AccessToken);
            var client = new DataFactoryManagementClient(cred)
            {
                SubscriptionId = subscriptionId
            };

            // Create a pipeline run
            Console.WriteLine("Creating pipeline run...");
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                 { "BusinessFunctionName", BusinessFunction }
            };
            CreateRunResponse runResponse = client.Pipelines.CreateRunWithHttpMessagesAsync(
                resourceGroup, dataFactoryName, pipelineName, parameters: parameters
            ).Result.Body;
            Console.WriteLine("Pipeline run ID: " + runResponse.RunId);
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Accepted);

            return response;
        }
    }
}
