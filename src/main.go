package main

import (
	"fmt"
	"os"
	"path/filepath"
	"runtime"
)

var exeDir string = getExecutingDirectory()

var isWindows bool = runtime.GOOS == "windows"
var isLinux bool = runtime.GOOS == "linux"
var isOSX bool = runtime.GOOS == "darwin"
var isFreeBSD bool = runtime.GOOS == "freebsd"

var remotesList string = ".install.remote"
var isServer bool

var modlists []string
var modDirs []string

var homeDir string = getHomeDirectory()
var configDir string
var modCache string

var alwaysFetch bool
var useCache bool = true

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

	modlists, _ = filepath.Glob("*.modlist")
	FetchRemoteList()
	modlists, _ = filepath.Glob("*.modlist")

	for _, modlist := range modlists {
		fmt.Printf("Resolving %s ...\n", modlist)
		ResolveModlist(modlist)
	}

	ExitHandler("Done! ", 0)
}
