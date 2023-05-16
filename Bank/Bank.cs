namespace Banking
{
    using System;
    using System.Collections.Generic;
    using DataAccess.Database;
    using Logic;
    using global::DataAccess.Database;
    using System.Security.Cryptography;
    using Microsoft.AspNetCore.Cryptography.KeyDerivation;
    using Microsoft.Extensions.DependencyInjection;
    using global::Logic.Models;
    using Bank;

    class Banking
    {
        private static IUserRepository _userRepository;
        private static IAccountRepository _accountRepository;
        private static IAccountLogic _accountLogic;

        static void Main(string[] args)
        {
            IServiceProvider serviceProvider = ConfigureSerivces();

            _userRepository = serviceProvider.GetRequiredService<IUserRepository>();
            _accountRepository = serviceProvider.GetRequiredService<IAccountRepository>();
            _accountLogic = serviceProvider.GetRequiredService<IAccountLogic>();

            signIn();
        }

        static private void PrintUsers(List<BankUser> users)
        {
            Console.WriteLine($"Number of users: {users.Count}");
            foreach (BankUser user in users)
            {
                Console.WriteLine($"Id: {user.Id} UserName: {user.UserName}, FirstName: {user.FirstName}, LastName: {user.LastName}");
            }
            Console.WriteLine("");
        }

        static private void PrintAccounts(List<Account> accounts)
        {
            Console.WriteLine($"Number of accounts: {accounts.Count}");
            foreach (Account account in accounts)
            {
                Console.WriteLine($"UserName: {account.UserName}, Number: {account.Number}, Amount: {account.Amount}");
            }
            Console.WriteLine("");
        }

        static void signIn()
        {
            string userName;
            string password = String.Empty;

            // Ask for the username and verify
            Console.WriteLine("Welcome to CyberBank. Please sign in.");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("Enter your username: ");
            userName = Console.ReadLine();

            Result<BankUser> user = _userRepository.GetByUserNameAsync(userName).Result;

            if (user.Succeeded == false)
            {
                Console.WriteLine("This user does not exist in our system.");
                return;
            }

            Console.WriteLine("\nEnter your password: ");
            ConsoleKey key;

            // Ask for the password and verify
            while (password != user.Value.Password)
            {

                do
                {
                    ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                    key = keyInfo.Key;

                    if (key == ConsoleKey.Backspace && password.Length > 0)
                    {
                        Console.Write("\b \b");
                        password = password[0..^1];
                    }
                    else if (!char.IsControl(keyInfo.KeyChar))
                    {
                        Console.Write("*");
                        password += keyInfo.KeyChar;
                    }
                } while (key != ConsoleKey.Enter);

                // generate a 128-bit salt using a cryptographically strong random sequence of nonzero values
                byte[] salt = new byte[128 / 8];
                using (var rngCsp = new RNGCryptoServiceProvider())
                {
                    rngCsp.GetNonZeroBytes(salt);
                }
                Console.WriteLine($"\nSalt: {Convert.ToBase64String(salt)}");

                // derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));
                Console.WriteLine($"Hashed: {hashed}");

                if (password == user.Value.Password)
                {
                    Console.WriteLine("\nUser Authenticated.\nWhat would you like to do today?\n");
                    mainMenu(userName);
                }
                else
                {
                    Console.WriteLine("\nIncorrect Password. Please try again: ");
                    password = String.Empty;
                }
            }
        }

        static void mainMenu(string userName)
        {
            while (true)
            {


                Console.WriteLine("------------------------------");
                Console.WriteLine("Main Menu");
                Console.WriteLine("------------------------------");
                Console.WriteLine("1. Check Balance");
                Console.WriteLine("------------------------------");
                Console.WriteLine("2. Deposit");
                Console.WriteLine("------------------------------");
                Console.WriteLine("3. Withdraw");
                Console.WriteLine("------------------------------");
                Console.WriteLine("4. Transfer");
                Console.WriteLine("------------------------------");
                Console.WriteLine("5. View All Accounts");
                Console.WriteLine("------------------------------");
                Console.WriteLine("6. Terminate Transaction");
                Console.WriteLine("------------------------------");
                Console.WriteLine("7. Log Out");
                Console.WriteLine("------------------------------\n");

                int options = int.Parse(Console.ReadLine());

                switch (options)
                {
                    case 1:
                        balance();
                        break;
                    case 2:
                        deposit();
                        break;
                    case 3:
                        withdraw();
                        break;
                    case 4:
                        transfer();
                        break;
                    case 5:
                        viewAccounts(userName);
                        break;
                    case 6:
                        exit();
                        return;
                    case 7:
                        signOut();
                        return;
                    default:
                        Console.WriteLine("This is not a valid option.");
                        return;
                }
            }
        }

        static void balance()
        {
            int accountNumber;

            // Ask for the account number 
            Console.WriteLine("Please enter your account number: ");
            accountNumber = int.Parse(Console.ReadLine());

            // Verify the account number
            Result<Account> account = _accountRepository.GetByAccountNumberAsync(accountNumber).Result;

            if (account == null)
            {
                Console.WriteLine("This account does not exist in our system.");
                return;
            }

            Console.WriteLine($"\nYour current balance is: {account.Value.Amount}");
            return;
        }

        static void deposit()
        {

            int accountNumber;
            string firstName;
            string lastName;
            decimal depositAmount;

            // Ask for the account number 
            Console.WriteLine("Enter your account number: ");
            accountNumber = int.Parse(Console.ReadLine());

            // Ask for the first name 
            Console.WriteLine("Enter your first name: ");
            firstName = Console.ReadLine();

            // Ask for the last name 
            Console.WriteLine("Enter your last name: ");
            lastName = Console.ReadLine();

            // Ask for the amount 
            Console.WriteLine("Enter the amount you want to deposit: ");
            depositAmount = decimal.Parse(Console.ReadLine());

            // Call deposit function
            _accountLogic.DepositAmount(firstName, lastName, accountNumber, depositAmount).Wait();
            return;
        }

        static void withdraw()
        {
            int accountNumber;
            decimal withdrawAmount;
            string pin = "";

            // Ask for the account number 
            Console.WriteLine("Enter your account number: ");
            accountNumber = int.Parse(Console.ReadLine());

            // Ask for pin
            Console.WriteLine("Enter your pin: ");
            pin = Console.ReadLine();

            // Ask for withdraw amount
            Console.WriteLine("Select a withdraw option: ");
            Console.WriteLine("1. $20");
            Console.WriteLine("2. $40");
            Console.WriteLine("3. $60");
            Console.WriteLine("4. $80");
            Console.WriteLine("5. $100");
            Console.WriteLine("6. Enter an amount.");

            int withdrawOption = int.Parse(Console.ReadLine());

            switch (withdrawOption)
            {
                case 1:
                    withdrawAmount = 20;
                    exit();
                    break;
                case 2:
                    withdrawAmount = 40;
                    exit();
                    break;
                case 3:
                    withdrawAmount = 60;
                    exit();
                    break;
                case 4:
                    withdrawAmount = 80;
                    exit();
                    break;
                case 5:
                    withdrawAmount = 100;
                    exit();
                    break;
                case 6:
                    Console.WriteLine("Enter the amount you want to withdraw: ");
                    withdrawAmount = decimal.Parse(Console.ReadLine());
                    break;
                default:
                    Console.WriteLine("This is an invalid option.");
                    return;
            }

            _accountLogic.WithdrawAmount(accountNumber, pin, withdrawAmount);
            return;
        }

        static void transfer()
        {
            int sourceAccount;
            int destinationAccount;
            decimal transferAmount;
            string pin = "";
            string firstName;
            string lastName;

            Console.WriteLine("Enter the account number you want to transfer from: ");
            sourceAccount = int.Parse(Console.ReadLine());

            Console.WriteLine("\nEnter your pin: ");
            pin = Console.ReadLine();

            Console.WriteLine("\nEnter the amount you are transferring: ");
            transferAmount = decimal.Parse(Console.ReadLine());

            Console.WriteLine("\nEnter the number of the account you want to transfer to: ");
            destinationAccount = int.Parse(Console.ReadLine());

            Console.WriteLine("\nEnter the first name of the account holder: ");
            firstName = Console.ReadLine();

            Console.WriteLine("\nEnter the last name of the account holder: ");
            lastName = Console.ReadLine();

            _accountLogic.TransferAmount(sourceAccount, pin, destinationAccount, firstName, lastName, transferAmount).Wait();
            return;
        }

        static void viewAccounts(string userName)
        {
            Result<List<Account>> accounts = _accountRepository.GetAllByUsernameAsync(userName).Result;

            PrintAccounts(accounts.Value);
        }

        static void signOut()
        {
            Console.WriteLine("You have been signed out.\n");
            signIn();
        }

        static void exit()
        {
            Console.WriteLine("Thank you for using CyberBank. GoodBye.");
            return;
        }

        /// <summary>
        /// Configure the dependency injection 
        /// </summary>
        /// <returns></returns>
        private static IServiceProvider ConfigureSerivces()
        {
            IServiceCollection services = new ServiceCollection();

            // Define implementations of interfaces
            services.AddSingleton<IAccountRepository, AccountsCosmosDbRepository>();
            services.AddSingleton<IUserRepository, UsersCosmosDbRepository>();
            services.AddSingleton<IAccountLogic, AccountLogic>();

            // Build the service provider
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }
    }
}