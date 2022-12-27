package main

import (
	"fmt"
	"io/ioutil"
	"net"
	"net/http"
	"strings"

	"github.com/gin-gonic/gin"
)

type UpdateHandler struct {
	settings Settings
}

func NewUpdateHandler(settings Settings) *UpdateHandler {
	return &UpdateHandler{settings: settings}
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

		updateUrl := strings.ReplaceAll(updateUrlTemplate, "<ipaddr>", ip.String())

		request, error := http.NewRequest("GET", updateUrl, nil)
		if error != nil {
			c.Error(error)
			continue
		}

		client := &http.Client{}
		response, error := client.Do(request)
		if error != nil {
			c.Error(error)
			continue
		}

		defer response.Body.Close()
		body, error := ioutil.ReadAll(response.Body)
		if error != nil {
			c.Error(error)
			continue
		}

		successfulUpdates++

		fmt.Print(request.URL)
		fmt.Println(":")
		fmt.Print("\t")
		fmt.Println(string(body))
	}

	c.String(200, "Successfully updated %d of %d domains", successfulUpdates, len(h.settings.Accounts[account.(int)].UpdateUrls))
}
