namespace SocialNetworkModels
{
	public class User
	{
		public string Id { get; set; } 
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public string Password { get; set; } 
		public List<string> Interests { get; set; } = new List<string>();
		public List<string> Subscribers { get; set; } = new List<string>();
		public List<string> Posts { get; set; } = new List<string>();
	}
}
