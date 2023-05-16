namespace Logic.Models
{
    using Newtonsoft.Json;
    using System;

    /// <summary>
    /// Defines an Account object - the top-level representation of a bank account
    /// in the system.
    /// </summary>
    public class Account
    {
        /// <summary>
        /// Unique document identifier.
        /// The document identifier is used for document-based repositories and is the string
        /// representation of the user id.
        /// </summary>
        /// Tell the serializer that this field should be named "id" in the json document.
        /// Tell the deserializer that a field named "id" in the json document should map to this property.
        /// This is required because the Id field in cosmos db is id (lower case)
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Unique identifier of the user the account belongs to
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Unique account identifier
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Represents the balance of the account
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The account's pin number
        /// </summary>
        public string Pin { get; set; }

        /// <summary>
        /// Creates a new istance
        /// </summary>
        public Account()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
