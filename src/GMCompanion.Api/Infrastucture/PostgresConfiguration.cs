using System.Runtime.Serialization;

namespace GMCompanion.Api.Infrastucture;

public class PostgresConfiguration
{
    public string Host { get; set; }
    public string Port { get; set; }
    public string DataBase { get; set; }
    public string User { get; set; }
    public string Password { get; set; }

    public string BuildConnectionString()
    {
        return $"SSL Mode=Prefer;Host={Host};Port={Port};Database={DataBase};Username={User};Password={Password}";
    }

    public void Validate()
    {
        List<string> errors = new();
        if (string.IsNullOrEmpty(Host))
        {
            errors.Add("PostgresConfiguration:Host Parameter Cannot Be Null Or Empty");
        }

        if (string.IsNullOrEmpty(DataBase))
        {
            errors.Add("PostgresConfiguration:Database Parameter Cannot Be Null Or Empty");
        }

        if (string.IsNullOrEmpty(Port))
        {
            errors.Add("PostgresConfiguration:Port Parameter Cannot Be Null Or Empty");
        }

        if (string.IsNullOrEmpty(User))
        {
            errors.Add("PostgresConfiguration:User Parameter Cannot Be Null Or Empty");
        }

        if (string.IsNullOrEmpty(Password))
        {
            errors.Add("PostgresConfiguration:Password Parameter Cannot Be Null Or Empty");
        }

        if (errors.Any())
        {
            throw new Exception(string.Join("\n", errors));
        }
    }
}