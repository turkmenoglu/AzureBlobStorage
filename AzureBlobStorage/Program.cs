using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount  
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types  

namespace AzureBlobStorage
{
    public class Program
    {
        public static CloudBlobContainer cloudBlobContainer = null;
        public static CloudBlockBlob cloudBlockBlob = null;
        public static string sourceFile = null;
        public static string destinationFile = null;
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Azure Blob Storage -berkarat.com");
            Console.WriteLine();
            await CreateandSend();

        }
        private static async Task CreateandSend()
        {

            // Retrieve the connection string for use with the application. The storage connection string is stored
            // in an environment variable on the machine running the application called AZURE_STORAGE_CONNECTIONSTRING.
            // If the environment variable is created after the application is launched in a console or with Visual
            // Studio, the shell needs to be closed and reloaded to take the environment variable into account.
            string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=san1turkmenoglu;AccountKey=85aO6vbWLz3qO9t+0I0nWdTLCYrpK1DXd+MvPb14TF04sQ23Inm9Lqm/Cge728vs7e+9aayibjd4Ffwb3b7RKw==;EndpointSuffix=core.windows.net";

            // Check whether the connection string can be parsed.
            if (!CloudStorageAccount.TryParse(storageConnectionString, out CloudStorageAccount storageAccount))
            {
                Console.WriteLine(
                    "A connection string has not been defined in the system environment variables. " +
                    "Add a environment variable named 'storageconnectionstring' with your storage " +
                    "connection string as a value.");

                return;
            }

            try
            {
                // Create the CloudBlobClient that represents the Blob storage endpoint for the storage account.
                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
                // Create a container called 'quickstartblob' and append a GUID value to it to make the name unique. 
                cloudBlobContainer = cloudBlobClient.GetContainerReference("container1");
                bool isExists = cloudBlobContainer.Exists();
                if (!isExists)
                {
                    await cloudBlobContainer.CreateAsync();
                    Console.WriteLine($"Created container '{cloudBlobContainer.Name}'");
                }


                Console.WriteLine();

                // Set the permissions so the blobs are public. 
                // This means you'll be able to download it anonymously via HTTPS using the URL displayed in the console.
                BlobContainerPermissions permissions = new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                };

                await cloudBlobContainer.SetPermissionsAsync(permissions);

                // Create a file in your local MyDocuments folder to upload to a blob.
                string localPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string localFileName = $"Log" + DateTime.Now.AddDays(1).ToString("yyyyMMdd") + ".txt";
                sourceFile = Path.Combine(localPath, localFileName);

                // Write text to the file.
                File.WriteAllText(sourceFile, "Hello, turkmenoglu!");

                Console.WriteLine($"Temp file = {sourceFile}");
                Console.WriteLine($"Uploading to Blob storage as blob '{localFileName}'");
                Console.WriteLine();

                cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(localFileName);
                await cloudBlockBlob.UploadFromFileAsync(sourceFile);

            }
            catch (StorageException ex)
            {
                Console.WriteLine($"Error returned from the service: {ex.Message}");
            }


            await Download();

            await Delete();

        }
        public static async Task Delete()
        {
            Console.WriteLine("Deleting the container and any blobs it contains");
            if (cloudBlobContainer != null)
            {
                await cloudBlobContainer.DeleteIfExistsAsync();
                Console.WriteLine("Deleting the local source file and local downloaded files");
                Console.WriteLine();
                if (File.Exists(sourceFile))
                {
                    File.Delete(sourceFile);
                }
                if (File.Exists(destinationFile))
                {
                    File.Delete(destinationFile);
                }

            }









        }
        public static async Task Download()
        {


            Console.WriteLine("Listing blobs in container.");
            BlobContinuationToken blobContinuationToken = null;

            do
            {
                var resultSegment = await cloudBlobContainer.ListBlobsSegmentedAsync(null, blobContinuationToken);

                // Get the value of the continuation token returned by the listing call.
                blobContinuationToken = resultSegment.ContinuationToken;
                foreach (IListBlobItem item in resultSegment.Results)
                {
                    Console.WriteLine(item.Uri);
                }
            }

            while (blobContinuationToken != null); // Loop while the continuation token is not null.
            Console.WriteLine();

            // Download the blob to a local file, using the reference created earlier. 
            // Append the string "_DOWNLOADED" before the .txt extension so that you can see both files in MyDocuments.
            destinationFile = sourceFile.Replace(".txt", "_DOWNLOADED.txt");
            Console.WriteLine($"Downloading blob to {destinationFile}");
            Console.WriteLine();

            await cloudBlockBlob.DownloadToFileAsync(destinationFile, FileMode.Create);
        }

    }



}
