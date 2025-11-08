using FreelancerAssignment.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace FreelancerAssignment.Extensions;

public static class CustomResults
{
    public static IResult ToProblem(this Result result)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException("Cannet convert a successful result to a problem.");

        var problem = Results.Problem(statusCode: result.Error.Status);
        var problemDetails = problem.GetType().GetProperty(nameof(ProblemDetails))!.GetValue(problem) as ProblemDetails;

        problemDetails!.Extensions = new Dictionary<string, object?>()
        {
            {
                "errors", new Dictionary<string, IList<string>>
                {
                    { result.Error.Code, [result.Error.Description] }
                }
            }
        };
        return TypedResults.Problem(problemDetails);
    }
}
