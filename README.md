BB.Caching
==========

BB.Caching is a library for (ASP).NET developers with the goal of making caching as simple as possible. With out-of-the-box support for [Redis](http://redis.io), you can be up and running with a fast, distributed cache in less time than it would take for you to write everything from scratch. (I'm purposely setting your expectations as low as possible so that when everything _just works_ you'll end up happier :D)

Note: This code is not production ready, but has a lot of [tests](https://github.com/JesseBuesking/BB.Caching/blob/master/BB.Caching.Tests/CacheTests.cs) that cover many cases.

What can it be used for?
------------------------
1. Smart caching (in-memory and/or distributed)
2. Rate limiting
3. Bloom filters
4. Tracking statistics across servers

Documentation
-------------
Check out the [tests](https://github.com/JesseBuesking/BB.Caching/tree/master/BB.Caching.Tests).

Features
--------
1. An in-memory L1 cache (```Cache.Memory.TryGetString...```) and redis-based L2 cache (```Cache.Shared.Strings.GetString...```).
2. Support for serialization and/or smart compression (gzip or raw depending on compression size) of data being cached, [similar to what is used by StackOverflow (or at least similar to what is stated in this answer)](http://meta.stackoverflow.com/a/69172). (See ```Cache.Memory.SetCompress...``` and ```Cache.Memory.SetCompact...```).
3. Automatic protobufing of objects when serialization is requested. (Note: protobuf indices are automatically stored in Redis and synchronized across servers, but **use at your own risk**!; aka this is a fragile approach that might not work in your case) (See ```ProtoBufSerializer.Instance.Serialize...```)
4. Uses [consistent hashing](http://en.wikipedia.org/wiki/Consistent_hashing) for redis instances that are added (to support easier horizontal scaling).
5. Special configuration store for automatic syncing of settings across servers (See ```Cache.Config...```)
6. A shared ```Statistic``` object that tracks basic statistics for an object (count, min, max, mean, variance, standard deviation). (See ```Cache.Statistic...```).
7. [Rate limiting](http://en.wikipedia.org/wiki/Rate_limiting) capabilities. (See ```Cache.RateLimiter...```)
8. [Bloom filter](http://en.wikipedia.org/wiki/Bloom_filter) support. (See ```Cache.BloomFilter...```)
9. Consistent hashing & bloom filter both rely on [Murmur hashing](https://github.com/JesseBuesking/murmurhash-net).

Missing
-------
Redis support for:

1. [Lists](http://redis.io/commands#list)
2. [Sets](http://redis.io/commands#sets)
3. [Sorted Sets](http://redis.io/commands#sorted_set)

You'll find that this is rough around the edges, with unused code scattered throughout. I generally wait until I'm comfortable with a project before pushing it to GitHub but I felt that this could be perpetually in that stage, so I might as well move this to GitHub now since others might find that this scratches an itch.

Issues/Requests
---------------
Feel free to [report any bugs/issues](https://github.com/JesseBuesking/BB.Caching/issues) or make a [pull request](https://github.com/JesseBuesking/BB.Caching/pulls). I might not get to it as soon as you'd like (I have other projects  / a job that also require my attention), but I'll do my best to fix/merge what comes in as soon as I can.

Benchmarks
----------
Machine: i7 950 @ 3.07GHz (with redis in an Ubuntu Server 12.04 vm in virtual box on the same machine)

(Keep in mind that there is overhead with the consistent hashing logic; without it I see _~50k simple string gets/s_)

- ~40k simple string gets/s
- ~10k rate limit requests/s
- ~9k bloom filter sets/s
- ~23k bloom filter gets/s
- ~10k statistics updates/s