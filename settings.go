package main

import (
	"encoding/json"
	"io/ioutil"
	"os"
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

func LoadSettings() (Settings, error) {
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
