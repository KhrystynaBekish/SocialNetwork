using DALNeo4j.Interface;
using DALNeo4j.Model;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace DALNeo4j.Concrete
{
	public class Neo4jUserRepository : INeo4jUserRepository
	{
		private readonly IDriver _driver;

		public Neo4jUserRepository(IDriver driver)
		{
			_driver = driver;
		}

		public async Task RegisterUserAsync(Neo4jUser user)
		{
			try
			{
				var existingUser = await GetUserByEmailAsync(user.Email);
				if (existingUser != null)
				{
					throw new Exception("User with this email already exists.");
				}

				var query = "CREATE (u:User {Id: $id, FirstName: $firstName, LastName: $lastName, Email: $email, Password: $password})";
				var parameters = new
				{
					id = user.Id,
					firstName = user.FirstName,
					lastName = user.LastName,
					email = user.Email,
					password = user.Password
				};

				using var session = _driver.AsyncSession();
				await session.RunAsync(query, parameters);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error registering user: {ex.Message}");
			}
		}

		public async Task DeleteUserAsync(string userId)
		{
			try
			{
				var query = "MATCH (u:User {Id: $id}) DELETE u";
				await ExecuteWriteTransactionAsync(async session =>
				{
					await session.RunAsync(query, new { id = userId });
				});
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error deleting user: {ex.Message}");
			}
		}

		public async Task<Neo4jUser> GetUserByEmailAsync(string email)
		{
			try
			{
				var query = "MATCH (u:User {Email: $email}) RETURN u LIMIT 1";
				var parameters = new { email };

				Neo4jUser user = null;

				await ExecuteReadTransactionAsync(async session =>
				{
					var result = await session.RunAsync(query, parameters);
					if (await result.FetchAsync())
					{
						var node = result.Current["u"].As<INode>();
						user = new Neo4jUser
						{
							Id = node.Properties["Id"].As<string>(),
							FirstName = node.Properties["FirstName"].As<string>(),
							LastName = node.Properties["LastName"].As<string>(),
							Email = node.Properties["Email"].As<string>(),
							Password = node.Properties["Password"].As<string>()
						};
					}
				});

				return user;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error retrieving user by email: {ex.Message}");
				return null;
			}
		}

		public async Task<List<Neo4jUser>> GetAllUsersAsync()
		{
			var users = new List<Neo4jUser>();
			try
			{
				var query = "MATCH (u:User) RETURN u";
				await ExecuteReadTransactionAsync(async session =>
				{
					var result = await session.RunAsync(query);
					while (await result.FetchAsync())
					{
						var node = result.Current["u"].As<INode>();
						users.Add(new Neo4jUser
						{
							Id = node.Properties["Id"].As<string>(),
							FirstName = node.Properties["FirstName"].As<string>(),
							LastName = node.Properties["LastName"].As<string>(),
							Email = node.Properties["Email"].As<string>(),
							Password = node.Properties["Password"].As<string>()
						});
					}
				});
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error retrieving all users: {ex.Message}");
			}

			return users;
		}

		public async Task SubscribeToUserAsync(string userId, string subscriberId)
		{
			try
			{
				var query = @"
            MATCH (u:User {Id: $userId}), (s:User {Id: $subscriberId})
            MERGE (s)-[:SUBSCRIBED]->(u)";

				await ExecuteWriteTransactionAsync(async session =>
				{
					await session.RunAsync(query, new { userId, subscriberId });
				});
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
				var query = @"
            MATCH (s:User {Id: $subscriberId})-[r:SUBSCRIBED]->(u:User {Id: $userId})
            DELETE r";

				await ExecuteWriteTransactionAsync(async session =>
				{
					await session.RunAsync(query, new { userId, subscriberId });
				});
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error unsubscribing from user: {ex.Message}");
			}
		}
		public async Task<bool> CheckSubscriptionAsync(string userId, string subscriberId)
		{
			try
			{
				using var session = _driver.AsyncSession();
				var query = "MATCH (u:User {Id: $userId})<-[:SUBSCRIBED]-(s:User {Id: $subscriberId}) RETURN COUNT(s) > 0 AS isSubscribed";
				var parameters = new { userId, subscriberId };
				var result = await session.RunAsync(query, parameters);
				var record = await result.SingleAsync();
				return record["isSubscribed"].As<bool>();
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
				var query = @"
			MATCH (start:User {Id: $userId}), (end:User {Id: $targetUserId})
			RETURN CASE
				WHEN start = end THEN 0
				ELSE length(shortestPath((start)-[:SUBSCRIBED*]-(end)))
			END AS distance";

				using var session = _driver.AsyncSession();
				var result = await session.RunAsync(query, new { userId, targetUserId });

				if (await result.FetchAsync())
				{
					var distance = result.Current["distance"].As<int?>();
					return distance.HasValue && distance > 0 ? distance : null;
				}

				return null;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Помилка при обчисленні відстані: {ex.Message}");
				return null;
			}
		}

		private async Task ExecuteWriteTransactionAsync(Func<IAsyncTransaction, Task> work)
		{
			using (var session = _driver.AsyncSession())
			{
				await session.WriteTransactionAsync(async tx =>
				{
					await work(tx);
				});
			}
		}

		private async Task ExecuteReadTransactionAsync(Func<IAsyncTransaction, Task> work)
		{
			using (var session = _driver.AsyncSession())
			{
				await session.ReadTransactionAsync(async tx =>
				{
					await work(tx);
				});
			}
		}
	}
}
