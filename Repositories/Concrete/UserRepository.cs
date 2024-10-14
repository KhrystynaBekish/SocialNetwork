using MongoDB.Driver;
using Repositories.Interface;
using SocialNetworkModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Concrete
{
	public class UserRepository : IUserRepository
	{
		private readonly IMongoCollection<User> _users;

		public UserRepository(IMongoDatabase database)
		{
			_users = database.GetCollection<User>("Users");
		}

		public async Task<User> GetUserByIdAsync(string userId)
		{
			try
			{
				return await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error retrieving user by ID: {ex.Message}");
				return null;
			}
		}

		public async Task<List<User>> GetAllUsersAsync()
		{
			try
			{
				return await _users.Find(_ => true).ToListAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error retrieving all users: {ex.Message}");
				return new List<User>(); 
			}
		}


		public async Task<User> GetUserByEmailAsync(string email)
		{
			try
			{
				return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error retrieving user by email: {ex.Message}");
				return null;
			}
		}

		public async Task<List<User>> SearchUsersAsync(string firstName, string lastName)
		{
			try
			{
				return await _users.Find(u => u.FirstName == firstName && u.LastName == lastName).ToListAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error during user search: {ex.Message}");
				return new List<User>(); 
			}
		}

		public async Task RegisterUserAsync(User user)
		{
			try
			{
				var existingUser = await GetUserByEmailAsync(user.Email);
				if (existingUser != null)
				{
					throw new Exception("User with this email already exists.");
				}

				await _users.InsertOneAsync(user);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error registering user: {ex.Message}");
			}
		}

		public async Task UpdateUserAsync(User user)
		{
			try
			{
				var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
				var update = Builders<User>.Update
					.Set(u => u.Posts, user.Posts);

				await _users.UpdateOneAsync(filter, update);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error updating user: {ex.Message}");
			}
		}


		public async Task SubscribeToUserAsync(string userId, string subscriberId)
		{
			try
			{
				var user = await GetUserByIdAsync(userId);
				if (user == null)
				{
					throw new Exception("User not found.");
				}

				var update = Builders<User>.Update.AddToSet(u => u.Subscribers, subscriberId);
				await _users.UpdateOneAsync(u => u.Id == userId, update);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error subscribing to user: {ex.Message}");
			}
		}

		public async Task UnsubscribeFromUserAsync(string userId, string subscriberId)
		{
			try
			{
				var user = await GetUserByIdAsync(userId);
				if (user == null)
				{
					throw new Exception("User not found.");
				}

				var update = Builders<User>.Update.Pull(u => u.Subscribers, subscriberId);
				await _users.UpdateOneAsync(u => u.Id == userId, update);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error unsubscribing from user: {ex.Message}");
			}
		}

		public async Task<List<User>> SearchUsersByNameAsync(string name)
		{
			try
			{
				var filter = Builders<User>.Filter.Or(
					Builders<User>.Filter.Regex(u => u.FirstName, new MongoDB.Bson.BsonRegularExpression(name, "i")),
					Builders<User>.Filter.Regex(u => u.LastName, new MongoDB.Bson.BsonRegularExpression(name, "i"))
				);

				return await _users.Find(filter).ToListAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error during user search by name: {ex.Message}");
				return new List<User>();
			}
		}

		public async Task<List<User>> GetUsersSubscribedToAsync(string loggedInUserId)
		{
			try
			{
				var filter = Builders<User>.Filter.AnyEq(u => u.Subscribers, loggedInUserId);
				return await _users.Find(filter).ToListAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error retrieving users subscribed to the logged-in user: {ex.Message}");
				return new List<User>(); 
			}
		}

	}
}
