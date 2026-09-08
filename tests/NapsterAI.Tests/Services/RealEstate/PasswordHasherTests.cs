using NapsterAI.Api.Services.RealEstate;
using Xunit;

namespace NapsterAI.Tests.Services.RealEstate;

public class PasswordHasherTests
{
    [Fact]
    public void Verify_ReturnsTrue_ForCorrectPassword()
    {
        var hash = PasswordHasher.Hash("Correct-Horse-Battery-Staple1!");

        Assert.True(PasswordHasher.Verify("Correct-Horse-Battery-Staple1!", hash));
    }

    [Fact]
    public void Verify_ReturnsFalse_ForWrongPassword()
    {
        var hash = PasswordHasher.Hash("Correct-Horse-Battery-Staple1!");

        Assert.False(PasswordHasher.Verify("wrong-password", hash));
    }

    [Fact]
    public void Hash_ProducesDifferentOutput_ForSamePasswordOnDifferentCalls()
    {
        // Different random salt each time, so the stored hash string should never repeat -
        // this is what stops two users with the same password from having identical hashes.
        var hash1 = PasswordHasher.Hash("SamePassword1!");
        var hash2 = PasswordHasher.Hash("SamePassword1!");

        Assert.NotEqual(hash1, hash2);
        Assert.True(PasswordHasher.Verify("SamePassword1!", hash1));
        Assert.True(PasswordHasher.Verify("SamePassword1!", hash2));
    }

    [Theory]
    [InlineData("not-a-valid-hash")]
    [InlineData("")]
    [InlineData("only.two.parts.too.many")]
    public void Verify_ReturnsFalse_ForMalformedStoredHash(string malformed)
    {
        Assert.False(PasswordHasher.Verify("anything", malformed));
    }
}
