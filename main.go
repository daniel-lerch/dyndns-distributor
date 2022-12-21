package main

import (
	"fmt"
	"net"

	"github.com/gin-gonic/gin"
)

func update(c *gin.Context) {
	ip := net.ParseIP(c.Query("myip"))
	if ip == nil {
		c.String(400, "Please specify your IP address.")
	} else {
		c.Status(200)
	}

}

func main() {
	settings, err := LoadSettings()
	if err != nil {
		fmt.Println(err)
	} else {
		fmt.Println(settings.IpRetrieveUrl)
		fmt.Println(settings.IpPollingInterval)
		for _, account := range settings.Accounts {
			fmt.Println(account)
		}
		router := gin.Default()
		router.GET("/update", NewAuthHandler(settings).Handle, update)

		router.Run("localhost:8080")
	}
}
