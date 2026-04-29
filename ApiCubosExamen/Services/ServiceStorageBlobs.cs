using ApiCubosExamen.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

public class ServiceStorageBlobs
{
    private BlobServiceClient client;

    public ServiceStorageBlobs(BlobServiceClient client)
    {
        this.client = client;
    }

    //SUBIR UN BLOB A UN CONTAINER 
    public async Task UploadBlobAsync(string containerName, string blobName, Stream stream)
    {
        BlobContainerClient containerClient =
        this.client.GetBlobContainerClient(containerName);
        await containerClient.UploadBlobAsync
        (blobName, stream);
    }
    public string GetContainerUrl(string containerName)
    {
        // Asumiendo que tienes tu BlobServiceClient instanciado en tu servicio
        BlobContainerClient containerClient = this.client.GetBlobContainerClient(containerName);

        return containerClient.Uri.ToString();
    }

    public string GetBlobSasUrl(string containerName, string blobName)
    {
        BlobContainerClient containerClient = this.client.GetBlobContainerClient(containerName);
        BlobClient blobClient = containerClient.GetBlobClient(blobName);

        // Comprobamos si podemos generar el SAS token
        if (blobClient.CanGenerateSasUri)
        {
            // Creamos un token que expira en 1 hora y solo da permiso de Lectura
            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "b", // "b" significa que el permiso es para un Blob
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            // Generamos la URL completa con el token incluido
            Uri sasUri = blobClient.GenerateSasUri(sasBuilder);
            return sasUri.ToString();
        }

        return null;
    }
}