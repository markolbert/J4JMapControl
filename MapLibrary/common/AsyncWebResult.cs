namespace J4JSoftware.MapLibrary;

public record AsyncWebResult<TResult>(
    TResult? ReturnValue,
    int HttpStatusCode,
    string? Url = null,
    string? Message = null
)
    where TResult : class;
