package main

import (
	"errors"
	"io"
	"net"
	"net/http"
	"net/url"
	"strings"
)

type DynClient struct {
	client    *http.Client
	userAgent string
}

func NewDynClient(userAgent string) *DynClient {
	return &DynClient{&http.Client{}, userAgent}
}

func (c *DynClient) Update(record Record, ip net.IP) (string, string, error) {

	record.UpdateUrl = strings.ReplaceAll(record.UpdateUrl, "<ipaddr>", ip.String())
	record.Body = strings.ReplaceAll(record.Body, "<ipaddr>", ip.String())

	parsedUrl, err := url.Parse(record.UpdateUrl)
	if err != nil {
		return "", "", err
	}

	parsedUrl.User = url.UserPassword(parsedUrl.User.Username(), "***")
	urlWithoutPassword := strings.ReplaceAll(parsedUrl.String(), "%2A%2A%2A", "***")

	method := record.Method
	if method == "" {
		method = "GET"
	}

	request, err := http.NewRequest(method, record.UpdateUrl, strings.NewReader(record.Body))
	if err != nil {
		return "", urlWithoutPassword, err
	}

	request.Header.Set("User-Agent", c.userAgent)
	for _, header := range record.Headers {
		name, value, found := strings.Cut(header, ": ")
		if !found {
			return "", urlWithoutPassword, errors.New("Invalid header: " + header)
		}
		request.Header.Set(name, value)
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

	if response.StatusCode < 200 || response.StatusCode >= 300 {
		return string(body), urlWithoutPassword, errors.New(method + " " + urlWithoutPassword + " returned " + response.Status + ": " + string(body))
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
