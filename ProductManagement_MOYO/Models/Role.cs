using System.Text.Json.Serialization;

namespace ProductManagement_MOYO.Models
{
    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }

        [JsonIgnore]
        public List<UserAccount> UserAccounts { get; set; }
    }
}
