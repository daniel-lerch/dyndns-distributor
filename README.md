# DynDNS Distributor #

[![](https://img.shields.io/docker/pulls/daniellerch/dyndns-distributor.svg)](https://hub.docker.com/r/daniellerch/dyndns-distributor)
[![](https://img.shields.io/docker/image-size/daniellerch/dyndns-distributor/latest.svg)](https://hub.docker.com/r/daniellerch/dyndns-distributor)

A dynamic DNS proxy for your router to update multiple domains.

Most routers only support a single dynamic DNS update URL.
If you want to update multiple hostnames, just configure DynDNS Distributor as update URL in your router and all your configured URLs will be updated immediately.

## Installation ##

Installation is easiest with Docker Compose.
Create a `compose.yaml` file with the following content and move on to configuration (next heading) before start. 

```yaml
services:
  app:
    image: daniellerch/dyndns-distributor:4
    volumes:
      - ./dyndnsconfig.json:/app/dyndnsconfig.json
    ports:
      - "8080:8080"
```

Windows is not supported anymore but if you want me to provide Windows builds, just open an issue.

## Configuration ##
The configuration of update URLs, accounts and credentials is done in the file `dyndnsconfig.json` located in the application root.
Per default, the configuration looks like this:
```json
{
  "ListenerAddress": "localhost:8080",
  "IpRetrieveUrl": "https://api.ipify.org/",
  "UserAgent": "DynDNS Distributor - 4.0",
  "UpdateOnStartup": true,
  "Username": "proxyuser",
  "Password": "localnetpw123",
  "Records": [
    {
      "UpdateUrl": "https://13135:7sN2KS6L8W@members.feste-ip.net/nic/update?hostname=test.feste-ip.net"
    },
    {
      "UpdateUrl": "https://username:password@dyndns.example.com/update?hostname=mydomain.de&myip=<ipaddr>"
    },
    {
      "UpdateUrl": "https://api.cloudflare.com/client/v4/zones/{zone_id}/dns_records/{dns_record_id}",
      "Method": "PATCH",
      "Headers": [
        "Authorization: Bearer {api_token}"
      ],
      "Body": "{\"content\":\"<ipaddr>\"}"
    }
  ]
}
```

#### `ListenerAddress` ####
The address for DynDNS Distributor to listen for HTTP requests from your router.
Use `:8080` to listen on port 8080 on all network interfaces.

#### `IpRetrieveUrl` ####
An URL to retrieve the extern IP address for local accounts.

#### `UserAgent` ####
The user agent that will be used for DNS updates.
Some providers, like strato.de or dyn.com require a specific user agent.

#### `Username` & `Password` ####
Credentials for basic authentication. Setting `Username` and `Password` to `""` disables authentication.

> ⚠️ Note that an attacker in your local network can still capture packets sent from your router to the device running DynDNS Distributor and thereby gain access to change your DNS records to arbitrary IP addresses, generate valid TLS certificates, and so on. Basic authentication just makes it harder because the attacker must wait for some hours or days until an IP address change happens.

#### `UpdateOnStartup` ####
If set to `true`, DynDNS Distributor will query `IpRetrieveUrl` on startup and update all records according to its value.

#### `Records` ####
An array of records to update which must at least have a `UpdateUrl`. More sophisticated requests as required for Cloudflare are supported since DynDNS Distributor 4, too.

## AVM Fritz!Box ##
If you use a Fritz!Box as router, you have to configure the following update URL:
```
http://<localipaddr>:<port>/update?hostname=<domain>&myip=<ipaddr>
```
Do not replace `<domain>` or `<ipaddr>` as this will be done by the router.

Example:
```
http://192.168.178.46:8080/update?hostname=<domain>&myip=<ipaddr>
```
In the username and password field, please fill in the same values as in your config file.

## Development ##

Recommended setup:
- Git
- Go 1.23+
- VS Code with Go extension

Run it locally:

```bash
git clone https://github.com/daniel-lerch/dyndns-distributor
cd dyndns-distributor
go run .
```
