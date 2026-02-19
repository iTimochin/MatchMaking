#!/bin/sh
echo "Waiting for Kafka to be ready..."
while ! /opt/kafka/bin/kafka-broker-api-versions.sh --bootstrap-server matchmaking.kafka:9093 >/dev/null 2>&1; do sleep 2; done

echo "Kafka is ready, creating topics"
echo "KAFKA_PARTITIONS: $KAFKA_PARTITIONS"

# initialize topics before producer and consumers app container starts
/opt/kafka/bin/kafka-topics.sh --create --bootstrap-server matchmaking.kafka:9093 --topic matchmaking.complete --partitions "$KAFKA_PARTITIONS" --replication-factor 1 || echo "Topic matchmaking.complete already exists"
/opt/kafka/bin/kafka-topics.sh --create --bootstrap-server matchmaking.kafka:9093 --topic matchmaking.request --partitions "$KAFKA_PARTITIONS" --replication-factor 1 || echo "Topic matchmaking.request already exists"
