using System.IO;

namespace AzureStorage
{

    /// <summary>
    /// A struct that contains the URI and data from a file on an azure storage blob
    /// </summary>
    public struct File
    {
        /// <summary>
        /// The URI of the file on the blob
        /// </summary>
        public string FileUri { get; set; }

        /// <summary>
        /// The data of the file on the blob
        /// </summary>
        public Stream Data { get; set; }
    }
}