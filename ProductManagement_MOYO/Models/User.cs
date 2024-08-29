using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProductManagement_MOYO.Models
{
    public class UserAccount
    {
        [Key]
        public int Id { get; set; }

        
        public string? GitHubId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public string? OAuthProvider { get; set; }
        public string? OAuthId {get; set;}
        public string? Name { get; set; }
        public int RoleId { get; set; }

        // Navigation
        [JsonIgnore]
        public Role Role { get; set; }
        [JsonIgnore]
        public Vendor Vendor { get; set; }
        public List<Order> Orders { get; set; }

    }
}
