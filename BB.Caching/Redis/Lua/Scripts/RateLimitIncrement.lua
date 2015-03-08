local key = KEYS[1]
local time_in_ms = tonumber(ARGV[1])
local span_ms = tonumber(ARGV[2])
local bucket_ms = tonumber(ARGV[3])
local incrby = tonumber(ARGV[4])
local throttle = tonumber(ARGV[5])

local current_bucket = math.floor((time_in_ms % span_ms) / bucket_ms)
local current_count = incrby

local last_bucket = tonumber(redis.call('HGET', key, 'L'))
local not_same = last_bucket ~= current_bucket

if nil ~= last_bucket then
    local bucket_count = span_ms / bucket_ms

    -- clear unused buckets
    if not_same then
        local j = current_bucket
        while j ~= last_bucket do
            redis.call('HDEL', key, j)
            j = ((j - 1) % bucket_count)
        end
    end

    -- generate an array containing all of the possible fields
    local getting = {}
    local bc = bucket_count + 1
    for i = 1, bc, 1 do
        getting[i] = i - 1
    end

    -- get all of the available values at once
    local all = redis.call('HMGET', key, unpack(getting))
    for k, v in pairs(all) do
        current_count = current_count + (tonumber(v) or 0)
    end

    -- stop here if the throttle value will be surpassed on this request
    if throttle < current_count then
        return throttle - current_count
    end
end

-- only set the 'current bucket' if we're actually incrementing it's value
if not_same then
    redis.call('HSET', key, 'L', current_bucket)
end

redis.call('HINCRBY', key, current_bucket, incrby)
redis.call('PEXPIRE', key, span_ms)

return throttle - current_count
