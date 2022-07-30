using Azure.Storage.Blobs;
using Azure.Storage.Sas;

public class DocumentStore
{
    private StorageOptions _storageOptions;

    public DocumentStore(StorageOptions storageOptions)
    {
        _storageOptions = storageOptions;
    }

    public async Task<string> Store(string path)
    {
        var client = GetContainer();

        var filename = Guid.NewGuid().ToString("N");

        var sas = GetWriteSAS(client, filename);

        await UploadAsync(sas, path);

        return filename;
    }

    public string GetAccessUrl(string url)
    {
        var client = GetContainer();
        return GetReadSAS(client, url).AbsoluteUri;
    }

    private BlobContainerClient GetContainer()
    {
        var connectionString = "DefaultEndpointsProtocol=https;" +
            $"AccountName={_storageOptions.AccountName};" +
            $"AccountKey={_storageOptions.AccountKey};" +
            $"EndpointSuffix={_storageOptions.EndpointSuffix}";

        BlobServiceClient service = new BlobServiceClient(connectionString);

        return service.GetBlobContainerClient(_storageOptions.ContainerName);
    }

    private Uri GetWriteSAS(BlobContainerClient containerClient, string filename)
    {
        var blobClient = containerClient.GetBlobClient(filename);

        return blobClient
        .GenerateSasUri(BlobSasPermissions.Write, DateTimeOffset.UtcNow.AddMinutes(5));
    }

    private Uri GetReadSAS(BlobContainerClient containerClient, string filename)
    {
        var blobClient = containerClient.GetBlobClient(filename);

        return blobClient
        .GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(5));
    }

    private async Task UploadAsync(Uri sas, string path)
    {
        BlobClient blobClient = new BlobClient(sas, null);
        await blobClient.UploadAsync(path);
    }
}