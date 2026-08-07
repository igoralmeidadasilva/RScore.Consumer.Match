db-up:
	docker compose -p r-score -f ./docker/docker-compose.database.yaml up -d --build
db-down:
	docker compose -p r-score -f ./docker/docker-compose.database.yaml down

dev-up:
	docker compose -p r-score -f ./docker/docker-compose.yaml up -d --build
dev-down:
	docker compose -p r-score -f ./docker/docker-compose.yaml down