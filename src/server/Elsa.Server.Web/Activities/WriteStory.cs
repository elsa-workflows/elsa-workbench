using System.Threading.Tasks;
using Elsa.Agents;
using Elsa.Workflows;
using Elsa.Extensions;
using Elsa.Server.Web.Agents;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;

namespace Elsa.Server.Web.Activities;

/// <summary>
/// Represents an activity that utilizes the OpenAI API to create a story based on the specified genre, topic, and author.
/// </summary>
/// <remarks>
/// This activity is categorized under "Examples" and is designated as a Task activity.
/// Upon execution, it interacts with a <see cref="NativeStoryWriterAgent"/> to generate story content in line with the provided inputs.
/// </remarks>
[Activity(Category = "Examples", DisplayName = "Copy Writer and Editor", Description = "Demonstrates how to use the OpenAI API to write a story.", Kind = ActivityKind.Task)]
public class WriteStory : CodeActivity<string>
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
        var agent = context.GetRequiredService<NativeStoryWriterAgent>();
        agent.Author = author;
        agent.Genre = genre;
        agent.Topic = topic;
        var response = await agent.RunAsync(cancellationToken);

        context.SetResult(response);
    }
}