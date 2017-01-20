# AzureStorage
A library for downloading and uploading files to an Azure Storage Blob

# Using AzureStorage.Blob

Blob can be used to upload files from http multipart/form-data requests and download files form the blob,
this allows for a restful service to easily upload and download files to and from an Azure Storage Blob.

# Using AzureStorage.Blob with WebApi

* Initialise blob 
  - with account name and key

  ```
    AzureStorage.Blob blob = new AzureStorage.Blob("AccountName", "AccountKey", "ContainerName");
  ```

  - with conncetion string

  ```
    AzureStorage.Blob blob = new AzureStorage.Blob(
      Microsoft.Azure.CloudConfigurationManager.GetSetting("StorageConnectionString"), "ContainerName");
  ```

* Uploading files to Azure Storage Blob

```
  ICollection<AzureStorage.File> files = blob.Upload(await Request.Content.ReadAsStreamAsync());
```

* Downloading a file from AzureStorageBlob

```
  AzureStorage.File file = blob.Download("FileAddress");
```

* Downloading files from AzureStorageBlob

```
  ICollection<AzureStorage.File> files = blob.Download(new List<string> {
                                        "https://exampleAccount.blob.core.windows.net/1.jpg",
                                        "https://exampleAccount.blob.core.windows.net/2.jpg",   
                                        "https://exampleAccount.blob.core.windows.net/3.jpg",});
```


