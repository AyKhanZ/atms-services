## How RedisCacheService works

### Fast path (cache hit)
Request comes in → check Redis → data found → return immediately (0.1ms)

If TTL has less than 30 seconds left and no background refresh is running → launch background refresh silently.
User gets current value without any delay. Background refresh updates cache so next requests get fresh data.

### Slow path (cache miss)
Request comes in → check Redis → data not found → acquire semaphore.

Only ONE thread gets the semaphore and goes to DB. All other threads wait.
When the first thread finishes and writes to cache → releases semaphore → waiting threads check cache again and read from it.
This prevents 500 identical DB requests at the same time (Cache Stampede).

### Double check after semaphore
After a thread acquires the semaphore it checks Redis again before going to DB.
While it was waiting — the first thread may have already filled the cache.
Without this check all 500 threads would go to DB one by one even with the semaphore.

### Background refresh protection
Only one background refresh runs per key at a time.
`RefreshingKeys.TryAdd` returns true only for the first thread — all others skip.
After refresh completes the flag is removed so next cycle can refresh again when needed.

### Redis is down
If Redis crashes — the app continues working without cache.
All requests go directly to DB until Redis recovers. No exceptions reach the user.

### Cache invalidation
Always call `RemoveAsync` after Update or Delete.
TTL is just a safety net — the main freshness mechanism is manual invalidation.

```
Request flow:
GET by id  → check Redis → hit  → return (fast)
                        → miss → DB → write to Redis → return (slow, once)

UPDATE     → update DB → RemoveAsync → next GET fills cache again
DELETE     → delete DB → RemoveAsync → next GET returns 404
```

---

### Set settings to docker-compose.yaml
```
VPS 1GB RAM:   maxmemory 128mb  (you leave 872mb for databases and services)
VPS 2GB RAM:   maxmemory 256mb
VPS 4GB RAM:   maxmemory 512mb
```

### Go into container
```
docker exec -it atms-redis redis-cli
```

### Redis basic commands
```
# See all keys
KEYS *

# See all keys by patters
KEYS atms:admin:*

# See the value by key
GET atms:admin:user:42

# See how many seconds the key has left to live
TTL atms:admin:user:42

# How many keys are there in total
DBSIZE

# Memory Information
INFO memory

# Delete the key manually (for the disability test)
DEL atms:admin:user:42

# Exit
EXIT
```