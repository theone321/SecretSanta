building:
docker-compose.exe build

running in foreground:
docker-compose up

running in background:
docker-compose up -d

stop a background container:
docker stop postgresql_santapostgres_1

remove the docker container so you can start fresh:
docker rm postgresql_santapostgres_1

view all docker containers:
docker ps -a