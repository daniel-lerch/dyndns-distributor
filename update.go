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

	successfulUpdates := 0

	for _, record := range h.settings.Records {

		body, loggingUrl, err := h.client.Update(record, ip)
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

	c.String(200, "Successfully updated %d of %d records", successfulUpdates, len(h.settings.Records))
}
