using System.Threading;
using System.Threading.Tasks;
using Elsa.Agents;
using Elsa.Server.Web.Activities;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Elsa.Server.Web.Agents;

/// <summary>
/// Represents an AI-driven story-writing agent that leverages an external chat client
/// to generate stories based on the specified author, topic, and genre.
/// This agent is completely unrelated to Elsa and can be instantiated and executed by any component.
/// Including the <see cref="WriteStory"/> sample activity.
/// </summary>
public class NativeStoryWriterAgent(IChatClient chatClient, ILoggerFactory loggerFactory)
{
    public string Author { get; set; }
    public string Topic { get; set; }
    public string Genre { get; set; }
    
    public async Task<AgentRunResponse> RunAsync(CancellationToken cancellationToken = default)
    {
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

