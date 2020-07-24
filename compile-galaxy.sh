#!/usr/bin/env bash

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
cd ${DIR}

dotnet run --project src/CosmicMachine compile --change-names galaxy/translation.txt galaxy/galaxy.txt
dotnet run --project src/CosmicMachine compile --with-comments --with-logging --change-names galaxy/translation.txt galaxy/galaxy-debug.txt
