using System.Threading.Tasks;
using Elsa.Agents;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Elsa.Server.Web.Agents;

/// <summary>
/// Represents an AI-driven story-writing agent that leverages an external chat client
/// to generate stories based on the specified author, topic, and genre.
/// Implement <see cref="IAgent"/> is optional, but when doing so, it can be automatically used as an activity from Elsa.
/// </summary>
public class DecoratedStoryWriterAgent(IChatClient chatClient, ILoggerFactory loggerFactory)
{
    public string Author { get; set; }
    public string Topic { get; set; }
    public string Genre { get; set; }
    
    public Task<string> ContemplateAsync(string story) => Task.FromResult(story);
    
    public async Task<AgentRunResponse> WriteAsync(AgentExecutionContext context)
    {
        var cancellationToken = context.CancellationToken;
        
        // Local tools for the writer agent.
        string GetAuthor() => Author;
        string FormatStory(string title, string a, string story) => $"Title: {title}\nAuthor: {a}\n\n{story}";
        
        var writer = chatClient.CreateAIAgent(
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

        var editor = chatClient.CreateAIAgent(
            new()
            {
                Name = "Editor",
                ChatOptions = new()
                {
                    
                    Instructions = "Make the story more engaging, fix grammar, and enhance the plot."
                }
            },
            loggerFactory);

        var narrativeOrchestrator = AgentWorkflowBuilder.BuildSequential(writer, editor);
        var narrativeOrchestratorAgent = narrativeOrchestrator.AsAgent();
        var response = await narrativeOrchestratorAgent.RunAsync($"Write a story about {Topic} in the genre of {Genre} written by {Author}.", cancellationToken: cancellationToken);
        return response;
    }
}

