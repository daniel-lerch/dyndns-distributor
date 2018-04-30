# DynDNS Distributor #

This application works a dynamic DNS proxy for your router to update multiple domains.
It is primary considered for private use but you can use this proxy as well for your friends
or in small enterprises when you don't need to change the configuration automatically or often.

## When to use this application? ##

Most routers do not allow to specify multiple dynamic DNS update URLs.
When you want to update multiple hostnames you can only use a slow and inefficient polling solution.
DynDNS distributor catches the update event from your router and immediately updates all configured URLs.

This project is build on ASP.NET Core making it easy to run on every platform with automatic startup,
reverse proxy configuration, etc. However when you don't like ASP.NET Core and don't need Basic Authentication,
you could also take a look at [DNSButler](https://github.com/stahlstift/dnsbutler) which is built on Node.js.

## Installation ##
The required steps for configuring an ASP.NET Core application vary for different platforms and deployment setups.
Precompiled binaries are offered for .NET Core Runtime and can be retrieved [here](https://github.com/daniel-lerch/dyndns-distributor/releases).
For the further setup refer to [Microsoft's deployment guides](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/).

## Configuration ##
The configuration of update URLs, accounts and credentials is done in the file `dyndnsconfig.json` located in the application root.
Per default, the configuration looks like this:
```json
{
    "IpRetrieveUrl": "https://api.ipify.org/",
    "IpPollingInterval": null,
    "LocalAccounts": [ "proxyuser" ],
    "Accounts": [
        {
            "Hostname": "update.any",
            "Username": "proxyuser",
            "Password": "localnetpw123",
            "UpdateUrls": [
                "https://example.de:password123@dyndns.strato.com/nic/update?hostname=example.de&myip=<ipaddr>",
                "https://example.de:password123@dyndns.strato.com/nic/update?hostname=sub.example.de&myip=<ipaddr>"
            ]
        }
    ]
}
```
#### `IpRetrieveUrl` ####
An URL to retrieve the extern IP address for local accounts.

#### `IpPollingInterval` ####
An intervall to look for extern IP updates using the `IpRetrieveUrl`.
This parameter is specified in milliseconds which means that `900000` equals one refresh each 15 minutes.
Because this application is designed as a proxy, polling is disabled by default by setting `IpPollingInverval` to `null`.

#### `LocalAccounts` ####
An array that lists the usernames of all local accounts.
Local accounts are located in the local network and have the same IP address as the result of `IpRetrieveUrl`.
Every local account is updated automatically on startup and with each polling.

#### `Accounts` ####
An array that contains all account objects.
Each account has fields for `hostname`, `username`, `password` and another array `UpdateUrls`.
If you don't want to specify a hostname, you can set this value to `null`.
Setting `username` and `password` to `null` disables authentication.

##### `UpdateUrl` #####
This is the URL to push an IP address change.
Credentials for authentication should be included in the URL.
When no credentials are found, basic authentication will not be used for the request.

## AVM Fritz!Box ##
In combination with a Fritz!Box as router you have to configure the following update URL:
```
http://<localipaddr>:<port>/update?hostname=<domain>&myip=<ipaddr>
```
Example:
```
http://192.168.178.46:8080/update?hostname=<domain>&myip=<ipaddr>
```
In the domain, username and password field, please fill in the same values as in your config file.

## Building from source ##

### Requirements ###
- Git
- .NET Core SDK
### For .NET Runtime ###
```bash
git clone https://github.com/daniel-lerch/dyndns-distributor.git
cd src/DynDnsDistributor
dotnet publish -c Release
```
### For Raspberry Pi ###
You need to run this on a machine with .NET Core SDK which not available for ARM at the moment.
```bash
git clone https://github.com/daniel-lerch/dyndns-distributor.git
cd src/DynDnsDistributor
dotnet publish -c Release -r linux-arm
```