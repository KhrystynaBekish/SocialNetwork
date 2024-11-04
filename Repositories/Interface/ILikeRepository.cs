using SocialNetworkModels;

namespace Repositories.Interface
{
	public interface ILikeRepository
	{
		Task AddLikeAsync(Like like);
		Task RemoveLikeAsync(string postId, string userId);
		Task<List<Like>> GetLikesByPostIdAsync(string postId);
	}
}
