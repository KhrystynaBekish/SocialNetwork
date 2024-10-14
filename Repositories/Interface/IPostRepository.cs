using SocialNetworkModels;

namespace Repositories.Interface
{
	public interface IPostRepository
	{
		Task AddPostAsync(Post post);
		Task<List<Post>> GetPostsByUserAsync(string userId);
		Task<List<Post>> GetPostsBySubscribersAsync(List<string> subscriberIds);
		Task LikePostAsync(string postId, string userId);
		Task UnlikePostAsync(string postId, string userId);
		Task AddCommentToPostAsync(string postId, string commentId);
	}
}
