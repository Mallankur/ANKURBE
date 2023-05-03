# wrk load generator

## Prerequisites

1. [wrk2](https://github.com/giltene/wrk2)
2. [lua5.1](https://www.lua.org/ftp/lua-5.1.5.tar.gz)
3. [cjson](https://luarocks.org/modules/openresty/lua-cjson) installed preferably with [luarocks](https://github.com/luarocks/luarocks/wiki/Installation-instructions-for-Unix)
4. Python3.7+
5. Python dependencies installed (`requirements.txt`)

## How to run it

Currently it's semi-automated (ultimately will be consumed by perf test tool).  

1. Navigate to `./scripts` directory
2. Fill parameters in `config.json`
3. Run `extract_data.py`. It should produce two intermediate files: `data.json` and `token.txt`
4. Run `wrk` with host parameter provided. For example:

```bash
wrk -c8 -t2 -d30s -R5 -s ./generate_load.lua --latency https://adform-bloom-runtime.app.d1.adform.zone
```

## Docker [WIP]

I tried to container the environment but ran into issues with resolving Python dependencies.
