COMPOSE_PROJECT_NAME := workforcehub
COMPOSE := docker compose -p $(COMPOSE_PROJECT_NAME) -f docker-compose.yml
CURRENT_DIR_PROJECT := $(shell basename "$(CURDIR)" | tr '[:upper:]' '[:lower:]' | sed 's/[^a-z0-9]/-/g')
CONTAINERS := workforcehub-gateway account-service-command account-service-query profile-service-command profile-service-query time-service-command time-service-query evolution-service-command evolution-service-query postgres_write mongo_read kafka-workforce adminer mongo_express kafka-ui-workforce jaeger
POSTGRES_CONTAINER := postgres_write
POSTGRES_WAIT_RETRIES ?= 30
POSTGRES_WAIT_SECONDS ?= 2

.PHONY: up up-min up-fresh down kafka jaeger logs clean init-dbs wait-postgres reset reset-dbs build-gateway rebuild-gateway

up:
	@echo " Starting all services (full)..."
	$(COMPOSE) --profile full up -d
	@echo " Ensuring per-service write databases exist..."
	$(MAKE) init-dbs

up-min:
	@echo " Starting minimal stack (postgres, mongo, kafka, admin tools, gateway)..."
	$(COMPOSE) --profile minimal up -d
	@echo " Ensuring per-service write databases exist..."
	$(MAKE) init-dbs

up-fresh: rebuild-gateway
	@echo " Starting full stack with a freshly rebuilt gateway..."
	$(COMPOSE) --profile full up -d
	@echo " Ensuring per-service write databases exist..."
	$(MAKE) init-dbs

down:
	@echo " Stopping containers and removing the stack..."
	-$(COMPOSE) down --remove-orphans
	-docker compose -p $(CURRENT_DIR_PROJECT) -f docker-compose.yml down --remove-orphans
	-docker compose -f docker-compose.yml down --remove-orphans
	-docker rm -f $(CONTAINERS) 2>/dev/null || true
	-docker network rm $(COMPOSE_PROJECT_NAME)_default $(CURRENT_DIR_PROJECT)_default hrmapp_default 2>/dev/null || true

kafka:
	@echo " Starting Kafka only..."
	$(COMPOSE) up -d kafka

jaeger:
	@echo " Starting Jaeger only..."
	$(COMPOSE) up -d jaeger

logs:
	@$(COMPOSE) logs -f

clean:
	@echo " Removing all volumes..."
	-$(COMPOSE) down -v --remove-orphans
	-docker compose -p $(CURRENT_DIR_PROJECT) -f docker-compose.yml down -v --remove-orphans
	-docker compose -f docker-compose.yml down -v --remove-orphans
	-docker rm -f $(CONTAINERS) 2>/dev/null || true
	-docker volume rm $(COMPOSE_PROJECT_NAME)_pg_write_data $(COMPOSE_PROJECT_NAME)_mongo_data $(COMPOSE_PROJECT_NAME)_kafka_data $(COMPOSE_PROJECT_NAME)_media_uploads $(CURRENT_DIR_PROJECT)_pg_write_data $(CURRENT_DIR_PROJECT)_mongo_data $(CURRENT_DIR_PROJECT)_kafka_data $(CURRENT_DIR_PROJECT)_media_uploads hrmapp_pg_write_data hrmapp_mongo_data hrmapp_kafka_data hrmapp_media_uploads 2>/dev/null || true
	-docker network rm $(COMPOSE_PROJECT_NAME)_default $(CURRENT_DIR_PROJECT)_default hrmapp_default 2>/dev/null || true

init-dbs:
	@$(MAKE) wait-postgres
	@echo " Ensuring account/profile/time/evolution databases exist..."
	@for db in account_write profile_write time_write evolution_write; do \
		if ! docker exec $(POSTGRES_CONTAINER) psql -U admin -d postgres -tAc "SELECT 1 FROM pg_database WHERE datname = '$$db'" | grep -q 1; then \
			echo " Creating $$db..."; \
			docker exec $(POSTGRES_CONTAINER) psql -U admin -d postgres -c "CREATE DATABASE $$db"; \
		fi; \
	done

wait-postgres:
	@echo " Waiting for $(POSTGRES_CONTAINER) to accept connections..."
	@attempt=1; \
	until docker exec $(POSTGRES_CONTAINER) pg_isready -U admin -d postgres >/dev/null 2>&1; do \
		if [ $$attempt -ge $(POSTGRES_WAIT_RETRIES) ]; then \
			echo " Postgres did not become ready in time."; \
			exit 1; \
		fi; \
		echo " Postgres not ready yet ($$attempt/$(POSTGRES_WAIT_RETRIES)); retrying in $(POSTGRES_WAIT_SECONDS)s..."; \
		attempt=$$((attempt + 1)); \
		sleep $(POSTGRES_WAIT_SECONDS); \
	done
	@echo " Postgres is ready."

reset-dbs:
	@echo " Dropping and recreating account/profile/time/evolution databases..."
	@for db in account_write profile_write time_write evolution_write; do \
		echo " Resetting $$db..."; \
		docker exec postgres_write psql -U admin -d postgres -c "DROP DATABASE IF EXISTS $$db WITH (FORCE);"; \
		docker exec postgres_write psql -U admin -d postgres -c "CREATE DATABASE $$db;"; \
	done

build-gateway:
	@echo " Building workforcehub-gateway image..."
	$(COMPOSE) --profile full build workforcehub-gateway

rebuild-gateway:
	@echo " Rebuilding and recreating workforcehub-gateway..."
	$(COMPOSE) --profile full build --no-cache workforcehub-gateway
	$(COMPOSE) --profile full up -d --force-recreate workforcehub-gateway

reset:
	@echo " Recreating containers and volumes from scratch..."
	$(MAKE) clean
	$(COMPOSE) --profile full up -d
	$(MAKE) init-dbs
