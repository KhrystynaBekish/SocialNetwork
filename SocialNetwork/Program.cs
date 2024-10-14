using MongoDB.Driver;
using Repositories.Concrete;
using Repositories.Interface;
using SocialNetworkModels;

namespace SocialNetwork
{
	class Program
	{
		private static UserRepository _userRepository;
		private static PostRepository _postRepository;
		private static CommentRepository _commentRepository;
		private static LikeRepository _likeRepository;
		private static User _loggedInUser;

		static async Task Main(string[] args)
		{
			var connectionString = "mongodb+srv://Khrystyna:12rnkk!098@atlascluster.zq8rb.mongodb.net/";
			var client = new MongoClient(connectionString);
			var database = client.GetDatabase("SocialNetwork");

			_userRepository = new UserRepository(database);
			_postRepository = new PostRepository(database);
			_commentRepository = new CommentRepository(database);
			_likeRepository = new LikeRepository(database);

			Console.WriteLine("Welcome to Social Network!\n");
			char option = 's';

			while (option != '0')
			{
				if (_loggedInUser == null)
				{
					Console.WriteLine("1 - Sign in" +
									  "\n2 - Sign up" +
									  "\n0 - Exit \n");

					Console.Write("\nPlease select an option: ");
					string selectedOption = Console.ReadLine() ?? "";

					if (string.IsNullOrWhiteSpace(selectedOption) || selectedOption.Trim().Length > 1)
					{
						Console.WriteLine("Incorrect option selected!");
						continue;
					}

					option = Convert.ToChar(selectedOption.Trim());
					switch (option)
					{
						case '1':
							await Login();
							break;
						case '2':
							await Registration();
							break;
						case '0':
							return;
						default:
							Console.WriteLine("Incorrect option selected!");
							break;
					}
				}
				else
				{
					await ShowUserFeed();
				}
			}
		}
		static async Task Registration()
		{
			try
			{
				Console.WriteLine("Please enter your first name:");
				string firstName = Console.ReadLine() ?? "";

				Console.WriteLine("Please enter your last name:");
				string lastName = Console.ReadLine() ?? "";

				Console.WriteLine("Please enter your email:");
				string email = Console.ReadLine() ?? "";

				Console.WriteLine("Please enter your password:");
				string password = Console.ReadLine() ?? "";

				Console.WriteLine("Enter your interests separated by commas:");
				string interestsInput = Console.ReadLine() ?? "";
				List<string> interests = interestsInput.Split(',').Select(i => i.Trim()).ToList();

				var newUser = new User
				{
					Id = Guid.NewGuid().ToString(),
					FirstName = firstName,
					LastName = lastName,
					Email = email,
					Password = password,
					Interests = interests,
					Subscribers = new List<string>(),
					Posts = new List<string>()
				};

				await _userRepository.RegisterUserAsync(newUser);
				Console.WriteLine("Registration successful!");
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error during registration: " + ex.Message);
			}
		}

