// took from https://docs.microsoft.com/en-us/azure/applied-ai-services/form-recognizer/quickstarts/try-v3-csharp-sdk

using Microsoft.Extensions.Configuration;

public class Program
{
    public static async Task Main(string[] args)
    {   
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var formRecognizerOptions = config
            .GetRequiredSection("FormRecognizer")
            .Get<FormRecognizerOptions>();

        var storageOptions = config
            .GetRequiredSection("Storage")
            .Get<StorageOptions>();

        var filePath = "sample-layout.pdf";

        var documentStore = new DocumentStore(storageOptions);
        var filename = await documentStore.Store(filePath);
        var storageUrl = $"https://{storageOptions.AccountName}.blob.{storageOptions.EndpointSuffix}";

        Console.WriteLine($"Stored document: {storageUrl}/{storageOptions.ContainerName}/{filename}");

        var readUrl = documentStore.GetAccessUrl(filename);
        Console.WriteLine($"SAS Read Url: {readUrl}");

        await new PrebuiltModels(formRecognizerOptions).Analyze(readUrl);
    }
}