using System.Threading;
using System.Threading.Tasks;
using Elsa.Workflows.Runtime;
using FastEndpoints;

namespace Elsa.Server.Agents.Web.Endpoints.LoanRequests.Review;

public class Endpoint(ITaskReporter taskReporter) : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Post("/loan-requests/review/{TaskId}");
        AllowAnonymous();
    }

    public override async Task<Response> ExecuteAsync(Request req, CancellationToken ct)
    {
        var taskId = Route<string>("TaskId");
        await taskReporter.ReportCompletionAsync(taskId, req, ct);
        return await Task.FromResult(new Response());
    }
}

public class Request
{
    public string Recommendation { get; set; }
    public string RecommendationReasoning { get; set; }
}

public class Response
{
}