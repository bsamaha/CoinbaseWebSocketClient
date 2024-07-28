#!/bin/bash

# Set variables
NAMESPACE="default"  # Change this if you're using a different namespace
COINBASE_API_KEY="your-api-key"  # Replace with your actual API key
COINBASE_PRIVATE_KEY="your-private-key"  # Replace with your actual private key

# Get Kafka password
KAFKA_PASSWORD=$(kubectl get secret kafka-user-passwords --namespace kafka -o jsonpath='{.data.client-passwords}' | base64 -d | cut -d , -f 1)

# Create or update the secret
echo "Creating/updating Coinbase secrets..."
kubectl create secret generic coinbase-secrets \
  --namespace $NAMESPACE \
  --from-literal=COINBASE_API_KEY=$COINBASE_API_KEY \
  --from-literal=COINBASE_PRIVATE_KEY=$COINBASE_PRIVATE_KEY \
  --from-literal=KAFKA_PASSWORD=$KAFKA_PASSWORD \
  --dry-run=client -o yaml | kubectl apply -f -

# Apply ConfigMap
echo "Applying Coinbase ConfigMap..."
kubectl apply -f k3s/coinbase_websocket_configmap.yaml

# Apply Deployment
echo "Applying Coinbase Deployment..."
kubectl apply -f k3s/deployment.yaml

echo "Configuration applied successfully!"