using BackendAPIService.BaseObjects;

namespace BackendAPIService.RequestObjects;

public class EndPointAddRequest
{
    public int UserID { get; set; }
    public string Path { get; set; }
    public List<Parameter> Parameters { get; set; }
}