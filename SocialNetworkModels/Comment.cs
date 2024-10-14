namespace SocialNetworkModels
{
	public class Comment
	{
		public string Id { get; set; }
		public string PostId { get; set; }
		public string UserId { get; set; }
		public string Text { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
