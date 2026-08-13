db-up:
	docker compose -p r-score-infrastructure -f ./docker/docker-compose.database.yaml up -d --build
db-down:
	docker compose -p r-score-infrastructure -f ./docker/docker-compose.database.yaml down

prod-up:
	docker compose -p r-score -f ./docker/docker-compose.yaml up -d --build
prod-down:
	docker compose -p r-score -f ./docker/docker-compose.yaml down