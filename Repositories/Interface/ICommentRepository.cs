using SocialNetworkModels;

namespace Repositories.Interface
{
	public interface ICommentRepository
	{
		Task AddCommentAsync(Comment comment);
		Task<List<Comment>> GetCommentsByPostIdAsync(string postId);
		Task DeleteCommentAsync(string postId, string commentId);
	}
}
