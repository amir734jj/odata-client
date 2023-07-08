using Newtonsoft.Json.Linq;

namespace App.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ODataPropertyAttribute : Attribute
{
    public string Name { get; set; }
    
    public Func<JToken, object> Converter { get; set; }
}