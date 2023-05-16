namespace DataAccess.Database
{
    using Bank;
    using Microsoft.Azure.Cosmos;
    using Logic.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the interface for interacting with the account data store.
    /// </summary>
    /// 

    public class UsersCosmosDbRepository : IUserRepository
    {
        private CosmosClient _cosmosClient;
        private Database _database;
        private Container _container;

        // The name of the database and container we will create
        private const string _databaseId = "BankUsers";
        private const string _containerId = "bankUsers";

        public UsersCosmosDbRepository()
        {
            string endpointUri = "https://bankingdb.documents.azure.com:443/";
            string primaryKey = "xfSTXbXQHEUe6D7rTtLdpZ8QXMz8qxuSsJfwiAtWapSPRM7olhT7aAvcgSkDVyAIsO1wVhe5Uxhf0GYVvbW58g==";

            _cosmosClient = new CosmosClient(endpointUri, primaryKey, new CosmosClientOptions() { ApplicationName = "BankProject" });
            _database = _cosmosClient.GetDatabase(_databaseId);
            _container = _database.GetContainer(_containerId);
        }

        /// <summary>
        /// Creates a new user data entity
        /// </summary>
        /// <param name="user">The user to be created</param>
        public async Task<Result<BankUser>> CreateUserAsync(BankUser user)
        {
            if (user == null)
            {
                return new Result<BankUser>
                {
                    Succeeded = false,
                    ResultType = ResultType.InvalidData
                };
            }

            // Set read only properties
            user.Id = Guid.NewGuid().ToString();

            // Create and item. Partition key value and id must be provided in order to create
            ItemResponse<BankUser> itemResponse = await _container.CreateItemAsync<BankUser>(user, new PartitionKey(user.UserName));

            // Check if the cosmos operation was successful or not. Create returns 204 No Content when successful
            if (itemResponse.StatusCode == System.Net.HttpStatusCode.Created)
            {
                return new Result<BankUser>()
                {
                    Succeeded = true,
                    ResultType = ResultType.Success,
                    Value = itemResponse.Resource
                };
            }

            // Check if the cosmos operation was successful or not.
            // Create returns 409 Conflict when the id already exists
            if (itemResponse.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                return new Result<BankUser>()
                {
                    Succeeded = false,
                    ResultType = ResultType.Duplicate,
                };
            }

            // The operation was not successful
            return new Result<BankUser>()
            {
                Succeeded = false,
                ResultType = ResultType.DataStoreError
            };
        }

        /// <summary>
        /// Updates the specified user data entity
        /// </summary>
        /// <param name="updatedUser">The user to be updated</param>
        public async Task<Result<BankUser>> UpdateUserAsync(BankUser updatedUser)
        {
            if (updatedUser == null)
            {
                return new Result<BankUser>
                {
                    Succeeded = false,
                    ResultType = ResultType.InvalidData
                };
            }

            // Verify that no other user has the updated username
            ItemResponse<BankUser> getUserResponse = await _container.ReadItemAsync<BankUser>(updatedUser.Id, new PartitionKey(updatedUser.UserName));
            if (getUserResponse.StatusCode == System.Net.HttpStatusCode.OK) // there is a user in the system with the user name
            {
                BankUser existingUser = getUserResponse.Resource;
                if (existingUser.Id != updatedUser.Id)
                {
                    // The user in the system with the given user name is not the same user we are updating -> username already exists
                    return new Result<BankUser>()
                    {
                        Succeeded = false,
                        ResultType = ResultType.Duplicate,
                        Message = $"A user with username {updatedUser.UserName} already exists"
                    };
                }
            }

            // Update an item. Partition key value and id must be provided in order to update
            ItemResponse<BankUser> itemResponse = _container.ReplaceItemAsync<BankUser>(updatedUser, id: updatedUser.Id, partitionKey: new PartitionKey(updatedUser.UserName)).Result;

            // Check if the cosmos operation was successful or not.
            // Create returns 409 Conflict when the id already exists
            if (itemResponse.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                return new Result<BankUser>()
                {
                    Succeeded = false,
                    ResultType = ResultType.Duplicate,
                };
            }

            // The query returned a list of accounts 
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
            if (oldUserName == null)
            {
                return new Result<BankUser>
                {
                    Succeeded = false,
                    ResultType = ResultType.InvalidData
                };
            }

            ItemResponse<BankUser> itemResponse = await _container.ReadItemAsync<BankUser>(oldUserName, new PartitionKey(updatedUser.UserName));

            // Check if the cosmos operation exist.
            // Create returns 404 Not Found oldusername may have been changed or deleted
            if (itemResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new Result<BankUser>()
                {
                    Succeeded = false,
                    ResultType = ResultType.NotFound,
                };
            }

            // The query returned a list of accounts 
            return new Result<BankUser>()
            {
                Succeeded = true,
                Value = updatedUser
            };
        }

        /// <summary>
        /// Deletes the specified user data entity
        /// </summary>
        /// <param name="deleteUser">The user to be deleted</param>
        public async Task<Result<BankUser>> DeleteUserAsync(BankUser deleteUser)
        {
            if (deleteUser == null)
            {
                return new Result<BankUser>
                {
                    Succeeded = false,
                    ResultType = ResultType.InvalidData
                };
            }

            // Delete an item. Partition key value and id must be provided in order to delete
            ItemResponse<BankUser> itemResponse = await _container.DeleteItemAsync<BankUser>(deleteUser.Id, new PartitionKey(deleteUser.UserName));

            // Check if the cosmos operation was successful or not. Delete returns 204 No Content when successful
            if (itemResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new Result<BankUser>()
                {
                    Succeeded = false,
                    ResultType = ResultType.NotFound
                };
            }

            if (itemResponse.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                return new Result<BankUser>()
                {
                    Succeeded = false,
                    ResultType = ResultType.DataStoreError
                };
            }

            // The query returned a list of accounts
            return new Result<BankUser>()
            {
                Succeeded = true,
                Value = deleteUser
            };
        }

        /// <summary>
        /// Gets a list of all users in the system
        /// </summary>
        /// <returns>A list of all <see cref="BankUser"/>s in the system</returns>
        public async Task<Result<List<BankUser>>> GetAllUsersAsync()
        {
            // Building the sql query
            string sqlQueryText = "SELECT * FROM users";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);

            // Querying the container
            FeedIterator<BankUser> queryResultSetIterator = _container.GetItemQueryIterator<BankUser>(queryDefinition);

            // Getting the results from the query
            List<BankUser> users = new List<BankUser>();
            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<BankUser> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (BankUser user in currentResultSet)
                {
                    users.Add(user);
                }
            }

            // Check if the operation returned any accounts
            if (!users.Any())
            {
                return new Result<List<BankUser>>()
                {
                    Succeeded = false,
                    ResultType = ResultType.NotFound
                };
            }

            // The query returned a list of accounts
            return new Result<List<BankUser>>()
            {
                Succeeded = true,
                Value = users
            };
        }

        /// <summary>
        /// Gets a list of all users that have the given user name
        /// </summary>
        /// <param name="userName">Unique user identifier</param>
        /// <returns>A list of all <see cref="BankUser"/>s that have the given user name</returns>
        public async Task<Result<List<BankUser>>> GetAllByUserNameAsync(string userName)
        {
            // Building the sql query
            string sqlQueryText = $"SELECT * FROM c WHERE c.UserName = \"{userName}\"";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);

            // Querying the container
            FeedIterator<BankUser> queryResultSetIterator = _container.GetItemQueryIterator<BankUser>(queryDefinition);

            // Getting the results from the query
            List<BankUser> users = new List<BankUser>();
            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<BankUser> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (BankUser user in currentResultSet)
                {
                    users.Add(user);
                }
            }

            // Check if the operation returned any accounts
            if (!users.Any())
            {
                return new Result<List<BankUser>>()
                {
                    Succeeded = false,
                    ResultType = ResultType.NotFound
                };
            }

            // The query returned a list of accounts
            return new Result<List<BankUser>>()
            {
                Succeeded = true,
                Value = users
            };
        }

        /// <summary>
        /// Gets a list of all users that have the given first name
        /// </summary>
        /// <param name="firstName">User's first name</param>
        /// <returns>A list of all <see cref="BankUser"/>s that have the given girst name</returns>
        public async Task<Result<List<BankUser>>> GetAllByFirstNameAsync(string firstName)
        {
            // Building the sql query
            string sqlQueryText = $"SELECT * FROM c WHERE c.FirstName = \"{firstName}\"";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);

            // Querying the container
            FeedIterator<BankUser> queryResultSetIterator = _container.GetItemQueryIterator<BankUser>(queryDefinition);

            // Getting the results from the query
            List<BankUser> users = new List<BankUser>();
            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<BankUser> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (BankUser user in currentResultSet)
                {
                    users.Add(user);
                }
            }

            // Check if the operation returned any accounts
            if (!users.Any())
            {
                return new Result<List<BankUser>>()
                {
                    Succeeded = false,
                    ResultType = ResultType.NotFound
                };
            }

            // The query returned a list of accounts
            return new Result<List<BankUser>>()
            {
                Succeeded = true,
                Value = users
            };
        }

        /// <summary>
        /// Gets a list of all users that have the given last name
        /// </summary>
        /// <param name="lastName">User's last name</param>
        /// <returns>A list of all <see cref="BankUser"/>s that have the given last name</returns>
        public async Task<Result<List<BankUser>>> GetAllByLastNameAsync(string lastName)
        {
            // Building the sql query
            string sqlQueryText = $"SELECT * FROM c WHERE c.LastName = \"{lastName}\"";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);

            // Querying the container
            FeedIterator<BankUser> queryResultSetIterator = _container.GetItemQueryIterator<BankUser>(queryDefinition);

            // Getting the results from the query
            List<BankUser> users = new List<BankUser>();
            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<BankUser> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (BankUser user in currentResultSet)
                {
                    users.Add(user);
                }
            }

            // Check if the operation returned any accounts
            if (!users.Any())
            {
                return new Result<List<BankUser>>()
                {
                    Succeeded = false,
                    ResultType = ResultType.NotFound
                };
            }

            // The query returned a list of accounts
            return new Result<List<BankUser>>()
            {
                Succeeded = true,
                Value = users
            };
        }

        /// <summary>
        /// Gets a list of all users that have the given first name and last name
        /// </summary>
        /// <param name="firstName">User's first name</param>
        /// <param name="lastName">User's last name</param>
        /// <returns>A list of all <see cref="BankUser"/>s that have the given first name and last name</returns>
        public async Task<Result<List<BankUser>>> GetAllByNameAsync(string firstName, string lastName)
        {
            // Building the sql query
            string sqlQueryText = $"SELECT * FROM c WHERE c.FirstName = \"{firstName}\" AND c.LastName = \"{lastName}\"";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);

            // Querying the container
            FeedIterator<BankUser> queryResultSetIterator = _container.GetItemQueryIterator<BankUser>(queryDefinition);

            // Getting the results from the query
            List<BankUser> users = new List<BankUser>();
            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<BankUser> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (BankUser user in currentResultSet)
                {
                    users.Add(user);
                }
            }

            // Check if the operation returned any accounts
            if (!users.Any())
            {
                return new Result<List<BankUser>>()
                {
                    Succeeded = false,
                    ResultType = ResultType.NotFound
                };
            }

            // The query returned a list of accounts
            return new Result<List<BankUser>>()
            {
                Succeeded = true,
                Value = users
            };
        }

        /// <summary>
        /// Gets the user with the given user name
        /// </summary>
        /// <param name="userName">Unique user identifier</param>
        /// <returns>The <see cref="BankUser"/> with the given user name, or null if no user exists with that user name</returns>
        public async Task<Result<BankUser>> GetByUserNameAsync(string userName)
        {
            // Building the sql query
            string sqlQueryText = $"SELECT * FROM c WHERE c.UserName = \"{userName}\"";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);

            // Querying the container
            FeedIterator<BankUser> queryResultSetIterator = _container.GetItemQueryIterator<BankUser>(queryDefinition);

            // Getting the results from the query
            FeedResponse<BankUser> currentResultSet = await queryResultSetIterator.ReadNextAsync();
            IEnumerable<BankUser> users = currentResultSet.Resource;

            // Check if the operation returned any accounts
            if (!users.Any())
            {
                return new Result<BankUser>()
                {
                    Succeeded = false,
                    ResultType = ResultType.NotFound
                };
            }

            // The query returned an account, our result should be the only account in the list
            BankUser user = users.FirstOrDefault();
            return new Result<BankUser>()
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
            // Building the sql query
            string sqlQueryText = $"SELECT * FROM c WHERE c.id = \"{id}\"";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);

            // Querying the container
            FeedIterator<BankUser> queryResultSetIterator = _container.GetItemQueryIterator<BankUser>(queryDefinition);

            // Getting the results from the query
            FeedResponse<BankUser> currentResultSet = await queryResultSetIterator.ReadNextAsync();
            IEnumerable<BankUser> users = currentResultSet.Resource;

            // Check if the operation returned any accounts
            if (!users.Any())
            {
                return new Result<BankUser>()
                {
                    Succeeded = false,
                    ResultType = ResultType.NotFound,
                    Message = $"Unable to get account with id ={id}. User not found."
                };
            }

            // The query returned an account, our result should be the only account in the list
            BankUser user = users.FirstOrDefault();
            return new Result<BankUser>()
            {
                Succeeded = true,
                Value = user
            };
        }

        public async Task<Result<BankUser>> DeleteUserAsync(string userName, string id)
        {
            if (id == null && userName == null)
            {
                return new Result<BankUser>
                {
                    Succeeded = false,
                    ResultType = ResultType.InvalidData,
                    Message = $"Invalid or missing parameters"
                };
            }

            // Get the account we want to delete
            Result<BankUser> getResult = await GetByIdAsync(id);

            if (getResult.Succeeded == false)
            {
                string message = $"Unable to delete account with id={id} and user name={userName}. Reason={getResult.Message}";
                return new Result<BankUser>
                {
                    Succeeded = false,
                    ResultType = getResult.ResultType,
                    Message = message
                };
            }

            // Delete an item. Partition key value and id must be provided in order to delete
            ItemResponse<BankUser> itemResponse = await _container.DeleteItemAsync<BankUser>(id, new PartitionKey(userName));

            // Check if the cosmos operation was successful or not. Delete returns 204 No Content when successful
            if (itemResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new Result<BankUser>()
                {
                    Succeeded = false,
                    ResultType = ResultType.NotFound,
                    Message = $"Unable to delete account with id={id} and user name={userName}. Account Not Found."
                };
            }

            if (itemResponse.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                return new Result<BankUser>()
                {
                    Succeeded = false,
                    ResultType = ResultType.DataStoreError,
                    Message = $"Unable to delete account with id={id} and user name={userName} due to data store errors."
                };
            }

            // The query returned a list of accounts
            return new Result<BankUser>()
            {
                Succeeded = true,
                Value = getResult.Value
            };
        }
    }
}
