# ingress-mqtt
An ingress service that connects to MQTT endpoints and pulls data into the platform

# Building and tagging with Docker

`docker build -f Dockerfile -t diwise/ingress-mqtt:latest .`

# Running locally with Docker

`docker run -it -e MQTT_HOST='<insert MQTT host here>' diwise/ingress-mqtt`
