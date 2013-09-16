local key = KEYS[1]
local value = ARGV[1]

local number_of_values = redis.call('HINCRBY', key, '0', 1)
local sum_of_values = redis.call('HINCRBYFLOAT', key, '1', value)
local sum_of_values_squared = redis.call('HINCRBYFLOAT', key, '2', value*value)

local minimum = redis.call('HGET', key, '3')
if nil == tonumber(minimum) then
    minimum = value
else
    minimum = math.min(minimum, value)
end
redis.call('HSET', key, '3', minimum)

local maximum = redis.call('HGET', key, '4')
if nil == tonumber(maximum) then
    maximum = value
else
    maximum = math.max(maximum, value)
end
redis.call('HSET', key, '4', maximum)