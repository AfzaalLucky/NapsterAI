using System.Net;

namespace NapsterAI.Api.Exceptions;

/// <summary>
/// Thrown whenever a call to the upstream Napster Companion API fails - a non-success
/// status code, a network/timeout error, or a response we could not parse.
/// The middleware maps this to an appropriate HTTP response for our own clients.
/// </summary>
public class NapsterApiException : Exception
{
    /// <summary>
    /// Status code returned by Napster, when one is available. Null for
    /// network-level failures (timeouts, DNS errors, etc.).
    /// </summary>
    public HttpStatusCode? UpstreamStatusCode { get; }

    public NapsterApiException(string message, HttpStatusCode? upstreamStatusCode = null, Exception? innerException = null)
        : base(message, innerException)
    {
        UpstreamStatusCode = upstreamStatusCode;
    }
}

/// <summary>
/// Thrown when a request supplied by the caller of our API is invalid
/// (e.g. a missing companion id or name) before we even call Napster, or
/// when Napster itself rejects the request as malformed (HTTP 400).
/// </summary>
public class InvalidRequestException : Exception
{
    public InvalidRequestException(string message) : base(message)
    {
    }
}

/// <summary>
/// Thrown when a requested resource (companion/agent/session) does not exist upstream.
/// </summary>
public class NapsterResourceNotFoundException : Exception
{
    public NapsterResourceNotFoundException(string message) : base(message)
    {
    }
}

/// <summary>
/// Thrown when Napster reports a 409 Conflict, e.g. creating an agent that
/// collides with an existing one.
/// </summary>
public class NapsterConflictException : Exception
{
    public NapsterConflictException(string message) : base(message)
    {
    }
}
