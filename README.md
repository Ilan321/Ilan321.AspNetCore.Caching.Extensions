# Ilan321.AspNetCore.Caching.Extensions
A handy caching extensions package.

## Usage

The `Ilan321.AspNetCore.Caching.Extensions` package currently supports the `IDistributedCache` interface, and adds these extensions:
 * `GetAsync<T>()` - gets a typed value from the `IDistributedCache`.
 * `SetAsync<T>()` - saved a typed value to the `IDistributedCache`.
 * `GetOrCreateAsync<T>()` - attempts to get a value from the `IDistributedCache`. If a value is not found (i.e. is null), then the given factory method is called, the received value is saved to the cache, and is returned.
 
 
## Examples

```csharp
public class SomeService
{
  private readonly IDistributedCache _cache;
  private readonly IModelRepository _modelRepository;

  public SomeService(
    IDistributedCache cache,
    IModelRepository modelRepository
  )
  {
    _cache = cache;
    _modelRepository = modelRepository;
  }
  
  public Task<SomeModel> GetModelByIdAsync(int id, CancellationToken cancellationToken = default)
  {
    var cacheKey = $"SomeModel:{id}";
    
    // The extension will attempt to get the value from the cache
    // If it does not exist in the cache, it will call the factory method,
    // save the result to the cache, then return it
    
    return _cache.GetOrCreateAsync<SomeModel>(
      cacheKey,
      async entry =>
      {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5); // Set the TTL to 5 minutes
        
        return _modelRepository.GetModelByIdAsync(id);
      },
      cancellationToken: cancellationToken
    );
  }
}
```
