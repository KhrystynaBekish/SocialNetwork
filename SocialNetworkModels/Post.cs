namespace SocialNetworkModels
{
	public class Post
	{
		public string Id { get; set; } 
		public string UserId { get; set; }
		public string Description { get; set; }
		public DateTime Date { get; set; }
		public List<string> Likes { get; set; }
		public int CountLikes { get; set; }
		public List<string> Comments { get; set; }
		public int CountComments { get; set; }
	}
}
