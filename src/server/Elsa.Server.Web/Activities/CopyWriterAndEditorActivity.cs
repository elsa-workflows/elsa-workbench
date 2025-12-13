using System.Collections.Generic;
using System.Threading.Tasks;
using Elsa.Agents;
using Elsa.Workflows;
using Elsa.Extensions;
using Elsa.Server.Web.Agents;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;

namespace Elsa.Server.Web.Activities;

[Activity(Category = "Examples", DisplayName = "Copy Writer and Editor", Description = "Demonstrates how to use the OpenAI API to write a story.", Kind = ActivityKind.Task)]
public class CopyWriterAndEditorActivity : CodeActivity<string>
{
    [Input(Description = "The genre of the story.")] public Input<string> Genre { get; set; }

    [Input(Description = "The topic of the story.", DefaultValue = "A haunted House")] public Input<string> Topic { get; set; }

    [Input(Description = "The author of the story.")] public Input<string> Author { get; set; }

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var cancellationToken = context.CancellationToken;
        var genre = context.Get(Genre);
        var topic = context.Get(Topic);
        var author = context.Get(Author);
        var agentResolver = context.GetRequiredService<ICodeFirstAgentResolver>();
        
        var agent = await agentResolver.ResolveAsync<CopyWriterAndEditorAgent>(cancellationToken);
        agent.Author = author;
        agent.Genre = genre;
        agent.Topic = topic;
        var agentExecutionContext = new AgentExecutionContext
        {
            Message = $"Write a short story about {topic} in the genre of {genre}.",
            CancellationToken = cancellationToken
        };
        var response = await agent.RunAsync(agentExecutionContext);

        context.SetResult(response);
    }
}