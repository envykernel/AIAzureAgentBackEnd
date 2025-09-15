namespace Domain.Exceptions;

public class ConfigurationException : Exception
{
    public ConfigurationException(string message) : base(message)
    {
    }

    public ConfigurationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class AzureConfigurationException : ConfigurationException
{
    public AzureConfigurationException(string message) : base(message)
    {
    }

    public AzureConfigurationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
