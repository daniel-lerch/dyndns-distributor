package main

import (
	"encoding/json"
	"fmt"
	"io/ioutil"
	"net"
	"os"

	"github.com/gin-gonic/gin"
)

type Settings struct {
	IpRetrieveUrl     string    `json:"IpRetrieveUrl"`
	IpPollingInterval int       `json:"IpPollingInterval"`
	UserAgent         string    `json:"UserAgent"`
	Accounts          []Account `json:"Accounts"`
}

type Account struct {
	Username   string   `json:"Username"`
	Password   string   `json:"Password"`
	Local      bool     `json:"Local"`
	UpdateUrls []string `json:"UpdateUrls"`
}

func basicAuth(c *gin.Context) {
	username, password, hasAuth := c.Request.BasicAuth()
	if !hasAuth || username != "admin" || password != "password" {
		c.Abort()
		c.Writer.Header().Set("WWW-Authenticate", "Basic realm=restricted")
	}
}

func update(c *gin.Context) {
	ip := net.ParseIP(c.Query("myip"))
	if ip == nil {
		c.String(400, "Please specify your IP address.")
	} else {
		c.Status(200)
	}

}

func load_settings() (Settings, error) {
	var settings Settings
	file, err := os.Open("src/DynDnsDistributor/dyndnsconfig.json")
	if err != nil {
		return settings, err
	} else {
		buffer, err := ioutil.ReadAll(file)
		if err != nil {
			return settings, err
		} else {
			json.Unmarshal(buffer, &settings)
			return settings, nil
		}
	}
}

func main() {
	settings, err := load_settings()
	if err != nil {
		fmt.Println(err)
	} else {
		fmt.Println(settings.IpRetrieveUrl)
		fmt.Println(settings.IpPollingInterval)
		for _, account := range settings.Accounts {
			fmt.Println(account)
		}
		router := gin.Default()
		router.GET("/update", basicAuth, update)

		router.Run("localhost:8080")
	}
}
