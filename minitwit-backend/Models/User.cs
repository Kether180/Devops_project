namespace Minitwit.Models
{
    public class User
    {
        public int UserId { get; set; } = 0;
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public string PwHash { get; set; } = "";

    }
}
