namespace MyMudBlazorApp.Models
{
    public class Endpoint
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Module { get; set; }
        public string Path { get; set; }
        public string Method { get; set; }
        public bool IsActive { get; set; }
    }
} 