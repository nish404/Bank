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

    public class AccountsCosmosDbRepository : IAccountRepository
    {
        private CosmosClient _cosmosClient;
        private Database _database;
        private Container _container;

        // The name of the database and container we will create
        private const string _databaseId = "Accounts";
        private const string _containerId = "accounts";

        public AccountsCosmosDbRepository()
        {
            string endpointUri = "https://bankingdb.documents.azure.com:443/";
            string primaryKey = "xfSTXbXQHEUe6D7rTtLdpZ8QXMz8qxuSsJfwiAtWapSPRM7olhT7aAvcgSkDVyAIsO1wVhe5Uxhf0GYVvbW58g==";

            _cosmosClient = new CosmosClient(endpointUri, primaryKey, new CosmosClientOptions() { ApplicationName = "BankProject" });
            _database = _cosmosClient.GetDatabase(_databaseId);
            _container = _database.GetContainer(_containerId);
        }

        /// <summary>
        /// Creates a new account data entity
        /// </summary>
        /// <param name="account">The account to be created</param>
        public async Task<Result<Account>> CreateAccountAsync(Account account)
        {
            if (account == null)
            {
                return new Result<Account>
                {
                    Succeeded = false,
                    ResultType = ResultType.InvalidData
                };
            }

            // Set read only properties
            account.Id = Guid.NewGuid().ToString();

            // Create and item. Partition key value and id must be provided in order to create
            ItemResponse<Account> itemResponse = await _container.CreateItemAsync<Account>(account, new PartitionKey(account.UserName));

            // Check if the cosmos operation was successful or not. Create returns 204 No Content when successful
            if (itemResponse.StatusCode == System.Net.HttpStatusCode.Created)
            {
                return new Result<Account>()
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
                return new Result<Account>()
                {
                    Succeeded = false,
                    ResultType = ResultType.Duplicate,
                };
            }

            // The operation was not successful
            return new Result<Account>()
            {
                Succeeded = false,
                ResultType = ResultType.DataStoreError
            };
        }

        /// <summary>
        /// Deletes the specified account data entity
        /// </summary>
        /// <param name="deletedAccount">The account to be deleted</param>
        public async Task<Result<Account>> DeleteAccountAsync(Account deletedAccount)
        {
            if (deletedAccount == null)
            {
                return new Result<Account>
                {
                    Succeeded = false,
                    ResultType = ResultType.InvalidData
                };
            }

            // Delete an item. Partition key value and id must be provided in order to delete
            ItemResponse<Account> itemResponse = await _container.DeleteItemAsync<Account>(deletedAccount.Id, new PartitionKey(deletedAccount.UserName));

            // Check if the cosmos operation was successful or not. Delete returns 204 No Content when successful
            if (itemResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new Result<Account>()
                {
                    Succeeded = false,
                    ResultType = ResultType.NotFound
                };
            }

            if (itemResponse.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                return new Result<Account>()
                {
                    Succeeded = false,
                    ResultType = ResultType.DataStoreError
                };
            }

            // The query returned a list of accounts
            return new Result<Account>()
            {
                Succeeded = true,
                Value = deletedAccount
            };
        }

        /// <summary>
        /// Updates the specified account data entity
        /// </summary>
        /// <param name="updatedAccount">The account to be updated</param>
        public async Task<Result<Account>> UpdateAccountAsync(Account updatedAccount)
        {
            if (updatedAccount == null)
            {
                return new Result<Account>
                {
                    Succeeded = false,
                    ResultType = ResultType.InvalidData
                };
            }

            // Update an item. Partition key value and id must be provided in order to update
            ItemResponse<Account> itemResponse = await _container.ReplaceItemAsync<Account>(updatedAccount, id: updatedAccount.Id, partitionKey: new PartitionKey(updatedAccount.UserName));

            // Check if the cosmos operation was successful or not.
            // Create returns 409 Conflict when the id already exists
            if (itemResponse.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                return new Result<Account>()
                {
                    Succeeded = false,
                    ResultType = ResultType.Duplicate,
                };
            }

            // The query returned a list of accounts 
            return new Result<Account>()
            {
                Succeeded = true,
                Value = updatedAccount
            };
        }

        /// <summary>
        /// Gets a list of all accounts in the system
        /// </summary>
        /// <returns>A list of all <see cref="Account"/>s in the system</returns>
        public async Task<Result<List<Account>>> GetAllAccountsAsync()
        {
            // Building the sql query
            string sqlQueryText = "SELECT * FROM accounts";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);

            // Querying the container
            FeedIterator<Account> queryResultSetIterator = _container.GetItemQueryIterator<Account>(queryDefinition);

            // Getting the results from the query
            List<Account> accounts = new List<Account>();
            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Account> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Account account in currentResultSet)
                {
                    accounts.Add(account);
                }
            }

            // Check if the operation returned any accounts
            if (!accounts.Any())
            {
                return new Result<List<Account>>()
                {
                    Succeeded = false,
                    ResultType = ResultType.NotFound
                };
            }

            // The query returned a list of accounts
            return new Result<List<Account>>()
            {
                Succeeded = true,
                Value = accounts
            };
        }

        /// <summary>
        /// Gets a list of all accounts that are belong to the user with a given user name
        /// </summary>
        /// <param name="userName">Unique identifier of the user we want to retrieve accounts for</param>
        /// <returns>A list of all <see cref="Account"/>s that belong to the user</returns>
        public async Task<Result<List<Account>>> GetAllByUsernameAsync(string userName)
        {
            // Building the sql query
            string sqlQueryText = $"SELECT * FROM c WHERE c.UserName = \"{userName}\"";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);

            // Querying the container
            FeedIterator<Account> queryResultSetIterator = _container.GetItemQueryIterator<Account>(queryDefinition);

            // Getting the results from the query
            List<Account> accounts = new List<Account>();
            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Account> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Account account in currentResultSet)
                {
                    accounts.Add(account);
                }
            }

            // Check if the operation returned any accounts
            if (!accounts.Any())
            {
                return new Result<List<Account>>()
                {
                    Succeeded = false,
                    ResultType = ResultType.NotFound
                };
            }

            // The query returned a list of accounts
            return new Result<List<Account>>()
            {
                Succeeded = true,
                Value = accounts
            };
        }

        /// <summary>
        /// Gets an account with the given account number
        /// </summary>
        /// <param name="accountNumber">Unique account identifier</param>
        /// <returns>The <see cref="Account"/> with the given account number, or null if no account exists with that number</returns>
        public async Task<Result<Account>> GetByAccountNumberAsync(int accountNumber)
        {
            // Building the sql query
            string sqlQueryText = $"SELECT * FROM c WHERE c.Number = {accountNumber}";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);

            // Querying the container
            FeedIterator<Account> queryResultSetIterator = _container.GetItemQueryIterator<Account>(queryDefinition);

            // Getting the results from the query
            FeedResponse<Account> currentResultSet = await queryResultSetIterator.ReadNextAsync();
            // query stops working here
            IEnumerable<Account> accounts = currentResultSet.Resource;

            // Check if the operation returned any accounts
            if (!accounts.Any())
            {
                return new Result<Account>()
                {
                    Succeeded = false,
                    ResultType = ResultType.NotFound
                };
            }

            // The query returned an account, our result should be the only account in the list
            Account account = accounts.FirstOrDefault();
            return new Result<Account>()
            {
                Succeeded = true,
                Value = account
            };
        }

        /// <summary>
        /// Gets the account with the given account id
        /// </summary>
        /// <param name="id">Unique account identifier</param>
        /// <returns>The <see cref="Account"/> with the given id, or null if no account exists with that id</returns>
        public async Task<Result<Account>> GetByIdAsync(string id)
        {
            // Building the sql query
            string sqlQueryText = $"SELECT * FROM c WHERE c.id = \"{id}\"";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);

            // Querying the container
            FeedIterator<Account> queryResultSetIterator = _container.GetItemQueryIterator<Account>(queryDefinition);

            // Getting the results from the query
            FeedResponse<Account> currentResultSet = await queryResultSetIterator.ReadNextAsync();
            IEnumerable<Account> accounts = currentResultSet.Resource;

            // Check if the operation returned any accounts
            if (!accounts.Any())
            {
                return new Result<Account>()
                {
                    Succeeded = false,
                    ResultType = ResultType.NotFound,
                    Message = $"Unable to get account with id ={id}. Account not found."
                };
            }

            // The query returned an account, our result should be the only account in the list
            Account account = accounts.FirstOrDefault();
            return new Result<Account>()
            {
                Succeeded = true,
                Value = account
            };
        }

        public async Task<Result<Account>> DeleteAccountAsync(string id, string userName)
        {
            if (id == null && userName == null)
            {
                return new Result<Account>
                {
                    Succeeded = false,
                    ResultType = ResultType.InvalidData,
                    Message = $"Invaid or missing parameters"
                };
            }

            // Get the account we want to delete
            Result<Account> getResult = await GetByIdAsync(id);

            if (getResult.Succeeded == false)
            {
                string message = $"Unable to delete account with id={id} and user name={userName}. Reason={getResult.Message}";
                return new Result<Account>
                {
                    Succeeded = false,
                    ResultType = getResult.ResultType,
                    Message = message
                };
            }

            // Delete an item. Partition key value and id must be provided in order to delete
            ItemResponse<Account> itemResponse = await _container.DeleteItemAsync<Account>(id, new PartitionKey(userName));

            // Check if the cosmos operation was successful or not. Delete returns 204 No Content when successful
            if (itemResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new Result<Account>()
                {
                    Succeeded = false,
                    ResultType = ResultType.NotFound,
                    Message = $"Unable to delete account with id={id} and user name={userName}. Account not found."
                };
            }

            if (itemResponse.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                return new Result<Account>()
                {
                    Succeeded = false,
                    ResultType = ResultType.DataStoreError,
                    Message = $"Unable to delete account with id={id} and user name={userName} due to data store errors."
                };
            }

            // Return the account that got deleted
            return new Result<Account>()
            {
                Succeeded = true,
                Value = getResult.Value
            };
        }
    }
}
