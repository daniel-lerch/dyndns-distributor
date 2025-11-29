FROM golang:1-alpine3.22 AS build-env
WORKDIR /app

# Copy go.mod and go.sum and download as distinct layers
COPY go.mod go.sum ./
RUN go mod download

# Copy everything else and build
COPY . ./
RUN go build -o /app/dyndns-distributor

# Build runtime image
FROM alpine:3.22
WORKDIR /app
ENV GIN_MODE=release
COPY --from=build-env /app/dyndns-distributor .
ENTRYPOINT ["./dyndns-distributor"]
