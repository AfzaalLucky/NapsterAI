using System.Net;

namespace NapsterAI.Tests.Services;

/// <summary>
/// Minimal stand-in for the real network so NapsterService can be tested
/// without making actual HTTP calls. Give it a canned response (or an
/// exception to throw) and it hands that back for every request.
/// </summary>
internal class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _content;
    private readonly Exception? _exceptionToThrow;

    public FakeHttpMessageHandler(HttpStatusCode statusCode, string content)
    {
        _statusCode = statusCode;
        _content = content;
    }

    public FakeHttpMessageHandler(Exception exceptionToThrow)
    {
        _exceptionToThrow = exceptionToThrow;
        _statusCode = HttpStatusCode.OK;
        _content = string.Empty;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_exceptionToThrow is not null)
        {
            throw _exceptionToThrow;
        }

        var response = new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_content, System.Text.Encoding.UTF8, "application/json")
        };

        return Task.FromResult(response);
    }
}
