#!/bin/bash
# Create a long-lived bearer token for the admin-user service account
kubectl -n kubernetes-dashboard create token admin-user
