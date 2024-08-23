using System.ComponentModel.DataAnnotations;

namespace ProductManagement_MOYO.Models
{
    public class UserAccount
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string GitHubId { get; set; }

        [Required]
        public string? Username { get; set; }

        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public string? OAuthProvider { get; set; }
        public string? OAuthId {get; set;}
        public string? Name { get; set; }

        [Required]
        public string Role { get; set; }
    }
}
