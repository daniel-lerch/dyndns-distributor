package main

import (
	"net"

	"github.com/gin-gonic/gin"
)

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

func main() {
	router := gin.Default()
	router.GET("/update", basicAuth, update)

	router.Run("localhost:8080")
}
