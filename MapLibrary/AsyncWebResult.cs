namespace J4JSoftware.MapLibrary;

public record AsyncWebResult<TResult, TStatus>(
    TResult? ReturnValue,
    TStatus HttpStatusCode,
    string? Url = null,
    string? Message = null
)
    where TResult : class
    where TStatus : struct
{
    public bool IsValid => string.IsNullOrEmpty( Message );
}
