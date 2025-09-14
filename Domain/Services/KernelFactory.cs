using Azure.Identity;
using Microsoft.SemanticKernel;
using Domain.Interfaces;

namespace Domain.Services;

public class KernelFactory : IKernelFactory
{
    public Kernel CreateKernel()
    {
        // Récupérer les variables d'environnement OPEN_AI_DEPLOYMENT_NAME et OPEN_AI_AZURE_OPEN_AI_ENDPOINT.
        string OpenAI_DEPLOYMENT_NAME = Environment.GetEnvironmentVariable("OPEN_AI_DEPLOYMENT_NAME") ?? throw new ArgumentNullException("OPEN_AI_DEPLOYMENT_NAME");
        string OpenAI_AZURE_OPEN_AI_ENDPOINT = Environment.GetEnvironmentVariable("OPEN_AI_AZURE_OPEN_AI_ENDPOINT") ?? throw new ArgumentNullException("OPEN_AI_AZURE_OPEN_AI_ENDPOINT");

        // Créer une instance de DefaultAzureCredential (avec des options par défaut ou personnalisées).
        var authOptions = new DefaultAzureCredentialOptions { ExcludeAzureDeveloperCliCredential = false };
        var credential = new DefaultAzureCredential(authOptions);

        // Utiliser Kernel.CreateBuilder() pour ajouter la complétion de chat Azure OpenAI (via AddAzureOpenAIChatCompletion) en passant le nom du déploiement, l'endpoint et le credential.
        var kernelBuilder = Kernel
        .CreateBuilder()
        .AddAzureOpenAIChatCompletion(deploymentName: OpenAI_DEPLOYMENT_NAME,
         endpoint: OpenAI_AZURE_OPEN_AI_ENDPOINT,
          credentials: credential);

        // Construire et retourner le kernel
        Kernel kernel = kernelBuilder.Build();
        return kernel;
    }
}
