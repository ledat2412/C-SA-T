using MauiApp1.Models;
using SQLite;

namespace MauiApp1.Services
{
    public class SQLiteService
    {
        private readonly SQLiteAsyncConnection _db;
        private static readonly TimeSpan DefaultExpiredGracePeriod = TimeSpan.FromDays(30);

        public SQLiteService()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "vinhkhanh_cache.db3");
            _db = new SQLiteAsyncConnection(dbPath);
        }

        public async Task InitAsync()
        {
            await _db.CreateTableAsync<AppCacheEntry>();
        }

        public Task<int> UpsertCacheAsync(AppCacheEntry entry)
        {
            return _db.InsertOrReplaceAsync(entry);
        }

        public async Task<AppCacheEntry?> GetCacheAsync(string cacheKey)
        {
            return await _db.Table<AppCacheEntry>()
                .FirstOrDefaultAsync(x => x.CacheKey == cacheKey);
        }

        public async Task<AppCacheEntry?> GetCacheIfFreshAsync(string cacheKey, TimeSpan maxAge)
        {
            var entry = await GetCacheAsync(cacheKey);
            if (entry is null)
                return null;

            var age = DateTime.UtcNow - entry.UpdatedAtUtc;
            if (age <= maxAge)
                return entry;

            return null;
        }

        public Task<int> DeleteCacheAsync(string cacheKey)
        {
            return _db.DeleteAsync<AppCacheEntry>(cacheKey);
        }

        public Task<int> ClearAllCacheAsync()
        {
            return _db.DeleteAllAsync<AppCacheEntry>();
        }

        public async Task<int> DeleteExpiredCacheAsync(TimeSpan maxAge)
        {
            var cutoff = DateTime.UtcNow - maxAge;
            var expiredEntries = await _db.Table<AppCacheEntry>()
                .Where(x => x.UpdatedAtUtc < cutoff)
                .ToListAsync();

            var deleted = 0;
            foreach (var entry in expiredEntries)
            {
                deleted += await _db.DeleteAsync<AppCacheEntry>(entry.CacheKey);
            }

            return deleted;
        }

        public Task<int> CleanupOldCacheAsync()
        {
            return DeleteExpiredCacheAsync(DefaultExpiredGracePeriod);
        }
    }
}
