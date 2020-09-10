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
	home, err := os.UserHomeDir()
	if err == nil {
		return home
	}

	if isWindows {
		home = filepath.Join(os.Getenv("HOMEDRIVE"), os.Getenv("HOMEPATH"))
		return home
	}

	return home
}
