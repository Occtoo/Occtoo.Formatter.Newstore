using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Occtoo.Formatter.Newstore.Services;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Net;
using System.Xml.Linq;

[assembly: FunctionsStartup(typeof(Occtoo.Formatter.Newstore.Startup))]

namespace Occtoo.Formatter.Newstore
{
    internal class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddTransient<ITokenService, OcctooTokenService>();
            builder.Services.AddTransient<IBlobService, BlobService>();
            builder.Services.AddTransient<INewStoreService, NewStoreService>();
            builder.Services.AddTransient<IOcctooService, OcctooService>();

            builder.Services.AddHttpClient("Default")
                .AddPolicyHandler(HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
                    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

            builder.Services.AddAzureClients(builder =>
            {
                var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

                builder.AddTableServiceClient(connectionString);
                builder.AddBlobServiceClient(connectionString);
                builder.AddQueueServiceClient(connectionString)
                    .ConfigureOptions(options => options.MessageEncoding = QueueMessageEncoding.Base64);
            });
        }
    }
}
