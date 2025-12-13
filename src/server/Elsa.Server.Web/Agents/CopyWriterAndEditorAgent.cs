using System.Threading.Tasks;
using Elsa.Agents;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Elsa.Server.Web.Agents;

public class CopyWriterAndEditorAgent(IChatClient chatClient, ILoggerFactory loggerFactory) : IElsaAgent
{
    public string Author { get; set; }
    public string Topic { get; set; }
    public string Genre { get; set; }
    
    public async Task<IAgentExecutionResponse> RunAsync(IAgentExecutionContext context)
    {
        // Local tools for the writer agent.
        string GetAuthor() => Author;
        string FormatStory(string title, string a, string story) => $"Title: {title}\nAuthor: {a}\n\n{story}";

        var writer = new ChatClientAgent(
            chatClient,
            new()
            {
                Name = "Writer",
                ChatOptions = new()
                {
                    Instructions = "Write stories that are engaging and creative.",
                    Tools =
                    [
                        AIFunctionFactory.Create(GetAuthor),
                        AIFunctionFactory.Create(FormatStory)
                    ],
                }
            },
            loggerFactory);

        var editor = new ChatClientAgent(
            chatClient,
            new()
            {
                Name = "Editor",
                ChatOptions = new() { Instructions = "Make the story more engaging, fix grammar, and enhance the plot." }
            },
            loggerFactory);

        var workflow = AgentWorkflowBuilder.BuildSequential(writer, editor);
        var workflowAgent = workflow.AsAgent();
        var cancellationToken = context.CancellationToken;
        var response = await workflowAgent.RunAsync($"Write a story about {Topic} in the genre of {Genre} written by {Author}.", cancellationToken: cancellationToken);
        return new AgentExecutionResponse
        {
            Text = response.Text
        };
    }
}

