using System;
using System.Threading.Tasks;
using Elsa.Extensions;
using Elsa.Workflows;

namespace Elsa.Server.Web.Agents;

public class ResumableAgent
{
    public string Start(ActivityExecutionContext context)
    {
        Console.WriteLine("Resumable agent started.");
        var bookmark = context.CreateBookmark(Resume);
        var bookmarkToken = context.GenerateBookmarkTriggerToken(bookmark.Id);
        return bookmarkToken;
    }

    private async ValueTask Resume(ActivityExecutionContext context)
    {
        Console.WriteLine("Resuming activity.");
        await context.CompleteActivityAsync();
        Console.WriteLine("Activity resumed.");
    }
}