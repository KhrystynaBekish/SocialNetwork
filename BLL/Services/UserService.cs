using Repositories.Concrete;
using DALNeo4j.Concrete;
using DALNeo4j.Model;
using SocialNetworkModels;

namespace BLL.Services
{
	public class UserService
	{
		private readonly UserRepository _userRepository;
		private readonly Neo4jUserRepository _neo4jUserRepository;

		public UserService(UserRepository userRepository, Neo4jUserRepository neo4jUserRepository)
		{
			_userRepository = userRepository;
			_neo4jUserRepository = neo4jUserRepository;
		}

		public async Task RegisterUserAsync(User user)
		{
			try
			{
				var existingUser = await _userRepository.GetUserByEmailAsync(user.Email);
				if (existingUser != null)
				{
					throw new Exception("User with this email already exists in MongoDB.");
				}

				await _userRepository.RegisterUserAsync(user);

				var neo4jUser = new Neo4jUser
				{
					Id = user.Id,
					FirstName = user.FirstName,
					LastName = user.LastName,
					Email = user.Email,
					Password = user.Password 
				};
				await _neo4jUserRepository.RegisterUserAsync(neo4jUser);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error during registration: {ex.Message}");
			}
		}

		public async Task SubscribeToUserAsync(string userId, string subscriberId)
		{
			try
			{
				await _userRepository.SubscribeToUserAsync(userId, subscriberId);

				await _neo4jUserRepository.SubscribeToUserAsync(userId, subscriberId);

				Console.WriteLine($"Successfully subscribed to the user with ID: {userId}.");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error subscribing to the user: {ex.Message}");
			}
		}

		public async Task UnsubscribeFromUserAsync(string userId, string subscriberId)
		{
			try
			{
				await _userRepository.UnsubscribeFromUserAsync(userId, subscriberId);

				await _neo4jUserRepository.UnsubscribeFromUserAsync(userId, subscriberId);

				Console.WriteLine($"Successfully unsubscribed from the user with ID: {userId}.");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error unsubscribing from the user: {ex.Message}");
			}
		}

		public async Task<bool> IsSubscribedToUserAsync(string userId, string subscriberId)
		{
			try
			{
				return await _neo4jUserRepository.CheckSubscriptionAsync(userId, subscriberId);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error checking subscription: {ex.Message}");
				return false;
			}
		}

		public async Task<int?> GetDistanceToUserAsync(string userId, string targetUserId)
		{
			try
			{
				return await _neo4jUserRepository.GetDistanceToUserAsync(userId, targetUserId);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in distance calculation: {ex.Message}");
				return null;
			}
		}

	}
}
