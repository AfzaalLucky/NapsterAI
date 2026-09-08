namespace NapsterAI.Api.Models.Dtos;

/// <summary>
/// Body for POST /api/knowledge-bases. Mirrors POST /public/knowledge-bases -
/// a container of reference documents an agent can be pointed at.
/// </summary>
public class CreateKnowledgeBaseRequestDto
{
    public string Name { get; set; } = string.Empty;

    /// <summary>One of: azureOpenAI, gemini, openAI, humain, microsoftFoundry. Defaults to azureOpenAI if omitted.</summary>
    public string? Provider { get; set; }
}

public class KnowledgeBaseDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Provider { get; set; }
    public int? ItemsCount { get; set; }
    public IReadOnlyDictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

    /// <summary>Epoch timestamp as returned by Napster; the docs don't specify seconds vs. milliseconds.</summary>
    public long? Created { get; set; }
}
