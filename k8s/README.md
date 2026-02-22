# Kubernetes manifests for Mango Microservices

## Directory Structure

This directory contains Kubernetes manifests for deploying Mango microservices to a Kubernetes cluster.

### Files

- `00-namespace.yaml` - Namespace definition
- `01-configmap.yaml` - Configuration Map
- `02-secret.yaml` - Secrets
- `03-ingress.yaml` - API Gateway Ingress

### Service-specific manifests

- `services/03-authapi-deployment.yaml`
- `services/04-productapi-deployment.yaml`
- `services/05-cartapi-deployment.yaml`
- `services/06-orderapi-deployment.yaml`
- `services/07-couponapi-deployment.yaml`
- `services/08-rewardapi-deployment.yaml`
- `services/09-emailapi-deployment.yaml`
- `services/10-gateway-deployment.yaml`
- `services/11-web-deployment.yaml`

### Additional manifests

- `05-hpa.yaml` - Horizontal Pod Autoscaler
- `30-network-policy.yaml` - Network Policies
- `40-resourcequota.yaml` - Resource Quotas
- `41-limitrange.yaml` - Limit Ranges
