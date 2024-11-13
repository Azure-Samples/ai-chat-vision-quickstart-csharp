using Azure.AI.Inference;
using Azure;
using Microsoft.Extensions.AI;
using OpenAI;
using Azure.AI.OpenAI;
using Azure.Identity;

namespace AIChatImgApp.Extensions
{
    /// <summary>
    /// Extension methods for configuring the client options. This will provide the explicit implementation for <see cref="IChatClient"/>.
    /// </summary>
    public static class ClientConfigurationExtensions
    {
        public static WebApplicationBuilder ConfigureClientOptions(this WebApplicationBuilder builder)
        {
            var client = (builder.Configuration["AIHost"] ?? "local").ToLower();

            if (client == "local")
            {
                // Ollama implementation
                return builder.AddOllamaChatCompletion();
            }

            var modelId = builder.Configuration["REMOTE_MODEL_OR_DEPLOYMENT_ID"] ?? throw new Exception("Missing configuration REMOTE_MODEL_OR_DEPLOYMENT_ID");

            // OpenAI is a special case and doesn't require an endpoint
            if (client == "openai")
            {
                // OpenAI implementation
                IChatClient chatClientOpenAI = new OpenAIClient(builder.Configuration["OPENAI_KEY"] ?? throw new Exception("Missing configuration OPENAI_KEY")).AsChatClient(modelId);
                builder.Services.AddSingleton(chatClientOpenAI);
                return builder;
            }

            if (client == "azureopenai")
            {
                var azureEndpoint = builder.Configuration["AZURE_OPENAI_ENDPOINT"] ?? throw new Exception("Missing configuration AZURE_OPENAI_ENDPOINT");
                var azureDeploymentId = builder.Configuration["AZURE_OPENAI_DEPLOYMENT"] ?? throw new Exception("Missing configuration AZURE_OPENAI_DEPLOYMENT");

                // Azure OpenAI implementation using Entra ID
                IChatClient chatClientAzureOpenAI = new AzureOpenAIClient(
                    new Uri(azureEndpoint),
                    new DefaultAzureCredential()).AsChatClient(azureDeploymentId);
                builder.Services.AddSingleton(chatClientAzureOpenAI);
                return builder;
            }

            // preferred for these to be user secrets or environment variables
            var remoteEndpoint = builder.Configuration["REMOTE_ENDPOINT"] ?? throw new Exception("Missing configuration REMOTE_ENDPOINT");
            var key = builder.Configuration[client == "github" ? "GITHUB_TOKEN" : "AZURE_INFERENCE_KEY"] ?? throw new Exception($"Missing key for client {client}");

            // Azure Inference also works with GitHub models
            IChatClient chatClient = new ChatCompletionsClient(
                new Uri(remoteEndpoint),
                new AzureKeyCredential(key)).AsChatClient(modelId);
            builder.Services.AddSingleton(chatClient);
            return builder;
        }

        private static WebApplicationBuilder AddOllamaChatCompletion(this WebApplicationBuilder builder)
        {
            string modelId = builder.Configuration["LOCAL_MODEL_NAME"] ?? throw new Exception("Missing configuration LOCAL_MODEL_NAME");
            string endpoint = builder.Configuration["LOCAL_ENDPOINT"] ?? throw new Exception("Missing configuration LOCAL_ENDPOINT");
            IChatClient ollama = new OllamaChatClient(endpoint, modelId);
            builder.Services.AddSingleton(ollama);
            return builder;
        }
    }
}
