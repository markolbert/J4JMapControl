namespace J4JSoftware.MapLibrary;

public record ImageRetrievalResult(object? ImageSource, string? Message)
{
    public bool IsValid => ImageSource != null;
}
