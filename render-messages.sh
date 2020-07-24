#!/usr/bin/env bash

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
cd ${DIR}

dotnet run --project src/CosmicMachine render --with-wav src/CosmicMachine/message.txt message