package main

import (
	"errors"
	"fmt"
	"io"
	"net/http"
	"net/url"
	"os"
	"path"
	"path/filepath"
)

// DownloadFile downloads a file over http to a specified directory
// with an automatic filename.
func DownloadFile(URL string, DestDir string) {
	Filename, _ := url.PathUnescape(path.Base(URL))
	if !urlIsReachable(URL) {
		fmt.Printf("URL: %s is unreachable!\n",URL)
		return
	}
	fmt.Printf("[%s] Downloading %s ...\n", DestDir, Filename)
	DownloadFileToPath(URL, DestDir, Filename)
}

// DownloadFileSilent comment
func DownloadFileSilent(URL, DestDir string) {
	Filename, _ := url.PathUnescape(path.Base(URL))
	if !urlIsReachable(URL) {
		fmt.Printf("URL: %s is unreachable!\n",URL)
		return
	}
	DownloadFileToPath(URL, DestDir, Filename)
}

// DownloadFileToPath comment
func DownloadFileToPath(URL, DestDir, Filename string) error {

	if !urlIsReachable(URL) {
		fmt.Printf("URL: %s is unreachable!\n",URL)
		return errors.New("URL is unreachable.")
	}

	// Get the data
	resp, err := http.Get(URL)
	if err != nil {
		return err
	}
	defer resp.Body.Close()

	os.MkdirAll(DestDir, 0755)

	// Create the file
	out, err := os.Create(filepath.FromSlash(path.Join(DestDir, Filename)))
	if err != nil {
		return err
	}
	defer out.Close()

	// Write the body to file
	_, err = io.Copy(out, resp.Body)
	return err

}
