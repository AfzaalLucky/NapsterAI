namespace NapsterAI.Api.Models.Dtos;

/// <summary>
/// Simplified representation of a Napster stock companion (an AI persona
/// that can be attached to an agent or connection).
/// </summary>
public class CompanionDto
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PreviewUrl { get; set; }
    public string? VideoLoopUrl { get; set; }
    public string? Ethnicity { get; set; }
    public string? Gender { get; set; }
    public string? Headline { get; set; }
    public IReadOnlyDictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
    public string? Status { get; set; }
}
