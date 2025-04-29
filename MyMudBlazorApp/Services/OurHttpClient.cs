namespace MyMudBlazorApp.Services;

public class OurHttpClient : HttpClient
{
    public OurHttpClient()
    {
        BaseAddress = new Uri("http://localhost:5000");
    }
}