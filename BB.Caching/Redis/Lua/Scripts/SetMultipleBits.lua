for i = 2, #ARGV, 1 do
    redis.call('SETBIT', KEYS[1], ARGV[i], ARGV[1])
end