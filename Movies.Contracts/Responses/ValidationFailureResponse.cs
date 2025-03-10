namespace Movies.Contracts.Responses;

public class ValidationFailureResponse
{
    public required IEnumerable<ValidationResponse> Errors { get; init; }
}

public record ValidationResponse
{
    public required string PropertyName { get; init; }
    
    public required string Message { get; init; }
}