namespace DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using Bank;
    using System.Threading.Tasks;
    using DataAccess.Database;
    using Logic.Models;

    /// <summary>
    /// Implements the <see cref="IUserRepository"/> interface for
    /// interacting with the user file system data store
    /// </summary>
    public class UserFileSystemRepository : IUserRepository
    {
        /// <summary>
        /// Creates a new user data entity
        /// </summary>
        /// <param name="user">The user to be created</param>
        public async Task<Result<BankUser>> CreateUserAsync(BankUser user)
        {
            Result<List<BankUser>> getAllUsersResult = await GetAllUsersAsync();
            List<BankUser> users = getAllUsersResult.Value;
            // Checking if there is any account that has the same account number
            // as the account we want to create
            if (users.Any(i => i.UserName == user.UserName))
            {
                return new Result<BankUser>()
                {
                    Succeeded = false,
                    Message = $"Unable to create username. Username {user.UserName} already exists",
                    ResultType = ResultType.Duplicate
                };
            }

            users.Add(user);
            using (StreamWriter writer = new StreamWriter("../../../users.json"))
            {
                string usersJson = JsonSerializer.Serialize(users);
                writer.Write(usersJson);
            }

            return new Result<BankUser>()
            {
                Succeeded = true,
                Value = user
            };
        }

        /// <summary>
        /// Updates the specified user data entity
        /// </summary>
        /// <param name="updatedUser">The user to be updated</param>
        public async Task<Result<BankUser>> UpdateUserAsync(BankUser updatedUser)
        {
            // Check if the account we want to update exists
            Result<BankUser> getAllUserNamesResult = await GetByIdAsync(updatedUser.Id);
            if (getAllUserNamesResult.Succeeded == false)
            {
                return getAllUserNamesResult;
            }

            Result<BankUser> existingUser = await GetByIdAsync(updatedUser.Id);
            if (existingUser.Succeeded == false)
            {
                return new Result<BankUser>()
                {
                    Succeeded = false,
                    Message = $"Unable to update user. No user found with ID {updatedUser.Id} exists",
                    ResultType = ResultType.NotFound
                };
            }

            Result<BankUser> getByUserNameResult = await GetByUserNameAsync(updatedUser.UserName);
            if (getByUserNameResult.Succeeded == true /*There is a user with the matching user name*/ &&
                // getByUserNameResult.Value is the existing user with the given user name
                getByUserNameResult.Value.Id != existingUser.Value.Id /* The user with the mathing user name has a different id*/)
            {
                return new Result<BankUser>()
                {
                    Succeeded = false,
                    Message = $"Unable to upate user. Duplicate user name",
                    ResultType = ResultType.Duplicate
                };
            }

            // We have verified that the user exists - update the user
            Result<List<BankUser>> getAllUsersResult = await GetAllUsersAsync();
            List<BankUser> users = getAllUsersResult.Value;
            foreach (BankUser user in users)
            {
                if (user.Id == updatedUser.Id)
                {
                    user.FirstName = updatedUser.FirstName;
                    user.LastName = updatedUser.LastName;
                    user.UserName = updatedUser.UserName;
                }
            }
            using (StreamWriter writer = new StreamWriter("../../../users.json"))
            {
                string usersJson = JsonSerializer.Serialize(users);
                writer.Write(usersJson);
            }

            // Operation was successful
            return new Result<BankUser>()
            {
                Succeeded = true,
                Value = updatedUser
            };
        }

        /// <summary>
        /// Updates the specified user data entity
        /// </summary>
        /// <param name="oldUserName">The user's user name before the update</param>
        /// <param name="updatedUser">The user to be updated</param>
        public async Task<Result<BankUser>> UpdateUserAsync(string oldUserName, BankUser updatedUser)
        {
            // Check if the account we want to update exists
            Result<BankUser> existingUserName = await GetByUserNameAsync(oldUserName);
            if (existingUserName.Succeeded == false)
            {
                return new Result<BankUser>()
                {
                    Succeeded = false,
                    Message = $"Unable to update user. No user with that username {oldUserName} exists",
                    ResultType = ResultType.NotFound
                };
            }

            // We have verified that the user exists - update the user
            Result<List<BankUser>> getAllUsersResult = await GetAllUsersAsync();
            List<BankUser> users = getAllUsersResult.Value;
            foreach (BankUser user in users)
            {
                if (user.UserName == oldUserName)
                {
                    user.UserName = updatedUser.UserName;
                    user.FirstName = updatedUser.FirstName;
                    user.LastName = updatedUser.LastName;
                }
            }
            using (StreamWriter writer = new StreamWriter("../../../users.json"))
            {
                string usersJson = JsonSerializer.Serialize(users);
                writer.Write(usersJson);

            }

            // Operation was successful
            return new Result<BankUser>()
            {
                Succeeded = true,
                Value = updatedUser
            };
        }

        /// <summary>
        /// Deletes the specified user data entity
        /// </summary>
        /// <param name="deletedUser">The user to be deleted</param>
        public async Task<Result<BankUser>> DeleteUserAsync(BankUser deletedUser)
        {
            Result<List<BankUser>> getAllUsersResult = await GetAllUsersAsync();
            List<BankUser> allUsers = getAllUsersResult.Value;

            //allUsers.Remove(deletedUser);
            allUsers = allUsers.Where(i => i.UserName != deletedUser.UserName).ToList();
            using (StreamWriter writer = new StreamWriter("../../../users.json"))
            {
                string userJson = JsonSerializer.Serialize(allUsers);
                writer.Write(userJson);
            }

            Console.WriteLine($"User {deletedUser.UserName} has been deleted");
            // Operation was successful
            return new Result<BankUser>()
            {
                Succeeded = true,
                Value = deletedUser
            };
        }

        /// <summary>
        /// Gets a list of all users in the system
        /// </summary>
        /// <returns>A list of all <see cref="BankUser"/>s in the system</returns>
        public async Task<Result<List<BankUser>>> GetAllUsersAsync()
        {
            List<BankUser> users = new List<BankUser>();
            using (StreamReader reader = new StreamReader("../../../users.json"))
            {
                string usersJson = await reader.ReadToEndAsync();
                users = JsonSerializer.Deserialize<List<BankUser>>(usersJson);
            }
            Result<List<BankUser>> result = new Result<List<BankUser>>()
            {
                Succeeded = true,
                Value = users
            };
            return result;
        }

        /// <summary>
        /// Gets a list of all users that have the given user name
        /// </summary>
        /// <param name="userName">Unique user identifier</param>
        /// <returns>A list of all <see cref="BankUser"/>s that have the given user name</returns>
        public async Task<Result<List<BankUser>>> GetAllByUserNameAsync(string userName)
        {
            // Get all accounts in the system (json file)
            Result<List<BankUser>> getAllUsersResult = await GetAllUsersAsync();
            List<BankUser> users = getAllUsersResult.Value;

            // Filter the list to only have accounts for the given user name
            List<BankUser> allUsers = users.Where(i => i.UserName == userName).ToList();
            Result<List<BankUser>> result = new Result<List<BankUser>>()
            {
                Succeeded = true,
                Value = allUsers
            };
            return result;
        }

        /// <summary>
        /// Gets a list of all users that have the given first name
        /// </summary>
        /// <param name="firstName">User's first name</param>
        /// <returns>A list of all <see cref="BankUser"/>s that have the given girst name</returns>
        public async Task<Result<List<BankUser>>> GetAllByFirstNameAsync(string firstName)
        {
            // Get all accounts in the system (json file)
            Result<List<BankUser>> getAllUsersResult = await GetAllUsersAsync();
            List<BankUser> users = getAllUsersResult.Value;

            // Filter the list to only have accounts for the given first name
            List<BankUser> allFirst = users.Where(i => i.FirstName == firstName).ToList();
            Result<List<BankUser>> result = new Result<List<BankUser>>()
            {
                Succeeded = true,
                Value = allFirst
            };
            return result;
        }

        /// <summary>
        /// Gets a list of all users that have the given last name
        /// </summary>
        /// <param name="lastName">User's last name</param>
        /// <returns>A list of all <see cref="BankUser"/>s that have the given last name</returns>
        public async Task<Result<List<BankUser>>> GetAllByLastNameAsync(string lastName)
        {
            // Get all accounts in the system (json file)
            Result<List<BankUser>> getAllUsersResult = await GetAllUsersAsync();
            List<BankUser> users = getAllUsersResult.Value;

            // Filter the list to only have accounts for the given last name
            List<BankUser> allLast = users.Where(i => i.LastName == lastName).ToList();
            Result<List<BankUser>> result = new Result<List<BankUser>>()
            {
                Succeeded = true,
                Value = allLast
            };
            return result;
        }

        /// <summary>
        /// Gets a list of all users that have the given first name and last name
        /// </summary>
        /// <param name="firstName">User's first name</param>
        /// <param name="lastName">User's last name</param>
        /// <returns>A list of all <see cref="BankUser"/>s that have the given first name and last name</returns>
        public async Task<Result<List<BankUser>>> GetAllByNameAsync(string firstName, string lastName)
        {
            // Get all accounts in the system (json file)
            Result<List<BankUser>> getAllUsersResult = await GetAllUsersAsync();
            List<BankUser> users = getAllUsersResult.Value;

            // Filter the list to only have accounts for the given all names
            List<BankUser> allUsers = users.Where(i => i.FirstName == firstName && i.LastName == lastName).ToList();
            Result<List<BankUser>> result = new Result<List<BankUser>>()
            {
                Succeeded = true,
                Value = allUsers
            };
            return result;
        }

        /// <summary>
        /// Gets the user with the given user name
        /// </summary>
        /// <param name="userName">Unique user identifier</param>
        /// <returns>The <see cref="BankUser"/> with the given user name, or null if no user exists with that user name</returns>
        public async Task<Result<BankUser>> GetByUserNameAsync(string userName)
        {
            // Get all accounts in the system (json file)
            Result<List<BankUser>> getAllUsersResult = await GetAllUsersAsync();
            List<BankUser> users = getAllUsersResult.Value;

            // user will either be the user with the matching username, or null if there is no account with that username
            BankUser user = users.Where(i => i.UserName == userName).FirstOrDefault();
            return new Result<BankUser>
            {
                Succeeded = true,
                Value = user
            };
        }

        /// <summary>
        /// Gets the user with the given user id
        /// </summary>
        /// <param name="id">Unique user identifier</param>
        /// <returns>The <see cref="BankUser"/> with the given id, or null if no user exists with that id</returns>
        public async Task<Result<BankUser>> GetByIdAsync(string id)
        {
            Result<List<BankUser>> getAllUsersResult = await GetAllUsersAsync();
            List<BankUser> users = getAllUsersResult.Value;

            BankUser user = users.Where(i => i.Id == id).FirstOrDefault();
            return new Result<BankUser>
            {
                Succeeded = true,
                Value = user
            };
        }

        public Task<Result<BankUser>> DeleteUserAsync(string userName, string id)
        {
            throw new NotImplementedException();
        }
    }
}