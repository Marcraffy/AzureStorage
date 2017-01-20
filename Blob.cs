using HttpMultipartParser;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AzureStorage
{
    /// <summary>
    /// Class for uploading and downloading files to an Azure Storage blob
    /// </summary>
    public class Blob
    {
        private CloudBlobContainer container;
        private CloudBlobClient blobClient;

        /// <summary>
        /// Blob constructor for code based authentication
        /// </summary>
        /// <param name="accountName" cref="string">Storage account name on azure</param>
        /// <param name="accountKey" cref="string">Either Key 1 or 2 of the azure storage account</param>
        /// <param name="containerName" cref="string">Name of the container to access</param>
        /// <param name="permissions" cref="BlobContainerPermissions">Permissions for accessing the container (default: public access)</param>
        public Blob(string accountName, string accountKey, string containerName, BlobContainerPermissions permissions = null)
            : this($"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accountKey}", containerName, permissions) { }

        /// <summary>
        /// Blob constructor for configuration based authentication
        /// </summary>
        /// <param name="connectionString" cref="string">Connection string providing account name and key</param>
        /// <param name="containerName" cref="string">Name of the container to access</param>
        /// <param name="permissions" cref="string">Permissions for accessing the container (default: public access)</param>
        public Blob(string connectionString, string containerName, BlobContainerPermissions permissions = null)
        {
            blobClient = CloudStorageAccount.Parse(connectionString).CreateCloudBlobClient();
            container = blobClient.GetContainerReference(containerName.ToLowerInvariant());
            container.CreateIfNotExists();
            container.SetPermissions(permissions ?? new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
        }

        /// <summary>
        /// Gets a collection of URI's of files in an azure storage blob
        /// </summary>
        /// <returns>
        /// A collection of uri of all the files in the blob
        /// </returns>
        public ICollection<string> GetURIs()
        {
            var uris = new List<string>();
            foreach (var item in container.ListBlobs(null, true))
            {
                uris.Add(((CloudBlob)item).StorageUri.PrimaryUri.AbsoluteUri);
            }
            return uris;
        }

        /// <summary>
        /// Upload one or more files to blob from multipart/form-data
        /// </summary>
        /// <param name="content">Stream containing http multipart/form-data content</param>
        /// <param name="id">Name to give all files when saving to blob, each item will be appended with a sequential number</param>
        /// <returns>
        /// A collection of files containing the data stream and their addresses on the azure storage blob
        /// </returns>
        public ICollection<File> Upload(Stream content, string id = null)
        {
            var parser = new MultipartFormDataParser(content);

            id = id ?? parser.GetParameterValue("id");
            int fileCount = 0;
            var collection = new List<File>();
            foreach (var file in parser.Files)
            {
                var name = (id == null) ? file.Name : $"{id}-{fileCount.ToString()}";
                var fileName = $"{name}.{GetExtension(file.FileName)}";
                collection.Add(Upload(fileName, file.Data));
                fileCount++;
            }
            return collection;
        }

        /// <summary>
        /// Download a file from the blob
        /// </summary>
        /// <param name="fileName" cref="string">The uri of the file to download</param>
        /// <returns>
        /// A file containing the data stream and their addresses on the azure storage blob
        /// </returns>
        public File Download(string fileName) =>
            new File { FileUri = fileName, Data = DownloadFromBlob(fileName) };

        /// <summary>
        /// Download files from the blob
        /// </summary>
        /// <param name="fileNames" cref="ICollection{T}">A collection of Uri's to download</param>
        /// <returns>
        /// A collection of files containing the data stream and their addresses on the azure storage blob
        /// </returns>
        public ICollection<File> Download(ICollection<string> fileNames)
        {
            var files = new List<File>();
            foreach (var fileName in fileNames)
            {
                files.Add(Download(fileName));
            }
            return files;
        }

        /// <summary>
        /// Download every file in the blob
        /// </summary>
        /// <returns>
        /// A collection of files containing the data stream and their addresses on the azure storage blob
        /// </returns>
        public ICollection<File> DownloadBlob()
        {
            var files = new List<File>();
            foreach (var item in container.ListBlobs(null, true))
            {
                var stream = new MemoryStream();
                ((CloudBlob)item).DownloadToStream(stream);
                files.Add(new File { FileUri = item.Uri.AbsoluteUri, Data = stream });
            }
            return files;
        }

        private void UploadToBlob(string name, Stream stream) =>
            container.GetBlockBlobReference(name).UploadFromStream(stream);

        private File Upload(string fileName, Stream content)
        {
            UploadToBlob(fileName, content);
            return new File { FileUri = $"{blobClient.StorageUri.PrimaryUri + fileName}", Data = content };
        }

        private Stream DownloadFromBlob(string fileName)
        {
            var stream = new MemoryStream();
            container.GetBlockBlobReference(fileName).DownloadToStream(stream);
            return stream;
        }

        private string GetExtension(string fileName) => fileName.Split('.').Last();
    }
}