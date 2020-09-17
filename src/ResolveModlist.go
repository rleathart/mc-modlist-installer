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

// ResolveModlistAlways Resolve a modlist, optionally downloading everything
// regardless of whether it exists already.
func ResolveModlistAlways(Modlist string, always bool) {

	// Download to the current directory by default
	var DestDir string = "."
	// Reset alwaysFetch to this value for every directory specification
	var alwaysFetchDefault bool = always
	alwaysFetch = alwaysFetchDefault
	// Is this a common mod (-1), server only (0) or client only (1)
	var clientMod int = -1
	// Does the user need to download something manually?
	var userDownload bool = false

	// Regex that defines comments
	commentRegex := regexp.MustCompile(`#.*`)
	// Regex that defines comments in URLs
	commentRegexURL := regexp.MustCompile(` #.*`)
	// Regex that identifies a control seqeunce in the modlist.
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
				// Reset alwaysFetch to its default value
				alwaysFetch = alwaysFetchDefault
				// Set mods to be common to both server and client by default
				clientMod = -1

				updating = false

				if !hasUpdated {
					// We'll definitely have updated by the time we get to a directory CS.
					hasUpdated = true
					// Now we wan't to recall this function with hasUpdated = true
					ResolveModlistAlways(Modlist, always)
					// Reset hasUpdated in case there are other modlists that need to update
					// themselves.
					hasUpdated = false
					// We're all done with this modlist after the recursion above, so just return
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
				if strings.ToLower(cs) == "common" {
					clientMod = -1
				}
				if strings.ToLower(cs) == "always-fetch" {
					alwaysFetch = true
				}
				if strings.ToLower(cs) == "smart-fetch" {
					alwaysFetch = false
				}
				if strings.ToLower(cs) == "update" {
					/* URLs should always be downloaded when updating. They should also be
					updated for both client and server */
					alwaysFetch = true
					clientMod = -1
					updating = true
				}
			}
			// That's all we need to do for anything that's not a URL, so just move on
			// to the next line.
			continue
		}

		Line = commentRegexURL.ReplaceAllString(Line, "")

		Filename, err := url.PathUnescape(path.Base(Line))
		if err != nil {
			log.Fatal(err)
		}

		// If this file is client only and the current instance is a server
		if clientMod == 1 && isServer {
			// Remove the file if it exists
			if PathExists(filepath.Join(DestDir, Filename)) {
				fmt.Printf("[%s] Removing client only file: %s\n", DestDir, Filename)
				os.Remove(filepath.Join(DestDir, Filename))
			}
			// Go to the next line in LinesInModlist
			continue
		}
		// As above
		if clientMod == 0 && !isServer {
			if PathExists(filepath.Join(DestDir, Filename)) {
				fmt.Printf("[%s] Removing server only file: %s\n", DestDir, Filename)
				os.Remove(filepath.Join(DestDir, Filename))
			}
			continue
		}

		// Create the destination directory.
		os.MkdirAll(DestDir, 0755)

		if updating {
			if hasUpdated {
				// Go to the next line if we've already updated
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

	// Slice containing all mods
	var mods []string
	for _, i := range modDirs {
		modfiles, _ := GetFilesInDirectory(i)
		for j, k := range modfiles {
			modfiles[j] = filepath.Join(i, k)
		}
		// Append this list of mods to the main slice.
		mods = append(mods, modfiles...)
	}

	for _, mod := range mods {
		var isInModlist bool = false
		// The directory that the mod is in
		dir := filepath.Dir(mod)
		encodedMod := url.PathEscape(filepath.Base(mod))
		// We want to decode some strings because of inconsistencies.
		encodedMod = strings.Replace(encodedMod, "%27", "'", -1)

		// For each modlist, check if any line in it contains the escaped mod filename
		for _, modlist := range modlists {
			modlistLines, _ := ReadLines(modlist)
			for _, line := range modlistLines {
				if strings.HasSuffix(line, encodedMod) {
					isInModlist = true
					// No point doing any further checking for this mod
					goto NEXTMOD
				}
			}
		}
		// If a file is found locally, but it's not in a modlist: prompt the user to
		// remove it.
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
