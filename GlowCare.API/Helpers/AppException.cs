namespace GlowCare.API.Helpers;

/// <summary>
/// A business-rule error raised by a service layer.
/// A global exception middleware (added in Phase 8) converts it into an
/// HTTP response with the given status code and message, so services never
/// have to build HTTP responses themselves.
/// </summary>
public class AppException : Exception
{
    /// <summary>HTTP status code the API should return for this error.</summary>
    public int StatusCode { get; }

    public AppException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}
