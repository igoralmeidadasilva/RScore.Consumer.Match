db-up:
	docker compose -p r-score -f ./docker/docker-compose.database.yaml up -d --build
db-down:
	docker compose -p r-score -f ./docker/docker-compose.database.yaml down