package main

import (
	"fmt"
	"os"
	"path/filepath"
)

// GetFileFromCache Link file from modCache to DestDir if it exists in the cache
func GetFileFromCache(Filename, DestDir string) {
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
	}
}
