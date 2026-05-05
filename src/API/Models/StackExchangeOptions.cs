namespace API.Models;

public class StackExchangeOptions
{
    public string BaseUrl { get; set; } = "https://api.stackexchange.com/2.3/";
    public string UserAgent { get; set; } = "DotNetWebApp/1.0";
}