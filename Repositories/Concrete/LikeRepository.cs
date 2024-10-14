using MongoDB.Driver;
using Repositories.Interface;
using SocialNetworkModels;

namespace Repositories.Concrete
{
	public class LikeRepository : ILikeRepository
	{
		private readonly IMongoCollection<Like> _likes;

		public LikeRepository(IMongoDatabase database)
		{
			_likes = database.GetCollection<Like>("Likes");
		}

		public async Task AddLikeAsync(Like like)
		{
			await _likes.InsertOneAsync(like);
		}

		public async Task RemoveLikeAsync(string postId, string userId)
		{
			await _likes.DeleteOneAsync(l => l.PostId == postId && l.UserId == userId);
		}

		public async Task<List<Like>> GetLikesByPostIdAsync(string postId)
		{
			return await _likes.Find(l => l.PostId == postId).ToListAsync();
		}
	}
}
