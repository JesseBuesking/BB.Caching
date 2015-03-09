BB.Caching
==========

BB.Caching is a library for (ASP) .NET developers with the goal of making caching as simple as possible. With out-of-the-box support for [Redis](http://redis.io) [on top of StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis), you can quickly get scalable, distributed caching up and running with minimal configuration overhead.

_Note: This code is not production ready, but has a lot of [tests](https://github.com/JesseBuesking/BB.Caching/tree/master/BB.Caching.Tests) that cover many cases._

What can it be used for?
------------------------
1. Smart caching (in-memory and/or distributed)
2. Rate limiting
3. Bloom filters
4. Simple bitwise analytics

Documentation
-------------
Check out the [tests](https://github.com/JesseBuesking/BB.Caching/tree/master/BB.Caching.Tests).

Get Started
-----------
In your startup process:

1. Define your cache settings either [programmatically or using an app config file](https://github.com/JesseBuesking/BB.Caching/blob/master/BB.Caching.Tests/Redis/ConnectionGroupTests.cs):
    - if you're using [a config file](https://github.com/JesseBuesking/BB.Caching/blob/master/BB.Caching.Tests/readandwrite.config) ([including analytics tracking](https://github.com/JesseBuesking/BB.Caching/blob/master/BB.Caching.Tests/analytics.config)), call ``Cache.LoadFromConfig(...)``
    - if you're defining the setup programmatically, see [this example](https://github.com/JesseBuesking/BB.Caching/blob/2fb3571f7f6882fd0062220dfafbefd1a24686ab/BB.Caching.Tests/Redis/ConnectionGroupTests.cs#L48-L58)
2. Then prepare the cache by calling ``Cache.Prepare()``.
3. Start using the cache!

[Examples](https://github.com/JesseBuesking/BB.Caching/blob/master/BB.Caching.Tests/Caching/CoreTests.cs)
--------
```
// set a value both in local memory and on redis to make it available to other
// servers
Cache.Set("my-key", "some value", Cache.Store.MemoryAndRedis);

// set a value both in local memory and on redis, giving it an expiration
Cache.Set("my-key", 1234, TimeSpan.FromSeconds(30), Cache.Store.MemoryAndRedis);

// retrieve the data for ``"my-key"``. if the key isn't found in local memory,
// it'll look the key up in redis, and if it's found it'll store it in local
// memory for next time and return the value
Cache.Get<string>("my-key", Cache.Store.MemoryAndRedis);

// deletes the key from redis and all other servers
Cache.BroadcastDelete("my-key");

// track an event with 15 minute precision
BitwiseAnalytics.TrackEvent("video", "watch", 1L, TimePrecision.FifteenMinutes);

// get a key containing the users who watched a video and purchased anything in
// the last 30 minutes...
RedisKey watchVideoAndPurchase = Ops.And(
    new Event("video", "watch", now.AddMinutes(-30), now, TimeInterval.FifteenMinutes),
    new Event("anything", "purchase", now.AddMinutes(-30), now, TimeInterval.FifteenMinutes));

// ... now count the users
long count = BitwiseAnalytics.Count(watchVideoAndPurchase);

// ... now get the ids of the users
List<long> ids = new RedisKeyBitEnumerable(watchVideoAndPurchase).ToList();
```

_Note: each method has an ``Async`` counterpart_

Features
--------
1. A simple-to-use caching interface, with method calls located under [``Cache.``](https://github.com/JesseBuesking/BB.Caching/blob/master/BB.Caching/Caching/Core.cs).
    - NOTE: The methods will serialize and compress objects using [protobuf](https://code.google.com/p/protobuf-net/) and gzip. Complex objects are assumed to be protobuf compatible.
2. More advanced caching and redis use defined under [``Cache.Shared.``](https://github.com/JesseBuesking/BB.Caching/tree/master/BB.Caching/Caching/Shared). These methods utilize the internal consistent hashing logic to distribute your keys across one or more redis nodes.
3. Uses [consistent hashing](http://en.wikipedia.org/wiki/Consistent_hashing) to distribute keys across one or more redis nodes to support easier horizontal scaling.
4. A shared ``Statistics`` implementation to track simple statistics like count, min, max, mean, variance, and standard deviation.
5. [Rate limiting](http://en.wikipedia.org/wiki/Rate_limiting) capabilities.
6. [Bloom filter](http://en.wikipedia.org/wiki/Bloom_filter) support.
7. Consistent hashing & bloom filter both rely on [Murmur hashing](https://github.com/darrenkopp/murmurhash-net).
8. [Simple bitwise analytics](http://blog.getspool.com/2011/11/29/fast-easy-realtime-metrics-using-redis-bitmaps/).

Missing
-------
Redis support for:

1. [Lists](http://redis.io/commands#list)
2. [Sets](http://redis.io/commands#sets)
3. [Sorted Sets](http://redis.io/commands#sorted_set)

You'll find that this is rough around the edges, with unused code scattered throughout. I generally wait until I'm comfortable with a project before pushing it to GitHub but I felt that this could be perpetually in that stage, so I might as well move this to GitHub now since others might find that this scratches an itch.

Issues/Requests
---------------
Feel free to [request features, report any bugs/issues you find](https://github.com/JesseBuesking/BB.Caching/issues), or make a [pull request](https://github.com/JesseBuesking/BB.Caching/pulls). I might not get to it as soon as you'd like (I have other projects  / a job that also require my attention), but I'll do my best to fix/merge what comes in as soon as I can.

Benchmarks
----------
Machine: i7 950 @ 3.07GHz running two instances of redis on windows over ports 6379 and 6380.

_Notes:_
- _consistent hashing adds some minor overhead_
- _the logic to time things kills the pipelining benefits provided by ``StackExchange.Redis``. I see throughput increase between a 6-8x across all the methods when it's actively pipelining requests_

```
Shared Keys
  'ExistsAsync' results summary:
    Successes         [10000]
    Failures          [0] 
    Average Exec Time [0.0079] ms
    Per Second        [126,806]
    Stdev Per Second  [+/-520]
  'ExpireAsync' results summary:
    Successes         [10000]
    Failures          [0] 
    Average Exec Time [0.0086] ms
    Per Second        [115,962]
    Stdev Per Second  [+/-2,567]
  'DeleteAsync' results summary:
    Successes         [10000]
    Failures          [0] 
    Average Exec Time [0.0083] ms
    Per Second        [120,730]
    Stdev Per Second  [+/-980]
  'PersistAsync' results summary:
    Successes         [10000]
    Failures          [0] 
    Average Exec Time [0.0083] ms
    Per Second        [119,973]
    Stdev Per Second  [+/-1,076]
Shared Strings
  'GetExpireAsync' results summary:
    Successes         [10000]
    Failures          [0] 
    Average Exec Time [0.0159] ms
    Per Second        [62,706]
    Stdev Per Second  [+/-3,846]
  'SetAsync' results summary:
    Successes         [10000]
    Failures          [0] 
    Average Exec Time [0.0081] ms
    Per Second        [123,364]
    Stdev Per Second  [+/-2,258]
  'GetAsync' results summary:
    Successes         [10000]
    Failures          [0] 
    Average Exec Time [0.0075] ms
    Per Second        [133,256]
    Stdev Per Second  [+/-323]
Shared Hashes
  'SetAsync' results summary:
    Successes         [10000]
    Failures          [0] 
    Average Exec Time [0.0084] ms
    Per Second        [118,385]
    Stdev Per Second  [+/-1,357]
  'GetAsync' results summary:
    Successes         [10000]
    Failures          [0] 
    Average Exec Time [0.0082] ms
    Per Second        [122,089]
    Stdev Per Second  [+/-779]
Bloom Filter
  'GetAsync' results summary:
    Successes         [10000]
    Failures          [0] 
    Average Exec Time [0.0215] ms
    Per Second        [46,563]
    Stdev Per Second  [+/-3,779]
  'SetAsync' results summary:
    Successes         [10000]
    Failures          [0] 
    Average Exec Time [0.0214] ms
    Per Second        [46,753]
    Stdev Per Second  [+/-2,360]
RateLimiter
  'IncrementAsync' results summary:
    Successes         [10000]
    Failures          [0] 
    Average Exec Time [0.0127] ms
    Per Second        [78,600]
    Stdev Per Second  [+/-2,337]
Statistics
  'SetAsync' results summary:
    Successes         [10000]
    Failures          [0] 
    Average Exec Time [0.0133] ms
    Per Second        [75,228]
    Stdev Per Second  [+/-2,150]
  'GetAsync' results summary:
    Successes         [10000]
    Failures          [0] 
    Average Exec Time [0.0127] ms
    Per Second        [78,780]
    Stdev Per Second  [+/-2,398]
ConsistentHashRing
  'GetNode' results summary:
    Successes         [10000]
    Failures          [0] 
    Average Exec Time [0.0056] ms
    Per Second        [177,961]
    Stdev Per Second  [+/-435]
Murmur3
  'HashSpeed' results summary:
    Successes         [10000]
    Failures          [0] 
    Average Exec Time [0.0029] ms
    Per Second        [347,367]
    Stdev Per Second  [+/-397]
Bitwise Analytics
  'AndOneMonth' results summary:
    Successes         [100]
    Failures          [0] 
    Average Exec Time [92.6215] ms
    Per Second        [11]
    Stdev Per Second  [+/-59]
  'AndOneMonthAsync' results summary:
    Successes         [100]
    Failures          [0] 
    Average Exec Time [19.6385] ms
    Per Second        [51]
    Stdev Per Second  [+/-229]

Memory Speed: (operations per second)
  Set:
    Raw: 500,000.0
    Serialized: 361,445.78 (72.29)%
    Serialized + Compressed: 36,275.7 (7.26)%
  Get:
    Raw: 10,000,000.0
    Serialized: 1,153,846.15 (11.54)%
    Serialized + Compressed: 909,090.91 (9.09)%

Memory Size:
  Raw: 19,670.8KB
  Serialized: 10,025.1KB (50.96%)
  Serialized + Compressed: 1,851.3KB (9.41%)
```
