using Core;
using Newtonsoft.Json;

namespace App;

class Root
{
    public string RedfishVersion { get; set; }
    
    public List<System> Systems { get; set; }
    
    public List<Chassis> Chassis { get; set; }
}

class Chassis
{
    public string Manufacturer { get; set; }
}

class System
{
    public string PartNumber { get; set; }
    
    public string PowerState { get; set; }
    
    public string Description { get; set; }
    
    public Boot Boot { get; set; }
    
    public List<Memory> Memory { get; set; }
}

class Memory
{
    public string MemoryType { get; set; }
    
    public string ErrorCorrection { get; set; }
    
    public MemoryLocation MemoryLocation { get; set; }
}

class MemoryLocation
{
    public string Slot { get; set; }
    
    public string Channel { get; set; }
}

class Boot
{
    public string UefiTargetBootSourceOverride { get; set; }
}

internal class Program
{
    public static async Task Main(string[] args)
    {
        var httpClientHandler = new HttpClientHandler();
        httpClientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true; 
        var httpClient = new HttpClient(new HttpClientHandler());

        var oDataClient = new ODataClient(httpClient);

        var result = await oDataClient.Get<Root>(new Uri("http://localhost:8000/redfish/v1"));
            
        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
    }
}