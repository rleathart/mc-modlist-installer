package main

import (
	"fmt"
)

// FetchRemoteList commment
func FetchRemoteList() {
	if len(modlists) == 0 && !PathExists(remotesList) {
		fmt.Println("No modlists found.")
		var remoteURL string = GetUserInput("Please specify a remote: ")
		DownloadFileToPath(remoteURL, ".", remotesList)
	}
	if PathExists(remotesList) {
		fmt.Println("Fetching remote files ...")
		ResolveModlistAlways(remotesList, true)
	}
}
