using Repositories.Interface;
using SocialNetworkModels;
using MongoDB.Driver;

namespace Repositories.Concrete
{
	public class CommentRepository : ICommentRepository
	{
		private readonly IMongoCollection<Comment> _comments;

		public CommentRepository(IMongoDatabase database)
		{
			_comments = database.GetCollection<Comment>("Comments");
		}

		public async Task AddCommentAsync(Comment comment)
		{
			await _comments.InsertOneAsync(comment);
		}

		public async Task<List<Comment>> GetCommentsByPostIdAsync(string postId)
		{
			return await _comments.Find(c => c.PostId == postId).ToListAsync();
		}

		public async Task DeleteCommentAsync(string postId, string commentId)
		{
			await _comments.DeleteOneAsync(c => c.PostId == postId && c.Id == commentId);
		}
	}
}
