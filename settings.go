package main

import (
	"encoding/json"
	"io"
	"os"
)

type Settings struct {
	ListenerAddress string    `json:"ListenerAddress"`
	IpRetrieveUrl   string    `json:"IpRetrieveUrl"`
	UserAgent       string    `json:"UserAgent"`
	Accounts        []Account `json:"Accounts"`
}

type Account struct {
	Username        string   `json:"Username"`
	Password        string   `json:"Password"`
	UpdateOnStartup bool     `json:"UpdateOnStartup"`
	UpdateUrls      []string `json:"UpdateUrls"`
}

func LoadSettings() (*Settings, error) {
	settings := new(Settings)
	file, err := os.Open("dyndnsconfig.json")
	if err != nil {
		return settings, err
	} else {
		buffer, err := io.ReadAll(file)
		if err != nil {
			return settings, err
		} else {
			json.Unmarshal(buffer, settings)
			return settings, nil
		}
	}
}
