using System;
using System.ClientModel;
using System.Threading.Tasks;
using Elsa.Workflows;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using System.ComponentModel;
using Elsa.Extensions;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Logging;

namespace Elsa.Server.Web.Activities;

[Activity(Category = "Examples", DisplayName = "Copy Writer and Editor", Description = "Demonstrates how to use the OpenAI API to write a story.", Kind = ActivityKind.Task)]
public class CopyWriterAndEditorActivity : CodeActivity<string>
{
    [Input(Description = "The genre of the story.")]
    public Input<string> Genre { get; set; }
    
    [Input(Description = "The topic of the story.", DefaultValue = "A haunted House")]
    public Input<string> Topic { get; set; }
    
    [Input(Description = "The author of the story.")]
    public Input<string> Author { get; set; }
    
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var chatClient = new ChatClient(
                "gpt-4o-mini",
                new ApiKeyCredential(Environment.GetEnvironmentVariable("GITHUB_TOKEN")!),
                new() { Endpoint = new("https://models.github.ai/inference") })
            .AsIChatClient();
        
        var genre = context.Get(Genre);
        var topic = context.Get(Topic);
        var author = context.Get(Author);
        var loggerFactory = context.GetRequiredService<ILoggerFactory>();
        
        var writer = new ChatClientAgent(
            chatClient,
            new()
            {
                Name = "Writer",
                ChatOptions = new()
                {
                    Instructions = $"Write stories that are engaging and creative.",
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
                ChatOptions = new() { Instructions = "Make the story more engaging, fix grammar, and enhance the plot. " }
            },
            loggerFactory);

        // Create a workflow that connects writer to editor
        var workflow = AgentWorkflowBuilder.BuildSequential(writer, editor);
        var workflowAgent = workflow.AsAgent();
        var workflowResponse = await workflowAgent.RunAsync($"Write a short story about {topic} in the genre of {genre}.");

        context.SetResult(workflowResponse.Text);
        
        [Description("Gets the author of the story.")]
        string GetAuthor() => author;
    }

    [Description("Formats the story for display.")]
    string FormatStory(string title, string author, string story) => $"Title: {title}\nAuthor: {author}\n\n{story}";
}