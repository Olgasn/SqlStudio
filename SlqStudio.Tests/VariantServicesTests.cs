using Microsoft.Extensions.Caching.Memory;
using SlqStudio.Application.Services.VariantServices;
using SlqStudio.Persistence.Models;

namespace SlqStudio.Tests;

public sealed class VariantServicesTests
{
    [Fact]
    public void GenerateVariant_WhenCacheKeyIsEmpty_ThrowsArgumentException()
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var service = new VariantServices(memoryCache);

        var ex = Assert.Throws<ArgumentException>(() => service.GenerateVariant(new List<LabTask>(), string.Empty));
        Assert.Equal("cacheKey", ex.ParamName);
    }

    [Fact]
    public void GenerateVariant_GroupsByTaskNumber_AndUsesCacheByKey()
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var service = new VariantServices(memoryCache);
        const string cacheKey = "student@example.com";

        var allTasks = new List<LabTask>
        {
            new() { Id = 1, Number = 1, Title = "t1a", Condition = "c", SolutionExample = "s" },
            new() { Id = 2, Number = 1, Title = "t1b", Condition = "c", SolutionExample = "s" },
            new() { Id = 3, Number = 2, Title = "t2a", Condition = "c", SolutionExample = "s" },
            new() { Id = 4, Number = 2, Title = "t2b", Condition = "c", SolutionExample = "s" },
            new() { Id = 5, Number = 3, Title = "t3a", Condition = "c", SolutionExample = "s" }
        };

        var first = service.GenerateVariant(allTasks, cacheKey);

        Assert.Equal(3, first.Count);
        Assert.Equal(new[] { 1, 2, 3 }, first.Select(t => t.Number).OrderBy(n => n).ToArray());
        Assert.All(first, task => Assert.Contains(task, allTasks));

        var second = service.GenerateVariant(
            new List<LabTask> { new() { Id = 999, Number = 99, Title = "x", Condition = "c", SolutionExample = "s" } },
            cacheKey);

        Assert.Same(first, second);
        Assert.Equal(new[] { 1, 2, 3 }, second.Select(t => t.Number).OrderBy(n => n).ToArray());
    }

    [Fact]
    public void GetVariantFromCache_WhenMissing_ReturnsEmptyList()
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var service = new VariantServices(memoryCache);

        var result = service.GetVariantFromCache("missing-key");

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ClearVariantCache_RemovesCachedVariant()
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var service = new VariantServices(memoryCache);
        const string cacheKey = "student@example.com";

        var tasks = new List<LabTask>
        {
            new() { Id = 1, Number = 1, Title = "t1a", Condition = "c", SolutionExample = "s" }
        };

        _ = service.GenerateVariant(tasks, cacheKey);
        Assert.NotEmpty(service.GetVariantFromCache(cacheKey));

        service.ClearVariantCache(cacheKey);

        Assert.Empty(service.GetVariantFromCache(cacheKey));
    }
}
