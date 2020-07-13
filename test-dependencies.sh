#!/bin/bash


export HOST_IP=`ipconfig getifaddr en0`
export PRAVEGA_CONTROLLER=tcp://$HOST_IP:9090
mssql_image=mcr.microsoft.com/mssql/server:2019-GA-ubuntu-16.04


if [ "$1" != "start" -a "$1" != "stop" -a "$1" != "restart" ]; then
  echo Start, stop or restart backends required for automated tests
  echo Usage: test-dependencies.sh { start \| stop \| restart }
fi


if [ "$1" == "stop" -o "$1" == "restart" ]; then
  docker rm -f eventstore
  docker rm -f postgres
  docker rm -f mssql2019
  #docker rm -f azurite
  #rm -fr certs
  docker-compose down 
fi


if [ "$1" == "start" -o "$1" == "restart" ]; then
  docker run --name eventstore -d -p 2113:2113 -p 1113:1113 eventstore/eventstore:release-4.1.0
  docker run --name postgres -dp5432:5432 -e POSTGRES_PASSWORD='postgres' postgres:9.6.10
  docker run --name mssql2019 -e 'ACCEPT_EULA=Y' -e 'MSSQL_SA_PASSWORD=abc123ABC' -p 1433:1433 -d $mssql_image
  # mkdir certs
  #openssl req -x509 -newkey rsa:4096 -keyout certs/key.pem -out certs/cert.pem -days 365 -nodes -subj "/CN=localhost"
# azurite v3 does not support table storage
#  docker run --name azurite -v $(pwd)/certs:/certs -dp 10000:10000 -p 10001:10001 mcr.microsoft.com/azure-storage/azurite azurite --oauth basic --cert /certs/cert.pem --key /certs/key.pem
  docker-compose up -d
fi

