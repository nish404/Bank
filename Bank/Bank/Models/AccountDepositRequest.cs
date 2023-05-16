namespace Logic.Models
{
    public class AccountDepositRequest
    {
        /// <summary>
        /// The amount to be deposited into the account
        /// </summary>
        public decimal amount { get; set; }

        /// <summary>
        /// The account number money will be deposited into
        /// </summary>
        public int accountNumber { get; set; }
    }
}
