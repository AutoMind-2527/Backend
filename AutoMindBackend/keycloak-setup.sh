#!/bin/bash

# Warte auf Keycloak
echo "Waiting for Keycloak to start..."
sleep 30

# Get access token
TOKEN=$(curl -s -X POST \
  http://localhost:8080/realms/master/protocol/openid-connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "username=admin&password=admin&grant_type=password&client_id=admin-cli" | jq -r '.access_token')

# Realm erstellen
curl -s -X POST \
  http://localhost:8080/admin/realms \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "realm": "automind-realm",
    "enabled": true,
    "displayName": "AutoMind Realm"
  }'

# Client erstellen
curl -s -X POST \
  http://localhost:8080/admin/realms/automind-realm/clients \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "clientId": "automind-backend",
    "enabled": true,
    "publicClient": false,
    "standardFlowEnabled": true,
    "implicitFlowEnabled": false,
    "directAccessGrantsEnabled": true,
    "serviceAccountsEnabled": true,
    "authorizationServicesEnabled": true
  }'

echo "Keycloak setup completed!"