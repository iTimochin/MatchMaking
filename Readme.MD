# Matchmaking

## Development notes

* .NET SDK version: 9
* Docker version: 20.10.11
* Docker compose version: v2.2.1 (later version does not supports replicas)

## Run and execute
For tear up application infrastructure run the following command.

Run first time for building apps or each time to rebuild apps:
`docker compose -f docker-compose.yaml up --build`

Run each time to tear up all of teh apps:
`docker compose -f docker-compose.yaml up`

Run to tear down all of the apps:
`docker compose -f docker-compose.yaml down`

After running in container web api accessible with Open API documentation:
`http://localhost:5232/scalar/v1`

## Curl commands (against dockerized app):

1) Request user to join match, note that **userId** must be unique.
```
curl http://localhost:5232/Match \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
  "userId": "fdaa9077-2223-4422-9f1a-2b09f6de4292"
}'
```
2) Get match by userId, note that request has throttling for 100 ms per request for a same user.
```
curl 'http://localhost:5232/Match/fdaa9077-2223-4422-9f1a-2b09f6de4292'
```