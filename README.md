# OData-Client

Tiny utility library to collect and map OData result with minimal effort.

Example:

Step 1: provide the class structure of how you want to collect the OData:

```csharp
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
```

Step 2: initialize the OData client with a URL and `HttpClient`

```csharp
var result = await oDataClient.Get<Root>(new Uri("http://localhost:8000/redfish/v1"));
```

Step 3: result

```json
{
  "RedfishVersion": "1.15.0",
  "Systems": [
    {
      "PartNumber": "224071-J23",
      "PowerState": "On",
      "Description": "Web Front End node",
      "Boot": {
        "UefiTargetBootSourceOverride": "/0x31/0x33/0x01/0x01"
      },
      "Memory": [
        {
          "MemoryType": "DRAM",
          "ErrorCorrection": "MultiBitECC",
          "MemoryLocation": {
            "Slot": "1",
            "Channel": "1"
          }
        },
        {
          "MemoryType": "DRAM",
          "ErrorCorrection": "MultiBitECC",
          "MemoryLocation": {
            "Slot": "2",
            "Channel": "1"
          }
        },
        {
          "MemoryType": "DRAM",
          "ErrorCorrection": "MultiBitECC",
          "MemoryLocation": {
            "Slot": "3",
            "Channel": "1"
          }
        },
        {
          "MemoryType": null,
          "ErrorCorrection": null,
          "MemoryLocation": {
            "Slot": "4",
            "Channel": "1"
          }
        }
      ]
    }
  ],
  "Chassis": [
    {
      "Manufacturer": "Contoso"
    }
  ]
}
```