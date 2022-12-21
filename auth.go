package main

import "github.com/gin-gonic/gin"

type AuthHandler struct {
	settings Settings
}

func NewAuthHandler(settings Settings) *AuthHandler {
	return &AuthHandler{settings: settings}
}

func (h *AuthHandler) Handle(c *gin.Context) {
	username, password, _ := c.Request.BasicAuth()

	for index, account := range h.settings.Accounts {
		if account.Username == username && account.Password == password {
			c.Set("account", index)
			c.Next()
			return
		}
	}

	c.Abort()
	c.Writer.Header().Set("WWW-Authenticate", "Basic realm=restricted")
	c.Status(401)
}
