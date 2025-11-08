using System.ComponentModel.DataAnnotations;

public class GoogleTokenRequest
{
    [Required]
    public string Token { get; set; }
}
