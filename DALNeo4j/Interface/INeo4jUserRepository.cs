using DALNeo4j.Model;

namespace DALNeo4j.Interface
{
	public interface INeo4jUserRepository
	{
		Task RegisterUserAsync(Neo4jUser user);
		Task DeleteUserAsync(string userId);
		Task<Neo4jUser> GetUserByEmailAsync(string email);
		Task<List<Neo4jUser>> GetAllUsersAsync();
	}
}
