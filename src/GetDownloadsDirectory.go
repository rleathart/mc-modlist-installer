package main

import (
	"os"
	"path/filepath"
)

func GetDownloadsDirectory() string {
	var dirs = []string{
		os.Getenv("XDG_DOWNLOAD_DIR"),
		filepath.Join(homeDir, "Downloads"),
		filepath.Join(homeDir, "downloads"),
	}

	// Return the first existing directory in dirs
	for _, path := range dirs {
		FileInfo, err := os.Stat(path)
		if os.IsNotExist(err) {
			continue
		}

		if FileInfo.IsDir() {
			return path
		}
	}

	// If none of the locations in dirs exist, the user can always download to their
	// home directory.
	return homeDir
}
