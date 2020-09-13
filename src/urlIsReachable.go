package main

import (
	"net/http"
	"time"
)

func urlIsReachable(url string) bool {

	// Default timeout of 1 second
	timeout := time.Duration(1 * time.Second)
	client := http.Client{
		Timeout: timeout,
	}

	_, err := client.Get(url)
	if err != nil {
		return false
	}

	return true
}
