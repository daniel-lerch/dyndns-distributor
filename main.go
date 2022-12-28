package main

import (
	"fmt"

	"github.com/gin-gonic/gin"
)

func main() {
	settings, err := LoadSettings()
	if err != nil {
		fmt.Println(err)
		return
	}

	client := NewDynClient()

	UpdateOnStartup(settings, client)

	router := gin.Default()
	router.Use(gin.Logger())

	router.GET("/update", NewAuthHandler(settings).Handle, NewUpdateHandler(settings, client).Handle)

	router.Run("localhost:8080")
}

func UpdateOnStartup(settings *Settings, client *DynClient) {
	if len(settings.IpRetrieveUrl) == 0 {
		return
	}

	ip, err := client.GetExternalIp(settings.IpRetrieveUrl)
	if err != nil {
		fmt.Println(err)
		return
	}

	for _, account := range settings.Accounts {
		if account.UpdateOnStartup {
			for _, updateUrlTemplate := range account.UpdateUrls {
				body, loggingUrl, err := client.Update(updateUrlTemplate, ip)

				fmt.Print(loggingUrl)
				fmt.Println(":")
				fmt.Print("\t")

				if err == nil {
					fmt.Println(body)
				} else {
					fmt.Println(err)
				}
			}
		}
	}
}
