﻿using System;
using System.Runtime.Caching;
using Events.Business.Interfaces;

namespace Events.RuntimeCache
{
    public class RuntimeCacheManager : ICacheManager
    {
        ObjectCache cache;

        public RuntimeCacheManager()
        {
            cache = MemoryCache.Default;
        }

        public TValue FromCache<TValue>(string key, Func<TValue> function)
        {
            var itemToSave = function();

            if (itemToSave != null)
            {
                CacheItem cacheItem = ToCache<TValue>(key,
                    () =>
                    {
                        return itemToSave;
                    });
            }

            return itemToSave;
        }

        public void ClearCacheByRegion(string region)
        {
            foreach (var item in MemoryCache.Default)
            {
                if (item.Key.Contains(region))
                {
                    MemoryCache.Default.Remove(item.Key);

                }
            }
        }

        public CacheItem ToCache<TValue>(string key, Func<TValue> function)
        {
            var itemToSave = function();

            if (itemToSave == null)
            {
                return null;
            }

            CacheItem cacheItem = cache.GetCacheItem(key);
            if (cacheItem == null)
            {
                CacheItemPolicy policy = new CacheItemPolicy();
                policy.AbsoluteExpiration = DateTimeOffset.Now.AddHours(1);
                cacheItem = new CacheItem(key, itemToSave);
                cache.Set(cacheItem, policy);
            }
            return cacheItem;
        }

        public void RemoveFromCache(string key)
        {
            CacheItem cacheItem = cache.GetCacheItem(key);
            if (cacheItem != null)
            {
                MemoryCache.Default.Remove(cacheItem.Key);
            }
        }
    }
}
