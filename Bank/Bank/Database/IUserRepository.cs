namespace DataAccess.Database
{
    using System.Collections.Generic;
    using Bank;
    using System.Threading.Tasks;
    using Logic.Models;

    /// <summary>
    /// defines the interface for interacting with the account data store
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Creates a new user data entity
        /// </summary>
        /// <param name="user">The user to be created</param>
        Task<Result<BankUser>> CreateUserAsync(BankUser user);

        /// <summary>
        /// Updates the specified user data entity
        /// </summary>
        /// <param name="updatedUser">The user to be updated</param>
        Task<Result<BankUser>> UpdateUserAsync(BankUser updatedUser);

        /// <summary>
        /// Updates the specified user data entity
        /// </summary>
        /// <param name="oldUserName">The user's user name before the update</param>
        /// <param name="updatedUser">The user to be updated</param>
        Task<Result<BankUser>> UpdateUserAsync(string oldUserName, BankUser updatedUser);

        /// <summary>
        /// Deletes the specified user data entity
        /// </summary>
        /// <param name="deleteUser">The user to be deleted</param>
        Task<Result<BankUser>> DeleteUserAsync(BankUser deleteUser);

        /// <summary>
        /// Gets a list of all users in the system
        /// </summary>
        /// <returns>A list of all <see cref="BankUser"/>s in the system</returns>
        Task<Result<List<BankUser>>> GetAllUsersAsync();

        /// <summary>
        /// Gets a list of all users that have the given user name
        /// </summary>
        /// <param name="userName">Unique user identifier</param>
        /// <returns>A list of all <see cref="BankUser"/>s that have the given user name</returns>
        Task<Result<List<BankUser>>> GetAllByUserNameAsync(string userName);

        /// <summary>
        /// Gets a list of all users that have the given first name
        /// </summary>
        /// <param name="firstName">User's first name</param>
        /// <returns>A list of all <see cref="BankUser"/>s that have the given first name</returns>
        Task<Result<List<BankUser>>> GetAllByFirstNameAsync(string firstName);

        /// <summary>
        /// Gets a list of all users that have the given last name
        /// </summary>
        /// <param name="lastName">User's last name</param>
        /// <returns>A list of all <see cref="BankUser"/>s that have the given last name</returns>
        Task<Result<List<BankUser>>> GetAllByLastNameAsync(string lastName);

        /// <summary>
        /// Gets a list of all users that have the given first name and last name
        /// </summary>
        /// <param name="firstName">User's first name</param>
        /// <param name="lastName">User's last name</param>
        /// <returns>A list of all <see cref="BankUser"/>s that have the given first name and last name</returns>
        Task<Result<List<BankUser>>> GetAllByNameAsync(string firstName, string lastName);

        /// <summary>
        /// Gets the user with the given user name
        /// </summary>
        /// <param name="userName">Unique user identifier</param>
        /// <returns>The <see cref="BankUser"/> with the given user name, or null if no user exists with that user name</returns>
        Task<Result<BankUser>> GetByUserNameAsync(string userName);

        /// <summary>
        /// Gets the user with the given user id
        /// </summary>
        /// <param name="id">Unique user identifier</param>
        /// <returns>The <see cref="BankUser"/> with the given id, or null if no user exists with that id</returns>
        Task<Result<BankUser>> GetByIdAsync(string id);

        /// Deletes the specified account data entity
        /// </summary>
        /// <param name="id">Id of the user to be deleted</param>
        /// <param name="userName">Username of the user who is associated with the account to be deleted</param>
        Task<Result<BankUser>> DeleteUserAsync(string userName, string id);
    }
}
