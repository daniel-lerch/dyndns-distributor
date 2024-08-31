package main

import (
	"encoding/json"
	"io"
	"os"
)

type Settings struct {
	ListenerAddress string   `json:"ListenerAddress"`
	IpRetrieveUrl   string   `json:"IpRetrieveUrl"`
	UserAgent       string   `json:"UserAgent"`
	UpdateOnStartup bool     `json:"UpdateOnStartup"`
	Username        string   `json:"Username"`
	Password        string   `json:"Password"`
	Records         []Record `json:"Records"`
}

type Record struct {
	UpdateUrl string   `json:"UpdateUrl"`
	Method    string   `json:"Method"`
	Headers   []string `json:"Headers"`
	Body      string   `json:"Body"`
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
