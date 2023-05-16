namespace Bank
{
    /// <summary>
    /// Defines the result of the operation made by an operation
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to be returned upon success</typeparam>
    public class Result<TEntity>
    {
        /// <summary>
        /// Indicates whether the operation was successful or not
        /// </summary>
        public bool Succeeded { get; set; }

        /// <summary>
        /// Provides additinal information why the opperation was not successful
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Provides the nature of the result (Success, NotFound, InvalidData)
        /// </summary>
        public string ResultType { get; set; }

        /// <summary>
        /// Provides the value of the result, if successful
        /// </summary>
        public TEntity Value { get; set; }
    }

    public class ResultType
    {
        public const string Success = "Success";
        public const string NotFound = "NotFound";
        public const string InvalidData = "InvalidData";
        public const string Duplicate = "Duplicate";
        public const string DataStoreError = "DataStoreError";
    }
}
