namespace NapsterAI.Api.Models.Dtos;

/// <summary>One question/answer pair submitted as part of creating an FAQ collection.</summary>
public class FaqItemRequestDto
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
}

/// <summary>
/// Body for POST /api/faqs. Mirrors POST /public/faqs - a named collection of
/// question/answer pairs an agent can be pointed at (`faqCollections` on Create Agent).
/// </summary>
public class CreateFaqCollectionRequestDto
{
    public string Name { get; set; } = string.Empty;
    public IReadOnlyList<FaqItemRequestDto>? Faqs { get; set; }
}

public class FaqCollectionDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? ItemsCount { get; set; }

    /// <summary>Epoch timestamp as returned by Napster; the docs don't specify seconds vs. milliseconds.</summary>
    public long? Created { get; set; }
}

public class FaqItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;

    /// <summary>Epoch timestamp as returned by Napster; the docs don't specify seconds vs. milliseconds.</summary>
    public long? CreatedAt { get; set; }
}
