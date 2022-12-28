# DynDNS Distributor #

This application works as a dynamic DNS proxy for your router to update multiple domains.

Most routers only support a single dynamic DNS update URL.
If you want to update multiple hostnames, just configure DynDNS Distributor as update URL in your router and all your configured URLs will be updated immediately.

## Installation ##
> TODO

## Configuration ##
The configuration of update URLs, accounts and credentials is done in the file `dyndnsconfig.json` located in the application root.
Per default, the configuration looks like this:
```json
{
  "ListenerAddress": ":8080",
  "IpRetrieveUrl": "https://api.ipify.org/",
  "UserAgent": "VectorData - DynDNS Distributor - 3.0",
  "Accounts": [
    {
      "Username": "proxyuser",
      "Password": "localnetpw123",
      "UpdateOnStartup": true,
      "UpdateUrls": [
        "https://13135:7sN2KS6L8W@members.feste-ip.net/nic/update?hostname=test.feste-ip.net",
        "https://username:password@dyndns.example.com/update?hostname=mydomain.de&myip=<ipaddr>"
      ]
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

#### `Accounts` ####
An array that contains all account objects.
Each account has fields for `Username`, `Password` and another array `UpdateUrls`.
Setting `Username` and `Password` to `""` disables authentication.
Set `UpdateOnStartup` to `false` for domains which should have a different external IP than the server that runs DynDNS Distributor.

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
- Go 1.19+
- VS Code with Go extension

Run it locally:

```bash
git clone https://github.com/daniel-lerch/dyndns-distributor
cd dyndns-distributor
go run .
```
