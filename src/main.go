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

	remotesList string = ".install.remote"
	isServer    bool

	modlists []string
	modDirs  []string

	homeDir   string = getHomeDirectory()
	configDir string
	modCache  string

	alwaysFetch bool
	useCache    bool = true
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

	modlists, _ = filepath.Glob("*.modlist")
	FetchRemoteList()
	modlists, _ = filepath.Glob("*.modlist")

	for _, modlist := range modlists {
		fmt.Printf("Resolving %s ...\n", modlist)
		ResolveModlist(modlist)
	}

	ExitHandler("Done! ", 0)
}
