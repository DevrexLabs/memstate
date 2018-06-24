# Start or stop  eventstore and postgres docker containers required by tests

if [ "$1" == "stop" ]; then
  docker rm -f eventstore
  docker rm -f postgres
elif [ "$1" == "start" ]; then
  docker run --name eventstore -d -p 2113:2113 -p 1113:1113 eventstore/eventstore:release-4.1.0
  docker run --name postgres -dp5432:5432 -e POSTGRES_PASSWORD='postgres' postgres
else
  echo Syntax: . test-dependencies.sh start\|stop
fi
