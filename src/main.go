package main

import (
	"fmt"
	"os"
	"path/filepath"
	"runtime"
)

var (
	exeDir string = getExecutingDirectory()

	isWindows bool = runtime.GOOS == "windows"
	isLinux   bool = runtime.GOOS == "linux"
	isOSX     bool = runtime.GOOS == "darwin"
	isFreeBSD bool = runtime.GOOS == "freebsd"

	isServer bool

	modlists []string
	modDirs  []string

	homeDir   string = getHomeDirectory()
	configDir string
	modCache  string

	alwaysFetch bool
	useCache    bool = true

	updating   bool = false
	hasUpdated bool = false
)

func main() {
	if len(os.Args) > 1 {
		exeDir, _ = filepath.Abs(os.Args[1])
	}

	fmt.Printf("CWD is: %s\n", exeDir)
	err := os.Chdir(exeDir)
	if err != nil {
		ErrHandler(err)
	}

	serverfiles, _ := filepath.Glob("*server*")
	isServer = len(serverfiles) > 0

	SetCacheLocation()

	// Check if there are any modlists
	modlists, _ = filepath.Glob("*.modlist")
	// If not, prompt the user for a URL to a modlist
	if len(modlists) == 0 {
		fmt.Println("No modlists found.")
		var remoteURL string
		for {
			remoteURL = GetUserInput("Please specify a modlist URL: ")
			if urlIsReachable(remoteURL) {
				break
			} else {
				fmt.Println("Invalid URL!")
			}
		}
		DownloadFile(remoteURL, ".")
	}
	// Regenerate list of modlists since it's changed since the user downloaded one
	modlists, _ = filepath.Glob("*.modlist")

	for _, modlist := range modlists {
		fmt.Printf("Resolving %s ...\n", modlist)
		ResolveModlist(modlist)
	}

	ExitHandler("Done! ", 0)
}
