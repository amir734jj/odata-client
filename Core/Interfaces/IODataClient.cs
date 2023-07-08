namespace Core.Interfaces;

public interface IODataClient
{
    public Task<T> Get<T>(Uri url) where T : class, new();
}