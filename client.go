package main

import (
	"io"
	"net"
	"net/http"
	"net/url"
	"strings"
)

type DynClient struct {
	client *http.Client
}

func NewDynClient() *DynClient {
	return &DynClient{&http.Client{}}
}

func (c *DynClient) Update(urlTemplate string, ip net.IP) (string, string, error) {

	updateUrl := strings.ReplaceAll(urlTemplate, "<ipaddr>", ip.String())

	parsedUrl, err := url.Parse(updateUrl)
	if err != nil {
		return "", "", err
	}

	parsedUrl.User = url.UserPassword(parsedUrl.User.Username(), "***")
	urlWithoutPassword := strings.ReplaceAll(parsedUrl.String(), "%2A%2A%2A", "***")

	request, err := http.NewRequest("GET", updateUrl, nil)
	if err != nil {
		return "", urlWithoutPassword, err
	}

	response, err := c.client.Do(request)
	if err != nil {
		return "", urlWithoutPassword, err
	}

	defer response.Body.Close()
	body, err := io.ReadAll(response.Body)
	if err != nil {
		return "", urlWithoutPassword, err
	}

	return string(body), urlWithoutPassword, nil
}

func (c *DynClient) GetExternalIp(ipRetrieveUrl string) (net.IP, error) {
	request, err := http.NewRequest("GET", ipRetrieveUrl, nil)
	if err != nil {
		return nil, err
	}

	response, err := c.client.Do(request)
	if err != nil {
		return nil, err
	}

	defer response.Body.Close()
	body, err := io.ReadAll(response.Body)
	if err != nil {
		return nil, err
	}

	return net.ParseIP(string(body)), nil
}
