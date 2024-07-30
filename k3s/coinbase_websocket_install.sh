#!/bin/bash

# Set variables
NAMESPACE="default"  # Change this if you're using a different namespace
IMAGE_NAME="bsamaha/coinbase-websocket-client"

# Get Kafka password
KAFKA_PASSWORD=$(kubectl get secret kafka-user-passwords --namespace kafka -o jsonpath='{.data.client-passwords}' | base64 -d | cut -d , -f 1)

# Prompt for Coinbase API key and private key
read -p "Enter Coinbase API key: " COINBASE_API_KEY
read -p "Enter Coinbase private key: " COINBASE_PRIVATE_KEY

# Verify that both keys were provided
if [ -z "$COINBASE_API_KEY" ] || [ -z "$COINBASE_PRIVATE_KEY" ]; then
    echo "Error: Both Coinbase API key and private key must be provided."
    exit 1
fi

# Apply the initial secret
echo "Applying initial Coinbase secrets..."
kubectl apply -f k3s/coinbase_websocket_secret.yaml

# Update the secrets
echo "Updating Coinbase secrets..."
kubectl patch secret coinbase-secrets \
  --namespace $NAMESPACE \
  --type='json' \
  -p='[
    {"op": "replace", "path": "/data/COINBASE_API_KEY", "value":"'$(echo -n "$COINBASE_API_KEY" | base64 -w 0)'"},
    {"op": "replace", "path": "/data/COINBASE_PRIVATE_KEY", "value":"'$(echo -n "$COINBASE_PRIVATE_KEY" | base64 -w 0)'"},
    {"op": "replace", "path": "/data/KAFKA_PASSWORD", "value":"'$(echo -n "$KAFKA_PASSWORD" | base64 -w 0)'"}
  ]'

# Apply ConfigMap
echo "Applying Coinbase ConfigMap..."
kubectl apply -f k3s/coinbase_websocket_configmap.yaml

# Apply Deployment
echo "Applying Coinbase Deployment..."
kubectl apply -f k3s/deployment.yaml

# Force the deployment to pull the new image
echo "Forcing deployment to use the latest image..."
kubectl rollout restart deployment/coinbase-websocket-client

echo "Configuration applied successfully!"