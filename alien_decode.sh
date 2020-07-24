#!/usr/bin/env bash

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
cd ${DIR}

if test -f src/Tools/bin/Debug/netcoreapp3.1/Tools.dll; then
    dotnet run --project src/Tools --no-build --decode "$@"
else
    dotnet run --project src/Tools --decode "$@"
fi
