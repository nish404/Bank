namespace Logic
{
    using Bank;
    using DataAccess.Database;
    using Models;
    using System;
    using System.Threading.Tasks;

    public class AccountLogic : IAccountLogic
    {
        IAccountRepository _accountRepository;
        IUserRepository _userRepository;

        public AccountLogic(IAccountRepository accountRepository, IUserRepository userRepository)
        {
            _accountRepository = accountRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Withdraws the given amount from the given account
        /// </summary>
        /// <param name="accountId">Identifier of the account we want to withdraw money from</param>
        /// <param name="pin">The pin of the account the amount should be withdrawn from</param>
        /// <param name="amount">The amount to be withdrawn from the account</param>
        /// <returns>The account after the withdraw</returns>
        public async Task<Result<Account>> WithdrawAmount(string accountId, string pin, decimal amount)
        {
            // Verify the input parameters
            if (string.IsNullOrWhiteSpace(accountId))
            {
                return new Result<Account>()
                {
                    Succeeded = false,
                    ResultType = ResultType.InvalidData,
                    Message = "Invalid or missing 'accountId' parameter"
                };
            }
            if (string.IsNullOrWhiteSpace(pin))
            {
                return new Result<Account>()
                {
                    Succeeded = false,
                    ResultType = ResultType.InvalidData,
                    Message = "Invalid or missing 'pin' parameter"
                };
            }
            if (amount <= 0)
            {
                return new Result<Account>()
                {
                    Succeeded = false,
                    ResultType = ResultType.InvalidData,
                    Message = "Invalid or missing 'amount' parameter. Amount must be greater than 0"
                };
            }

            // Get the account
            Result<Account> getAccountResult = await _accountRepository.GetByIdAsync(accountId);
            if (getAccountResult.Succeeded == false)
            {
                return new Result<Account>()
                {
                    Succeeded = false,
                    ResultType = ResultType.NotFound,
                    Message = $"Unable to withdraw {amount} from account {accountId} - account does not exist."
                };
            }

            Account account = getAccountResult.Value;

            // Verify that the pin is correct
            if (account.Pin != pin)
            {
                return new Result<Account>()
                {
                    Succeeded = false,
                    ResultType = ResultType.InvalidData,
                    Message = $"Unable to withdraw {amount} from account {accountId} - invalid pin"
                };
            }

            // Verify that we have enough balance on the account
            if (account.Amount < amount)
            {
                return new Result<Account>()
                {
                    Succeeded = false,
                    ResultType = ResultType.NotFound,
                    Message = $"Unable to withdraw {amount} from account {accountId} - not enough balance, account balance {account.Amount}."
                };
            }

            // Modify the balance of the account
            account.Amount = account.Amount - amount;

            // Update the account
            Result<Account> updateResult = await _accountRepository.UpdateAccountAsync(account);
            return updateResult;
        }

        public async Task<Result<Account>> DepositAmount(int accountNumber, decimal amount)
        {
            // Get the account
            Result<Account> getByAccountNumberResult = await _accountRepository.GetByAccountNumberAsync(accountNumber);
            if (getByAccountNumberResult.Succeeded == false)
            {
                return new Result<Account>()
                {
                    Succeeded = false,
                    ResultType = getByAccountNumberResult.ResultType,
                    Message = $"Unable to deposit {amount} to account {accountNumber}. Reason: {getByAccountNumberResult.Message}."
                };
            }
            Account account = getByAccountNumberResult.Value;
            return await DepositAmount(account.Id, amount, account.Number);
        }

        public async Task<Result<Account>> DepositAmount(string firstName, string lastName, int accountNumber, decimal amount)
        {
            Result<Account> getByAccountNumberResult = await _accountRepository.GetByAccountNumberAsync(accountNumber);
            if (getByAccountNumberResult.Succeeded == false)
            {
                // Return Result<decimal>
                return new Result<Account>()
                {
                    Succeeded = false,
                    ResultType = getByAccountNumberResult.ResultType,
                    Message = $"Unable to deposit {amount} to account {accountNumber}. Reason: {getByAccountNumberResult.Message}."
                };
            }
            Account account = getByAccountNumberResult.Value;

            Result<BankUser> user = await _userRepository.GetByUserNameAsync(account.UserName);
            if (firstName != user.Value.FirstName || lastName != user.Value.LastName)
            {
                return new Result<Account>()
                {
                    Succeeded = false,
                    ResultType = ResultType.NotFound,
                    Message = $"The given names do not match the name on the account."
                };
            }

            return await DepositAmount(accountNumber, amount);
        }

        /// <summary>
        /// Withdraws the given amount from the given account
        /// </summary>
        /// <param name="accountId">Identifier of the account we want to deposit money into</param>
        /// <param name="amount">The amount to be deposited into the account</param>
        /// <param name="firstName">FirstName identifier of the account we want to deposit money into</param>
        /// <param name="lastName">LastName identifier of the account we want to deposit money into</param>
        /// <returns>The account after the deposit</returns>
        public async Task<Result<Account>> DepositAmount(string accountId, decimal amount, int accountNumber)
        {
            // Verify the input parameters
            if (string.IsNullOrWhiteSpace(accountId))
            {
                return new Result<Account>()
                {
                    Succeeded = false,
                    ResultType = ResultType.InvalidData,
                    Message = "Invalid or missing 'accountId' parameter"
                };
            }

            if (amount <= 0)
            {
                return new Result<Account>()
                {
                    Succeeded = false,
                    ResultType = ResultType.InvalidData,
                    Message = "Invalid or missing 'amount' parameter. Amount must be greater than 0"
                };
            }

            // Get the account
            Result<Account> getAccountResult = await _accountRepository.GetByIdAsync(accountId);
            if (getAccountResult.Succeeded == false)
            {
                return new Result<Account>()
                {
                    Succeeded = false,
                    ResultType = ResultType.NotFound,
                    Message = $"Unable to deposit {amount} into account {accountId} - account does not exist."
                };
            }

            Account account = getAccountResult.Value;

            // Verify that the pin is correct
            if (account.Number != accountNumber)
            {
                return new Result<Account>()
                {
                    Succeeded = false,
                    ResultType = ResultType.InvalidData,
                    Message = $"Unable to deposit {amount} into account {accountId} - invalid account number"
                };
            }

            // Modify the balance of the account
            account.Amount = account.Amount + amount;

            // Update the account
            Result<Account> updateResult = await _accountRepository.UpdateAccountAsync(account);
            return updateResult;
        }

        public async Task<Result<decimal>> TransferAmount(int accountNumber, decimal amount)
        {
            // Get the account 
            Result<Account> getByAccountNumberResult = await _accountRepository.GetByAccountNumberAsync(accountNumber);
            if (getByAccountNumberResult.Succeeded == false)
            {
                return new Result<decimal>()
                {
                    Succeeded = false,
                    ResultType = getByAccountNumberResult.ResultType,
                    Message = $"Unable to deposit {amount} to account {accountNumber}. Reason: {getByAccountNumberResult.Message}."
                };
            }
            Account account = getByAccountNumberResult.Value;

            // Verify that we have enough balance on the account
            if (account.Amount < amount)
            {
                return new Result<decimal>()
                {
                    Succeeded = false,
                    ResultType = getByAccountNumberResult.ResultType,
                    Message = $"Unable to transfer {amount} from account {accountNumber} - not enough balance, account balance {account.Amount}."
                };
            }

            // Update the account
            await _accountRepository.UpdateAccountAsync(account);

            // Return the updated balance of the account
            return new Result<decimal>()
            {
                Succeeded = true,
                Value = account.Amount
            };
        }

        public async Task<Result<decimal>> TransferAmount(int sourceAccountNumber, string sourcePin, int destAccountNumber, string destFirstName, string destLastName, decimal amount)
        {
            // Validate the destination account information
            if (await Validate(destFirstName, destLastName, destAccountNumber) == false)
            {
                return new Result<decimal>()
                {
                    Succeeded = false,
                    ResultType = ResultType.InvalidData,
                    Message = $"Unable to transfer {amount} from {sourceAccountNumber} to {destAccountNumber}"
                };
            }

            // A transfer is a combination of a withdraw and a deposit
            Result<decimal> withDrawResult = await WithdrawAmount(sourceAccountNumber, sourcePin, amount);
            if (withDrawResult.Succeeded == false)
            {
                return new Result<decimal>()
                {
                    Succeeded = false,
                    ResultType = ResultType.InvalidData,
                    Message = $"Unable to transfer {amount} from {sourceAccountNumber} to {destAccountNumber}"
                };
            }

            Result<Account> depositResult = await DepositAmount(destFirstName, destLastName, destAccountNumber, amount);
            if (depositResult.Succeeded == false)
            {
                return new Result<decimal>()
                {
                    Succeeded = false,
                    ResultType = ResultType.InvalidData,
                    Message = $"Unable to transfer {amount} from {sourceAccountNumber} to {destAccountNumber}"
                };
            }
            return withDrawResult;
        }

        public async Task<Result<Account>> TransferAmount(string id, int sourceAccountNumber, string sourcePin, int destAccountNumber, decimal amount)
        {
            // Validate the destination account information
            if (await Validate(destAccountNumber) == false)
            {
                return new Result<Account>()
                {
                    Succeeded = false,
                    ResultType = ResultType.InvalidData,
                    Message = $"Unable to transfer {amount} from {sourceAccountNumber} to {destAccountNumber}"
                };
            }

            // A transfer is a combination of a withdraw and a deposit
            Result<Account> withDrawResult = await WithdrawAmount(id, sourcePin, amount);
            if (withDrawResult.Succeeded == false)
            {
                return new Result<Account>()
                {
                    Succeeded = false,
                    ResultType = ResultType.InvalidData,
                    Message = $"Unable to transfer {amount} from {sourceAccountNumber} to {destAccountNumber}"
                };
            }

            Result<Account> depositResult = await DepositAmount(destAccountNumber, amount);
            if (depositResult.Succeeded == false)
            {
                return new Result<Account>()
                {
                    Succeeded = false,
                    ResultType = ResultType.InvalidData,
                    Message = $"Unable to transfer {amount} from {sourceAccountNumber} to {destAccountNumber}"
                };
            }

            return withDrawResult;
        }

        public async Task<Result<decimal>> WithdrawAmount(int accountNumber, decimal amount)
        {
            // Get the account
            Result<Account> getByAccountNumberResult = await _accountRepository.GetByAccountNumberAsync(accountNumber);
            if (getByAccountNumberResult.Succeeded == false)
            {
                return new Result<decimal>()
                {
                    Succeeded = false,
                    ResultType = ResultType.NotFound,
                    Message = $"Unable to withdraw {amount} from account {accountNumber} - account does not exist."
                };
            }
            Account account = getByAccountNumberResult.Value;

            // Verify that we have enough balance on the account
            if (account.Amount < amount)
            {
                return new Result<decimal>()
                {
                    Succeeded = false,
                    ResultType = ResultType.NotFound,
                    Message = $"Unable to withdraw {amount} from account {accountNumber} - not enough balance, account balance {account.Amount}."
                };
            }

            // Modify the balance of the account
            account.Amount = account.Amount - amount;

            // Update the account
            await _accountRepository.UpdateAccountAsync(account);

            // Return the updated balance of the account
            return new Result<decimal>()
            {
                Succeeded = true,
                Value = account.Amount
            };
        }

        public async Task<Result<decimal>> WithdrawAmount(int accountNumber, string pin, decimal amount)
        {
            // Verify the account number 
            Result<Account> getByAccountNumberResult = await _accountRepository.GetByAccountNumberAsync(accountNumber);
            if (getByAccountNumberResult.Succeeded == false)
            {
                return new Result<decimal>()
                {
                    Succeeded = false,
                    ResultType = ResultType.NotFound,
                    Message = $"Unable to withdraw {amount} from account {accountNumber} - account does not exist."
                };
            }
            Account account = getByAccountNumberResult.Value;

            if (pin != account.Pin)
            {
                return new Result<decimal>()
                {
                    Succeeded = false,
                    ResultType = ResultType.InvalidData,
                    Message = "Incorrect Pin"
                };
            }
            return await WithdrawAmount(accountNumber, amount);
        }

        private async Task<bool> Validate(string firstName, string lastName, int accountNumber)
        {
            Result<Account> getByAccountNumberResult = await _accountRepository.GetByAccountNumberAsync(accountNumber);
            if (getByAccountNumberResult.Succeeded == false)
            {
                Console.WriteLine("This account does not exist.");
                return false;
            }

            Account account = getByAccountNumberResult.Value;

            Result<BankUser> user = await _userRepository.GetByUserNameAsync(account.UserName);
            if (firstName != user.Value.FirstName || lastName != user.Value.LastName)
            {
                Console.WriteLine("The names do not match the account number.");
                return false;
            }
            return true;
        }

        private async Task<bool> Validate(int accountNumber)
        {
            Result<Account> getByAccountNumberResult = await _accountRepository.GetByAccountNumberAsync(accountNumber);
            if (getByAccountNumberResult.Succeeded == false)
            {
                Console.WriteLine("This account does not exist.");
                return false;
            }

            return true;
        }
    }
}
