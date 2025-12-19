# Wikimedia.Reader

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
