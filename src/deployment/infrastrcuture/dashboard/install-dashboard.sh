#!/bin/bash
set -e

# 1. Add the Kubernetes Dashboard Helm repo
helm repo add kubernetes-dashboard https://kubernetes.github.io/dashboard/
helm repo update

# 2. Install the dashboard into its own namespace
helm upgrade --install kubernetes-dashboard kubernetes-dashboard/kubernetes-dashboard \
  --create-namespace \
  --namespace kubernetes-dashboard

# 3. Apply the admin ServiceAccount and ClusterRoleBinding
kubectl apply -f dashboard.rbac.yaml

# 4. Print a bearer token for login
echo ""
echo "=== Dashboard bearer token ==="
kubectl -n kubernetes-dashboard create token admin-user

# 5. Start a local proxy to access the dashboard
echo ""
echo "=== Starting kubectl proxy ==="
echo "Open: http://localhost:8001/api/v1/namespaces/kubernetes-dashboard/services/https:kubernetes-dashboard-kong-proxy:443/proxy/"
kubectl proxy
