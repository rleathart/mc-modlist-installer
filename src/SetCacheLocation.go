package main

import (
	"fmt"
	"os"
	"path/filepath"
)

// SetCacheLocation comment
func SetCacheLocation() {
	if isWindows {
		modCache = filepath.Join(os.Getenv("ProgramData"), "mcmli", "cache")
		configDir = filepath.Join(os.Getenv("ProgramData"), "mcmli", "config")
	} else {
		modCache = filepath.Join(homeDir, ".cache", "mcmli")
		configDir = filepath.Join(homeDir, ".config", "mcmli")
	}
	os.MkdirAll(modCache, 0755)
	os.MkdirAll(configDir, 0755)

	fmt.Printf("Mod cache: %s\n", modCache)
	fmt.Printf("Config directory: %s\n", configDir)
}

func getHomeDirectory() string {
	var home string
	if isWindows {
		home = filepath.Join(os.Getenv("HOMEDRIVE"), os.Getenv("HOMEPATH"))
		if home == "" {
			home = os.Getenv("USERPROFILE")
		}
		return home
	}

	home = os.Getenv("HOME")
	return home
}
