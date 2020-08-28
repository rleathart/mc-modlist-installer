package main

import (
	"io/ioutil"
)

// GetFilesInDirectory comment
func GetFilesInDirectory(root string) ([]string, error) {
	var files []string
	fileInfo, err := ioutil.ReadDir(root)
	if err != nil {
		return files, err
	}

	for _, file := range fileInfo {
		files = append(files, file.Name())
	}
	return files, nil

}
