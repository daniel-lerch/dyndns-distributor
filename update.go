package main

import (
	"fmt"
	"net"

	"github.com/gin-gonic/gin"
)

type UpdateHandler struct {
	settings *Settings
	client   *DynClient
}

func NewUpdateHandler(settings *Settings, client *DynClient) *UpdateHandler {
	return &UpdateHandler{settings, client}
}

func (h *UpdateHandler) Handle(c *gin.Context) {
	ip := net.ParseIP(c.Query("myip"))
	if ip == nil {
		c.String(400, "Please specify your IP address.")
		return
	}

	account, exists := c.Get("account")
	if !exists {
		c.String(500, "An unexpected error occured in the authentication middleware")
		return
	}

	successfulUpdates := 0

	for _, updateUrlTemplate := range h.settings.Accounts[account.(int)].UpdateUrls {

		body, loggingUrl, err := h.client.Update(updateUrlTemplate, ip)
		if err != nil {
			c.Error(err)
			continue
		}

		successfulUpdates++

		fmt.Print(loggingUrl)
		fmt.Println(":")
		fmt.Print("\t")
		fmt.Println(string(body))
	}

	c.String(200, "Successfully updated %d of %d domains", successfulUpdates, len(h.settings.Accounts[account.(int)].UpdateUrls))
}
