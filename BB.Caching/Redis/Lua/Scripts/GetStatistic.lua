local key = KEYS[1]

return {
    redis.call('HGET', key, '0'),
    redis.call('HGET', key, '1'),
    redis.call('HGET', key, '2'),
    redis.call('HGET', key, '3'),
    redis.call('HGET', key, '4')
}