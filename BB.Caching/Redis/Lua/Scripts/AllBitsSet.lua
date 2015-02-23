local result = 1
for i = 1, #ARGV, 1 do
    if 0 == redis.call('GETBIT', KEYS[1], ARGV[i]) then
        result = 0
        break
    end
end

return result