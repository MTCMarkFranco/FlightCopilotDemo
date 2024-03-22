using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Plugins.Core;
using CustomCopilot.Plugins.FlightTrackerPlugin;

namespace CustomCopilot
{
    internal class Program
    {

        static async Task Main(string[] args)

        {
            

            var configBuilder = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            var skBuilder = Kernel.CreateBuilder();
            skBuilder.AddAzureOpenAIChatCompletion("GPT4",
            configBuilder["AOI_ENDPOINT"] ?? string.Empty,
            configBuilder["AOI_KEY"] ?? string.Empty);
             
            // adding OOTB and Custom PLugins
            #pragma warning disable SKEXP0050
            skBuilder.Plugins.AddFromType<TimePlugin>();
            skBuilder.Plugins.AddFromObject(
                new FlightTrackerPlugin(
                        configBuilder["TRACKER_KEY"] ?? string.Empty), 
                        nameof(FlightTrackerPlugin)
                        );

            // Build the kernel
            var kernel = skBuilder.Build();


             // Create chat history
            ChatHistory history = [];
            history.AddSystemMessage(@"You're a virtual assistant that helps people track flight and find information.");

            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            // Start the conversation
            while (true)
            {
                // Get user input
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("User > ");
                history.AddUserMessage(Console.ReadLine()!);

                // Enable auto function calling
                OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
                {
                    MaxTokens = 200,
                    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
                };

                // Get the response from the AI
                var response = chatCompletionService.GetStreamingChatMessageContentsAsync(
                               history,
                               executionSettings: openAIPromptExecutionSettings,
                               kernel: kernel);


                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("\nAssistant > ");

                string combinedResponse = string.Empty;
                await foreach (var message in response)
                {
                    //Write the response to the console
                    Console.Write(message);
                    combinedResponse += message;
                }

                Console.WriteLine();

                // Add the message from the agent to the chat history
                history.AddAssistantMessage(combinedResponse);
            }
        }
    }
}