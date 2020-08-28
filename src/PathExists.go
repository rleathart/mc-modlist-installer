package main

import "os"

// PathExists checks if a path exists
func PathExists(path string) bool {
	if _, err := os.Stat(path); err == nil {
		return true
	}

	return false
}
