namespace Logic.Models
{
    public class AccountWithdrawRequest
    {
        /// <summary>
        /// The amount to be withdrawn from the account
        /// </summary>
        public decimal amount { get; set; }

        /// <summary>
        /// The pin number of the account
        /// </summary>
        public string pin { get; set; }
    }
}
