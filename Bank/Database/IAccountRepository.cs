namespace DataAccess.Database
{
    using System.Collections.Generic;
    using Bank;
    using System.Threading.Tasks;
    using Logic.Models;

    public interface IAccountRepository
    {
        /// <summary>
        /// Gets a list of all accounts in the system
        /// </summary>
        /// <returns>A list of all <see cref="Account"/>s in the system</returns>
        Task<Result<List<Account>>> GetAllAccountsAsync();

        /// <summary>
        /// Gets a list of all accounts that are belong to the user with a given user name
        /// </summary>
        /// <param name="userName">Unique identifier of the user we want to retrieve accounts for</param>
        /// <returns>A list of all <see cref="Account"/>s that belong to the user</returns>
        Task<Result<List<Account>>> GetAllByUsernameAsync(string userName);

        // function to get a single account
        /// <summary>
        /// Gets an account with the given account number
        /// </summary>
        /// <param name="accountNumber">Unique account identifier</param>
        /// <returns>The <see cref="Account"/> with the given account number, or null if no account exists with that number</returns>
        Task<Result<Account>> GetByAccountNumberAsync(int accountNumber);

        /// <summary>
        /// Gets the account with the given account id
        /// </summary>
        /// <param name="id">Unique account identifier</param>
        /// <returns>The <see cref="Account"/> with the given id, or null if no account exists with that id</returns>
        Task<Result<Account>> GetByIdAsync(string id);

        /// <summary>
        /// Creates a new account data entity
        /// </summary>
        /// <param name="account">The account to be created</param>
        Task<Result<Account>> CreateAccountAsync(Account account);

        /// <summary>
        /// Updates the specified account data entity
        /// </summary>
        /// <param name="updatedAccount">The account to be updated</param>
        Task<Result<Account>> UpdateAccountAsync(Account updatedAccount);

        /// <summary>
        /// Deletes the specified account data entity
        /// </summary>
        /// <param name="deletedAccount">The account to be deleted</param>
        Task<Result<Account>> DeleteAccountAsync(Account deletedAccount);

        /// Deletes the specified account data entity
        /// </summary>
        /// <param name="id">Id of the account to be deleted</param>
        /// <param name="userName">Username of the user who is associated with the account to be deleted</param>
        Task<Result<Account>> DeleteAccountAsync(string id, string userName);
    }
}
