package main

import "github.com/gin-gonic/gin"

type AuthHandler struct {
	settings *Settings
}

func NewAuthHandler(settings *Settings) *AuthHandler {
	return &AuthHandler{settings}
}

func (h *AuthHandler) Handle(c *gin.Context) {
	username, password, _ := c.Request.BasicAuth()

	if h.settings.Username == username && h.settings.Password == password {
		c.Next()
	} else {
		c.Abort()
		c.Writer.Header().Set("WWW-Authenticate", "Basic realm=restricted")
		c.Status(401)
	}
}
