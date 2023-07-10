using System.Diagnostics;
using System.Reflection;
using App.Attributes;
using Core.Interfaces;
using Newtonsoft.Json.Linq;

namespace Core;

public class ODataClient : IODataClient
{
    private const string ODATA_ID = "@odata.id";

    private const string MEMBERS = "Members";

    private readonly HttpClient _httpClient;

    public ODataClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<T> Get<T>(Uri url) where T : class, new()
    {
        var absoluteUriWithoutPath = url.GetComponents(
            UriComponents.Scheme | UriComponents.Host | UriComponents.Port,
            UriFormat.UriEscaped);
        
        return (T)await TraverseType(typeof(T), await Get(url.AbsoluteUri), absoluteUriWithoutPath);
    }

    private async Task<JObject> Get(string url)
    {
        var response = await _httpClient.GetAsync(url);

        var content = await response.Content.ReadAsStringAsync();

        var jsonContent = JObject.Parse(content);

        return jsonContent;
    }

    private static bool IsPrimitiveSystemType(Type type)
    {
        return type.IsPrimitive || type.IsValueType || type == typeof(string);
    }

    private static bool IsGenericListType(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
    }

    private async Task<object> TraverseType(Type type, JToken jsonContent, string baseUrl)
    {
        Debug.Assert(type.GetConstructor(Type.EmptyTypes) != null);

        var instance = Activator.CreateInstance(type)!;

        foreach (var propertyInfos in type
                     .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty |
                                    BindingFlags.SetProperty))
        {
            var propertyInfo = type.GetProperty(propertyInfos.Name)!;

            var odataAttribute = propertyInfo.GetCustomAttribute<ODataPropertyAttribute>();

            var odataName = odataAttribute?.Name ?? propertyInfo.Name;

            var jToken = jsonContent.SelectToken(odataName);

            switch (jToken)
            {
                case JValue jValue when IsPrimitiveSystemType(propertyInfos.PropertyType):
                    propertyInfo.SetValue(instance,
                        Convert.ChangeType(jValue.Value<string>(), propertyInfo.PropertyType));
                    continue;
                case JObject jObject when jObject.Property(ODATA_ID) != null:
                    var intermediateJObject = await Get(baseUrl + jObject.Value<string>(ODATA_ID) + "?$expand=.");
                    var membersJArray = intermediateJObject.SelectToken(MEMBERS);

                    if (membersJArray != null && IsGenericListType(propertyInfo.PropertyType))
                    {
                        var genericArg = propertyInfos.PropertyType.GenericTypeArguments[0];

                        var recursiveResults = await Task.WhenAll(membersJArray
                            .Select(o => TraverseType(genericArg, o, baseUrl)));

                        propertyInfo.SetValue(instance, CreateAndPopulateList(genericArg, recursiveResults));
                    }

                    break;
                case JObject jObject:
                    propertyInfo.SetValue(instance, jObject.ToObject(propertyInfo.PropertyType));
                    break;
            }
        }

        return instance;
    }

    private static object CreateAndPopulateList(Type genericType, IEnumerable<object> items)
    {
        var listType = typeof(List<>).MakeGenericType(genericType);

        var genericList = Activator.CreateInstance(listType);
        foreach (var item in items)
        {
            genericList.GetType().GetMethod("Add").Invoke(genericList, new[] { item });
        }

        return genericList;
    }
}
