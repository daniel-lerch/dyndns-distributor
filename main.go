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

	router.Run(settings.ListenerAddress)
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

	if settings.UpdateOnStartup {
		for _, record := range settings.Records {
			body, loggingUrl, err := client.Update(record, ip)

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
