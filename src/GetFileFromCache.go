package main

import (
	"fmt"
	"net/url"
	"os"
	"path"
	"path/filepath"
)

// GetFileFromCache comment
func GetFileFromCache(URL, DestDir string) {
	Filename, _ := url.PathUnescape(path.Base(URL))

	var LocalFile string = filepath.Join(exeDir, DestDir, Filename)
	var CachedFile string = filepath.Join(modCache, Filename)

	if PathExists(LocalFile) {
		err := os.Remove(LocalFile)
		ErrHandler(err)
	}

	if PathExists(CachedFile) {
		fmt.Printf("[%s] Linking %s ...\n", DestDir, Filename)
		err := os.Symlink(CachedFile, LocalFile)
		ErrHandler(err)
	} else {
		fmt.Printf("[%s] Downloading %s ...\n", DestDir, Filename)
		DownloadFileSilent(URL, modCache)
		GetFileFromCache(URL, DestDir)
	}
}
