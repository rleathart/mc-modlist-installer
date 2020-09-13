package main

import (
	"bufio"
	"fmt"
	"log"
	"net/url"
	"os"
	"path"
	"path/filepath"
	"regexp"
	"strings"
)

// ResolveModlist comment
func ResolveModlist(Modlist string) {
	ResolveModlistAlways(Modlist, false)
}

// ResolveModlistAlways comment
func ResolveModlistAlways(Modlist string, always bool) {

	var DestDir string = "."
	var alwaysFetchDefault bool = always
	alwaysFetch = alwaysFetchDefault
	var clientMod int = -1

	commentRegex := regexp.MustCompile(`#.*`)
	commentRegexURL := regexp.MustCompile(` #.*`)
	CSRegex := regexp.MustCompile(`(\[[^]]+\]|<[^>]+>)`)

	file, err := os.Open(Modlist)
	if err != nil {
		log.Fatal(err)
	}
	defer file.Close()

	scanner := bufio.NewScanner(file)
	for scanner.Scan() {
		var Line string = strings.TrimSpace(scanner.Text())
		var isURL bool = strings.HasPrefix(Line, "http")
		if len(Line) == 0 { // Skip empty lines
			continue
		}

		if !isURL {
			Line = commentRegex.ReplaceAllString(Line, "") // ignore comments
			isCS := CSRegex.MatchString(Line)

			if !isCS { // If this line doesn't look like a control sequence, skip it
				continue
			}

			isDirectoryCS := len(ExtractFromDelims(Line, "[]")) > 0
			if isDirectoryCS { // Do this for every directory CS
				DestDir = ExtractFromDelims(Line, "[]")[0]
				alwaysFetch = alwaysFetchDefault
				clientMod = -1

				updating = false

				if !hasUpdated {
					hasUpdated = true

					ResolveModlistAlways(Modlist, always)
					hasUpdated = false
					return
				}
			}

			// Check for client/server only and alwaysFetch
			for _, cs := range ExtractFromDelims(Line, "<>") {
				if strings.ToLower(cs) == "server-only" {
					clientMod = 0
				}
				if strings.ToLower(cs) == "client-only" {
					clientMod = 1
				}
				if strings.ToLower(cs) == "always-fetch" {
					alwaysFetch = true
				}
				if strings.ToLower(cs) == "update" {
					alwaysFetch = true
					clientMod = -1
					updating = true
				}
			}
			continue
		}

		Line = commentRegexURL.ReplaceAllString(Line, "")

		Filename, err := url.PathUnescape(path.Base(Line))
		if err != nil {
			log.Fatal(err)
		}

		if clientMod == 1 && isServer {
			if PathExists(filepath.Join(DestDir, Filename)) {
				fmt.Printf("[%s] Removing client only file: %s\n", DestDir, Filename)
				os.Remove(filepath.Join(DestDir, Filename))
			}
			continue
		}
		if clientMod == 0 && !isServer {
			if PathExists(filepath.Join(DestDir, Filename)) {
				fmt.Printf("[%s] Removing server only file: %s\n", DestDir, Filename)
				os.Remove(filepath.Join(DestDir, Filename))
			}
			continue
		}

		os.MkdirAll(DestDir, 0755)

		if updating {
			if hasUpdated {
				continue
			} else {
				fmt.Println("Updating ...")
				DownloadFile(Line, DestDir)
				continue
			}
		}

		if !ContainsStr(modDirs, DestDir) && DestDir != "." && DestDir != "config" {
			modDirs = append(modDirs, DestDir)
		}

		if useCache && !always && !alwaysFetch {
			GetFileFromCache(Line, DestDir)
			continue
		}

		if alwaysFetch || !PathExists(filepath.Join(DestDir, Filename)) {
			DownloadFile(Line, DestDir)
		} else {
			fmt.Printf("[%s] Skipping existing file %s\n", DestDir, Filename)
		}

	}

	if err := scanner.Err(); err != nil {
		log.Fatal(err)
	}

	var mods []string
	for _, i := range modDirs {
		modfiles, _ := GetFilesInDirectory(i)
		for j, k := range modfiles {
			modfiles[j] = filepath.Join(i, k)
		}
		mods = append(mods, modfiles...)
	}

	for _, mod := range mods {
		var isInModlist bool = false
		dir := filepath.Dir(mod)
		encodedMod := url.PathEscape(filepath.Base(mod))
		encodedMod = strings.Replace(encodedMod, "%27", "'", -1)
		for _, modlist := range modlists {
			modlistLines, _ := ReadLines(modlist)
			for _, line := range modlistLines {
				if strings.HasSuffix(line, encodedMod) {
					isInModlist = true
					goto NEXTMOD // No point doing any further checking for this mod
				}
			}
		}
		if !isInModlist {
			fmt.Printf("%s found in '%s' directory but not in any modlist.\n", encodedMod, dir)
			resp := GetUserInput("Do you want to remove it? [Y/n]: ")
			if strings.ToLower(resp) == "y" || resp == "" {
				os.Remove(mod)
			}
		}
	NEXTMOD:
	}
}
