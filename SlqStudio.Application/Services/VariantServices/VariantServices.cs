using Microsoft.Extensions.Caching.Memory;
using SlqStudio.Persistence.Models;

namespace SlqStudio.Application.Services.VariantServices;

public class VariantServices
{
    private readonly IMemoryCache _cache;
    private readonly MemoryCacheEntryOptions _cacheOptions;

    public VariantServices(IMemoryCache memoryCache)
    {
        _cache = memoryCache;
        _cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        };
    }

    public List<LabTask> GenerateVariant(List<LabTask> allTasks, string cacheKey)
    {
        if (string.IsNullOrWhiteSpace(cacheKey))
        {
            throw new ArgumentException("Cache key cannot be null or empty", nameof(cacheKey));
        }
        if (_cache.TryGetValue(cacheKey, out List<LabTask>? cachedVariant) && cachedVariant is not null)
        {
            return cachedVariant;
        }

        if (allTasks == null || allTasks.Count == 0)
        {
            return new List<LabTask>();
        }

        var tasksByNumber = allTasks.GroupBy(task => task.Number);
        var variant = new List<LabTask>();
        var random = new Random();

        foreach (var group in tasksByNumber)
        {
            var tasksInGroup = group.ToList();
            variant.Add(tasksInGroup[random.Next(tasksInGroup.Count)]);
        }

        _cache.Set(cacheKey, variant, _cacheOptions);
        return variant;
    }
    
    public List<LabTask> GetVariantFromCache(string cacheKey)
    {
        if (string.IsNullOrWhiteSpace(cacheKey))
        {
            throw new ArgumentException("Cache key cannot be null or empty", nameof(cacheKey));
        }
        if (_cache.TryGetValue(cacheKey, out List<LabTask>? cachedVariant) && cachedVariant is not null)
        {
            return cachedVariant;
        }
        return new List<LabTask>();
    }
    
    public void ClearVariantCache(string cacheKey)
    {
        _cache.Remove(cacheKey);
    }
}
