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
    /// Implements the <see cref="IAccountRepository"/> interface for
    /// interacting with the account file system data store
    /// </summary>
    public class AccountFileSystemRepository : IAccountRepository
    {

        /// <summary>
        /// Creates a new account entity
        /// </summary>
        /// <param name="account">The account to be created</param>
        public async Task<Result<Account>> CreateAccountAsync(Account account)
        {
            Result<List<Account>> getAllAccountsResult = await GetAllAccountsAsync();
            List<Account> accounts = getAllAccountsResult.Value;
            // Checking if there is any account that has the same account number
            // as the account we want to create
            if (accounts.Any(i => i.Number == account.Number))
            {
                return new Result<Account>()
                {
                    Succeeded = false,
                    Message = $"Unable to create account. Account number {account.Number} already exists",
                    ResultType = ResultType.Duplicate
                };
            }

            accounts.Add(account);
            using (StreamWriter writer = new StreamWriter("../../../accounts.json"))
            {
                string accountsJson = JsonSerializer.Serialize(accounts);
                writer.Write(accountsJson);
            }

            return new Result<Account>()
            {
                Succeeded = true,
                Value = account
            };
        }

        /// <summary>
        /// Deletes the specified account entity
        /// </summary>
        /// <param name="deletedAccount">The account to be deleted</param>
        public async Task<Result<Account>> DeleteAccountAsync(Account deletedAccount)
        {
            Result<List<Account>> getAllAccountsResult = await GetAllAccountsAsync();
            List<Account> allAccounts = getAllAccountsResult.Value;

            //allAccounts.Remove(deletedAccount);
            allAccounts = allAccounts.Where(i => i.Number != deletedAccount.Number).ToList();
            using (StreamWriter writer = new StreamWriter("../../../accounts.json"))
            {
                string accountsJson = JsonSerializer.Serialize(allAccounts);
                writer.Write(accountsJson);
            }

            Console.WriteLine($"Account {deletedAccount.Number} has been deleted");
            // Operation was successfull
            return new Result<Account>()
            {
                Succeeded = true,
                Value = deletedAccount
            };
        }

        public Task<Result<Account>> DeleteAccountAsync(string id, string userName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a list of all accounts in the system
        /// </summary>
        /// <returns>A list of all accounts in the system</returns>
        public async Task<Result<List<Account>>> GetAllAccountsAsync()
        {
            List<Account> accounts = new List<Account>();

            using (StreamReader reader = new StreamReader("../../../accounts.json"))
            {
                string accountsJson = await reader.ReadToEndAsync();
                accounts = JsonSerializer.Deserialize<List<Account>>(accountsJson);
            }
            Result<List<Account>> result = new Result<List<Account>>()
            {
                Succeeded = true,
                Value = accounts
            };
            return result;
        }

        /// <summary>
        /// Gets a list of all accounts that are belong to the user with a given user name
        /// </summary>
        /// <param name="userName">Unique identifier of the user we want to retrieve accounts gor</param>
        /// <returns>A list of all accounts that belong to the user</returns>
        public async Task<Result<List<Account>>> GetAllByUsernameAsync(string userName)
        {
            // Get all accounts in the system (json file)
            Result<List<Account>> getAllAccountsResult = await GetAllAccountsAsync();
            List<Account> accounts = getAllAccountsResult.Value;

            // Filter the list to only have accounts for the given user name
            List<Account> userAccounts = accounts.Where(i => i.UserName == userName).ToList();
            Result<List<Account>> result = new Result<List<Account>>()
            {
                Succeeded = true,
                Value = userAccounts
            };
            return result;
        }

        /// <summary>
        /// Gets an account with the given account number
        /// </summary>
        /// <param name="accountNumber">Unique account identifier</param>
        /// <returns>The <see cref="Account"/> with the given account number, or null if no account exists with that number</returns>
        public async Task<Result<Account>> GetByAccountNumberAsync(int accountNumber)
        {
            Result<List<Account>> getAllAccountsResult = await GetAllAccountsAsync();
            List<Account> accounts = getAllAccountsResult.Value;

            // account will either be the account with the matching account number, or null
            // if no account has that number.
            Account account = accounts.Where(i => i.Number == accountNumber).FirstOrDefault();

            if (account == null) // No account was found with the given account number
            {
                return new Result<Account>
                {
                    Succeeded = false,
                    ResultType = ResultType.NotFound,
                    Message = $"No account exists with account number {accountNumber}"
                };
            }

            // There is an account with the given account number
            return new Result<Account>
            {
                Succeeded = true,
                Value = account
            };
        }

        /// <summary>
        /// Gets an account with the given account id
        /// </summary>
        /// <param name="id">Unique account identifier</param>
        /// <returns>The <see cref="Account"/> with the given id, or null if no account exists with that id</returns>
        public async Task<Result<Account>> GetByIdAsync(string id)
        {
            Result<List<Account>> getAllAccountsResult = await GetAllAccountsAsync();
            List<Account> accounts = getAllAccountsResult.Value;

            Account account = accounts.Where(i => i.Id == id).FirstOrDefault();
            return new Result<Account>
            {
                Succeeded = true,
                Value = account
            };

        }

        /// <summary>
        /// Updates the specified account entity
        /// </summary>
        /// <param name="updatedAccount">The account to be updated</param>
        public async Task<Result<Account>> UpdateAccountAsync(Account updatedAccount)
        {
            // Check if the account we want to update exists
            Result<Account> getByAccountNumberResult = await GetByAccountNumberAsync(updatedAccount.Number);
            if (getByAccountNumberResult.Succeeded == false)
            {
                return getByAccountNumberResult;
            }

            // We have verified that the account exists - update the account
            Result<List<Account>> getAllAccountsResult = await GetAllAccountsAsync();
            List<Account> accounts = getAllAccountsResult.Value;
            foreach (Account account in accounts)
            {
                if (account.Number == updatedAccount.Number)
                {
                    account.Amount = updatedAccount.Amount;
                    account.UserName = updatedAccount.UserName;
                }
            }
            using (StreamWriter writer = new StreamWriter("../../../accounts.json"))
            {
                string accountsJson = JsonSerializer.Serialize(accounts);
                writer.Write(accountsJson);
            }

            // Operation was successfull
            return new Result<Account>()
            {
                Succeeded = true,
                Value = updatedAccount
            };
        }
    }
}
