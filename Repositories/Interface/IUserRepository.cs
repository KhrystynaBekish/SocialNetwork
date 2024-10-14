using SocialNetworkModels;

namespace Repositories.Interface
{
	public interface IUserRepository
	{
		Task<User> GetUserByIdAsync(string userId);
		Task<User> GetUserByEmailAsync(string email);
		Task<List<User>> SearchUsersAsync(string firstName, string lastName);
		Task RegisterUserAsync(User user);
		Task SubscribeToUserAsync(string userId, string subscriberId);
		Task UnsubscribeFromUserAsync(string userId, string subscriberId);
	}
}
