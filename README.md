# Wikimedia.Reader

A demo .NET project that demonstrates how to stream real-time data from the Wikimedia EventStream into Apache Kafka, and then consume that data to index it into OpenSearch.

This project is built as part of a Kafka learning exercise and serves as a practical example of integrating:

- **Server-Sent Events (SSE) source** → Wikimedia realtime stream
- **Apache Kafka producer** → pushing events into Kafka topics
- **Apache Kafka consumer** → reading events from Kafka
- **OpenSearch sink** → indexing consumed events into OpenSearch

---

## 🚀 Architecture Overview

Wikimedia SSE Stream
│
▼
.NET Wikipedia SSE Client
│
▼
Kafka Producer (writes to topic)
│
▼
Apache Kafka (Topic: wikimedia_recentchange)
│
▼
Kafka Consumer (Background Worker)
│
▼
OpenSearch (Indexed documents)

---

## 📌 Main Components

### 📥 Wikimedia SSE Client

A .NET HTTP SSE client that connects to the Wikimedia realtime event stream (https://stream.wikimedia.org/v2/stream/recentchange) and emits JSON events.

### 📤 Kafka Producer

Listens to SSE events and produces them into a Kafka topic (`wikimedia_recentchange`) using Confluent.Kafka.

Configured as a singleton producer with:

- Idempotence
- Acks
- Compression
- Batching
- Configurable via appsettings.json

### 📥 Kafka Consumer (Background Worker)

A .NET `BackgroundService` that:

- Subscribes to the Kafka topic
- Polls messages continuously
- Commits offsets safely
- Sends consumed records to OpenSearch

Uses `PartitionAssignmentStrategy.CooperativeSticky` for efficient rebalancing.

### 🔎 OpenSearch Sink

Consumed events are persisted to OpenSearch for indexing and search. This provides a way to explore and visualize realtime Wikimedia event data.

---

## 🧰 Tech Stack

| Component            | Technology                   |
| -------------------- | ---------------------------- |
| Real-time stream     | SSE from Wikimedia           |
| Producer/Consumer    | .NET 10 / Confluent.Kafka    |
| Message Broker       | Apache Kafka                 |
| Search & Analytics   | OpenSearch                   |
| Development UI       | Conduktor Console            |
| Dependency Injection | Microsoft.Extensions.Hosting |
| Logging              | Microsoft.Extensions.Logging |

---

## 📦 Project Structure

Wikimedia.Reader/
├── Wikimedia.Common/ # Shared config (KafkaOptions)
├── Wikimedia.Producer/ # Producer client & background worker
├── Wikimedia.Consumer/ # Consumer worker and OpenSearch logic
├── appsettings.json # Kafka + OpenSearch config
└── README.md

---

## 🐳 Docker Setup

Run all required services locally using Docker Compose.

Example services included:

```yaml
services:
  postgresql:
    image: postgres:14
    # ...
  conduktor-console:
    image: conduktor/conduktor-console
    # ...
  kafka1:
    image: confluentinc/cp-kafka:8.0.0
    # ...
  opensearch:
    image: opensearchproject/opensearch:2.12.0
    environment:
      - discovery.type=single-node
      - plugins.security.disabled=true
      - OPENSEARCH_INITIAL_ADMIN_PASSWORD=OS_Admin123!
    ports:
      - "9200:9200"
  opensearch-dashboards:
    image: opensearchproject/opensearch-dashboards:2.12.0
    ports:
      - "5601:5601"
    environment:
      OPENSEARCH_HOSTS: '["http://opensearch:9200"]'
```

Adding container in Docker

- Kafka will be available at `$DOCKER_HOST_IP:9092`
- Conduktor will be available at : `$DOCKER_HOST_IP:8080`
- OpenSearch will be available at : `$DOCKER_HOST_IP:9200`
- OpenSearch Dashboard will be available at : `$DOCKER_HOST_IP:5601`

Run with:

```
docker compose -f conduktor-kafka-single.yml up
docker compose -f conduktor-kafka-single.yml down
```
