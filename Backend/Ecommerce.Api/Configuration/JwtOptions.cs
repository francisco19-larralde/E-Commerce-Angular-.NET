using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    [MinLength(32)]
    public required string Key { get; init; }

    [Required]
    public required string Issuer { get; init; }

    [Required]
    public required string Audience { get; init; }

    [Range(1, 1440)]
    public int ExpiracionMinutos { get; init; }
}
