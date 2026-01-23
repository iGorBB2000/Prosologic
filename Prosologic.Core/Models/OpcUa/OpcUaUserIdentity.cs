namespace Prosologic.Core.Models.OpcUa
{
    public class OpcUaUserIdentity
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }
}