		static async Task Login()
		{
			Console.WriteLine("Please enter your email:");
			string email = Console.ReadLine() ?? "";

			Console.WriteLine("Please enter your password:");
			string password = Console.ReadLine() ?? "";

			try
			{
				var user = await _userRepository.GetUserByEmailAsync(email);

				if (user != null && user.Password == password)
				{
					_loggedInUser = user;
					Console.WriteLine($"Welcome, {user.FirstName} {user.LastName}!");
				}
				else
				{
					Console.WriteLine("Incorrect email or password.");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error during login: " + ex.Message);
			}
		}

		static async Task ShowUserFeed()
		{
			char option = 's';

			while (option != '0')
			{
				Console.WriteLine($"\n\n-----------------------------   (s)earch users   -----------------------------");
				Console.WriteLine("\nPosts Feed:\n");

				var posts = await DisplaySubscribedUsersPosts();

				Console.WriteLine("\n-----------------------------   to reply to a post, enter its number   -----------------------------");
				Console.WriteLine("\n-----------------------------------------   (n)ew post   -----------------------------");
				Console.WriteLine("                                         (L)ogout");

				Console.Write("\nPlease select an option: ");
				string selectedOption = Console.ReadLine() ?? "";

				if (string.IsNullOrWhiteSpace(selectedOption))
				{
					Console.WriteLine("Incorrect option selected!");
					continue;
				}

				if (selectedOption.ToLower() == "n")
				{
					await CreateNewPost();
				}
				else if (selectedOption.ToLower() == "s")
				{
					await SearchUsers();
				}
				else if (selectedOption.ToLower() == "l")
				{
					_loggedInUser = null;
					Console.WriteLine("You have logged out.");
					break;
				}
				else if (int.TryParse(selectedOption, out int postIndex) && postIndex > 0 && postIndex <= posts.Count)
				{
					await DisplayPostDetails(posts[postIndex - 1]);
				}
				else
				{
					Console.WriteLine("Invalid option selected.");
				}
			}
		}

		static async Task ViewUserProfile(User user)
		{
			Console.WriteLine($"\nProfile of {user.FirstName} {user.LastName}:");
			Console.WriteLine($"Interests: {string.Join(", ", user.Interests)}");
			Console.WriteLine($"Subscribers: {user.Subscribers.Count}");

			var userPosts = await _postRepository.GetPostsByUserAsync(user.Id);
			Console.WriteLine("\nPosts:");
			foreach (var post in userPosts)
			{
				Console.WriteLine($"- {post.Description} (Posted on: {post.Date})");
			}

			if (user.Subscribers.Contains(_loggedInUser.Id))
			{
				Console.WriteLine("\nYou are already subscribed to this user.");
				Console.WriteLine("Press 'u' to unsubscribe or any other key to return to the search results...");
				var key = Console.ReadKey();
				if (key.KeyChar == 'u')
				{
					await UnsubscribeFromUser(user.Id);
				}
			}
			else
			{
				Console.WriteLine("\nYou are not subscribed to this user.");
				Console.WriteLine("Press 's' to subscribe or any other key to return to the search results...");
				var key = Console.ReadKey();
				if (key.KeyChar == 's')
				{
					await SubscribeToUser(user.Id);
				}
			}

			Console.WriteLine("\nPress any key to return to the search results...");
			Console.ReadKey();
		}

		static async Task SubscribeToUser(string userId)
		{
			try
			{
				await _userRepository.SubscribeToUserAsync(userId, _loggedInUser.Id);
				Console.WriteLine($"You have subscribed to the user with ID: {userId}.");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error subscribing to the user: {ex.Message}");
			}
		}

		static async Task UnsubscribeFromUser(string userId)
		{
			try
			{
				await _userRepository.UnsubscribeFromUserAsync(userId, _loggedInUser.Id);
				Console.WriteLine($"You have unsubscribed from the user with ID: {userId}.");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error unsubscribing from the user: {ex.Message}");
			}
		}

		static async Task<List<Post>> DisplaySubscribedUsersPosts()
		{
			var posts = new List<Post>();

			if (_loggedInUser != null)
			{
				var subscribedToUsers = await _userRepository.GetUsersSubscribedToAsync(_loggedInUser.Id);

				if (subscribedToUsers.Any())
				{
					foreach (var subscribedUser in subscribedToUsers)
					{
						var userPosts = await _postRepository.GetPostsByUserAsync(subscribedUser.Id);
						posts.AddRange(userPosts);
					}

					for (int i = 0; i < posts.Count; i++)
					{
						var post = posts[i];
						var postOwner = await _userRepository.GetUserByIdAsync(post.UserId);
						Console.WriteLine($"\n{i + 1}. {postOwner.FirstName} {postOwner.LastName}\n{post.Description}\n{post.Date}");
					}
				}
				else
				{
					Console.WriteLine("There are no users whose posts you are subscribed to.");
				}
			}
			else
			{
				Console.WriteLine("User is not logged in.");
			}

			return posts;
		}


		static async Task DisplayPostDetails(Post post)
		{
			Console.WriteLine($"\n{post.Description}\nPosted on: {post.Date}");

			Console.WriteLine("\n1 - Like\t\t2 - Comment");
			Console.WriteLine(" (B)ack to feed");

			while (true)
			{
				Console.Write("\nPlease select an option: ");
				string selectedOption = Console.ReadLine() ?? "";

				if (selectedOption.ToLower() == "b")
				{
					break;
				}
				else if (selectedOption == "1")
				{
					await LikePost(post);
					break;
				}
				else if (selectedOption == "2")
				{
					await CommentOnPost(post);
					break;
				}
				else
				{
					Console.WriteLine("Invalid option selected.");
				}
			}
		}

		static async Task LikePost(Post post)
		{
			if (!_loggedInUser.Id.Equals(post.UserId))
			{
				var like = new Like
				{
					Id = Guid.NewGuid().ToString(), 
					PostId = post.Id,
					UserId = _loggedInUser.Id
				};

				await _likeRepository.AddLikeAsync(like); 

				post.Likes.Add(like.UserId);
				post.CountLikes++;
				await _postRepository.UpdatePostAsync(post);

				Console.WriteLine("You liked this post!");
			}
			else
			{
				Console.WriteLine("You cannot like your own post.");
			}
		}


		static async Task CommentOnPost(Post post)
		{
			Console.WriteLine("Enter your comment:");
			string commentText = Console.ReadLine() ?? "";

			if (!string.IsNullOrWhiteSpace(commentText))
			{
				var comment = new Comment
				{
					Id = Guid.NewGuid().ToString(),
					UserId = _loggedInUser.Id,
					PostId = post.Id,
					Text = commentText,
					CreatedAt = DateTime.Now
				};

				post.Comments.Add(comment.Id);
				post.CountComments++;
				await _commentRepository.AddCommentAsync(comment);
				await _postRepository.UpdatePostAsync(post);
				Console.WriteLine("Your comment has been added!");
			}
		}

		static async Task CreateNewPost()
		{
			Console.WriteLine("Enter the description of your post:");
			string description = Console.ReadLine() ?? "";

			if (string.IsNullOrWhiteSpace(description))
			{
				Console.WriteLine("Description cannot be empty!");
				return;
			}

			var newPost = new Post
			{
				Id = Guid.NewGuid().ToString(),
				UserId = _loggedInUser.Id,
				Description = description,
				Date = DateTime.Now,
				Likes = new List<string>(),
				CountLikes = 0,
				Comments = new List<string>(),
				CountComments = 0
			};

			try
			{
				await _postRepository.AddPostAsync(newPost);
				Console.WriteLine("Post created successfully!");

				_loggedInUser.Posts.Add(newPost.Id);

				await _userRepository.UpdateUserAsync(_loggedInUser);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error creating a new post: " + ex.Message);
			}
		}

		static async Task SearchUsers()
		{
			Console.WriteLine("Enter the user's name to search:");
			string userName = Console.ReadLine() ?? "";

			var users = await _userRepository.SearchUsersByNameAsync(userName);

			if (users.Any())
			{
				Console.WriteLine("Search Results:");
				for (int i = 0; i < users.Count; i++)
				{
					Console.WriteLine($"{i + 1}. {users[i].FirstName} {users[i].LastName}");
				}

				Console.WriteLine("Select a user by number to view their profile or press any other key to return.");
				string selectedOption = Console.ReadLine() ?? "";

				if (int.TryParse(selectedOption, out int userIndex) && userIndex > 0 && userIndex <= users.Count)
				{
					await ViewUserProfile(users[userIndex - 1]);
				}
			}
			else
			{
				Console.WriteLine("No users found with that name.");
			}
		}
	}
}
