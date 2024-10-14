using MongoDB.Driver;
using Repositories.Interface;
using SocialNetworkModels;

namespace Repositories.Concrete
{
	public class PostRepository : IPostRepository
	{
		private readonly IMongoCollection<Post> _posts;

		public PostRepository(IMongoDatabase database)
		{
			_posts = database.GetCollection<Post>("Posts");
		}

		public async Task AddPostAsync(Post post)
		{
			if (_posts != null)
			{
				await _posts.InsertOneAsync(post);
			}
			else
			{
				throw new Exception("Posts collection not initialized.");
			}
		}

		public async Task<List<Post>> GetPostsByUserAsync(string userId)
		{
			try
			{
				return await _posts.Find(p => p.UserId == userId)
								   .SortByDescending(p => p.Date)
								   .ToListAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error retrieving posts for user {userId}: {ex.Message}");
				return new List<Post>(); 
			}
		}


		public async Task<List<Post>> GetPostsBySubscribersAsync(List<string> subscriberIds)
		{
			return await _posts.Find(p => subscriberIds.Contains(p.UserId))
							   .SortByDescending(p => p.Date)
							   .ToListAsync();
		}

		public async Task LikePostAsync(string postId, string userId)
		{
			var update = Builders<Post>.Update.AddToSet(p => p.Likes, userId).Inc(p => p.CountLikes, 1);
			await _posts.UpdateOneAsync(p => p.Id == postId, update);
		}

		public async Task UnlikePostAsync(string postId, string userId)
		{
			var update = Builders<Post>.Update.Pull(p => p.Likes, userId).Inc(p => p.CountLikes, -1);
			await _posts.UpdateOneAsync(p => p.Id == postId, update);
		}

		public async Task AddCommentToPostAsync(string postId, string commentId)
		{
			var update = Builders<Post>.Update.AddToSet(p => p.Comments, commentId).Inc(p => p.CountComments, 1);
			await _posts.UpdateOneAsync(p => p.Id == postId, update);
		}
		public async Task<Post> GetPostByIdAsync(string postId)
		{
			var filter = Builders<Post>.Filter.Eq(p => p.Id, postId);
			return await _posts.Find(filter).FirstOrDefaultAsync();
		}

		public async Task UpdatePostAsync(Post post)
		{
			var filter = Builders<Post>.Filter.Eq(p => p.Id, post.Id);
			await _posts.ReplaceOneAsync(filter, post);
		}

	}
}
