local cjson = require "cjson"

local host
local json = {}
local path = "/v1/runtime/subject-evaluation"
local uri

local read_file_content = function(fileName)
    local file = io.open(fileName, "r")
    local content = file:read "*a"
    file:close()
    return content
end

init = function(args)
    local token = read_file_content("token.txt")
    local content = read_file_content("data.json")
    json = cjson.decode(content)
    math.randomseed(os.time())

    host = args[0]
    uri = host .. path
    local authorization = "Bearer " .. token
    wrk.headers["authorization"] = authorization
    wrk.headers["Content-Type"] = "application/json"
end

local get_body = function()
    local record = json[math.random(1, #json)]
    local body = {
        subjectId = record["subjectId"],
        tenantIds = {record["tenantId"]},
        forceReturnTenants = true
    }
    return cjson.encode(body)
end

request = function()
    local body = get_body()
    return wrk.format("POST", uri, wrk.headers, body)
end

-- response = function(status, headers, body) print(status) end
