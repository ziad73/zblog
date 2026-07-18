namespace Models.Auth;
public class RefreshToken
{
    public Guid Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; } = Guid.Empty;
    public DateTime Created { get; set; }
    public DateTime Expires { get; set; }
    public DateTime? Revoked { get; set; }// used/caneceled

    // When this token is rotated, we record the token that replaced it.
    public string? ReplacedByToken { get; set; }

    // A token is only usable if it has not been revoked and has not expired.
    public bool IsActive => Revoked is null && DateTime.UtcNow < Expires;
}
