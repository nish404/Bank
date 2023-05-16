namespace Logic.Models
{
    public class AccountTransferRequest
    {
        /// <summary>
        /// The amount to be transferred into the account
        /// </summary>
        public decimal amount { get; set; }

        /// <summary>
        /// The account number money will be transferred from
        /// </summary>
        public int sourceAccountNumber { get; set; }

        /// <summary>
        /// The pin number of the source account
        /// </summary>
        public string sourcePin { get; set; }

        /// <summary>
        /// The account number money will be transferred to
        /// </summary>
        public int destinationAccountNumber { get; set; }
    }
}
