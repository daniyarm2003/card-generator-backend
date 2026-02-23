#!/usr/bin/env bash

ENV_FILE=".env"

if [ ! -f "$ENV_FILE" ]; then
  echo "Error: $ENV_FILE not found!"
  exit 1
fi

cat "$ENV_FILE" | base64 -w 0