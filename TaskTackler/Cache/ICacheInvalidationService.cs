namespace TaskTackler.Cache;

public interface ICacheInvalidationService
{
    Task ClearCacheForAffectedPages(int todoId);
    Task ClearCacheForLastPage();
    Task ClearCacheForAllPages();
}
