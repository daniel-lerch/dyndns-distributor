package main

import (
	"fmt"

	"github.com/gin-gonic/gin"
)

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
		router.Use(gin.Logger())

		router.GET("/update", NewAuthHandler(settings).Handle, NewUpdateHandler(settings).Handle)

		router.Run("localhost:8080")
	}
}
