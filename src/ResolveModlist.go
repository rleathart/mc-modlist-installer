package main

import (
	"fmt"
	"log"
	"net/url"
	"os"
	"path"
	"path/filepath"
	"regexp"
	"strings"
	"time"
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
	// Regex that defines comments in URLs. Note that they must be preceded by
	// whitespace since there's a chance that a URL contains a '#'
	commentRegexURL := regexp.MustCompile(`[\s]+#.*`)
	/* This matches the first whitespace character and any characters
	following it. This works since whitespace is illegal in URLs */
	illegalRegexURL := regexp.MustCompile(`[\s].*`)
	// Regex that identifies a control seqeunce in the modlist.
	CSRegex := regexp.MustCompile(`(\[[^]]+\]|<[^>]+>)`)

	file, err := os.Open(Modlist)
	if err != nil {
		log.Fatal(err)
	}
	defer file.Close()

	// []string slice containing the plaintext lines from Modlist file.
	LinesInModlist, err := ReadLines(Modlist)
	if err != nil {
		ErrHandler(err)
	}

	for _, Line := range LinesInModlist {
		// We'll use these later, useful for distinguishing the line from a URL or filename.
		var URL, Filename string
		// Get rid of leading and trailing whitespace
		Line = strings.TrimSpace(Line)
		// Does the line start with "http"?
		isURL := strings.HasPrefix(Line, "http")

		// Replace comments with the empty string.
		if isURL {
			Line = commentRegexURL.ReplaceAllString(Line, "")
			URL = illegalRegexURL.ReplaceAllString(Line, "")
		} else {
			Line = commentRegex.ReplaceAllString(Line, "")
		}

		// Is this line a control sequence?
		isCS := CSRegex.MatchString(Line)

		// If line is empty after processing comments or it's not a URL or control
		// sequence, go to the next line.
		if len(Line) == 0 || (!isURL && !isCS) {
			continue
		}

		if !isURL {
			// Is this line a directory control sequence?
			isDirectoryCS := isCS && len(ExtractFromDelims(Line, "[]")) > 0
			// Do this for every directory CS
			if isDirectoryCS {
				// Set the destination directory to the text inside the first pair of
				// square brackets.
				DestDir = ExtractFromDelims(Line, "[]")[0]
				// Reset alwaysFetch to its default value
				alwaysFetch = alwaysFetchDefault
				// Set mods to be common to both server and client by default
				clientMod = -1
				// Unless it's specified later by a control sequence, we're not updating
				updating = false
				// Reset userDownload
				userDownload = false

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
			// Get control sequences from Line
			for _, cs := range ExtractFromDelims(Line, "<>") {
				switch strings.ToLower(cs) {
				case "server-only":
					clientMod = 0
				case "client-only":
					clientMod = 1
				case "common":
					clientMod = -1
				case "always-fetch":
					alwaysFetch = true
				case "smart-fetch":
					alwaysFetch = false
				case "update":
					/* URLs should always be downloaded when updating. They should also be
					updated for both client and server */
					alwaysFetch = true
					clientMod = -1
					updating = true
				case "user-download":
					userDownload = true
				default:
					fmt.Printf("Unknown control sequence: %s\n", strings.ToLower(cs))
				}
			}
			// That's all we need to do for anything that's not a URL, so just move on
			// to the next line.
			continue
		}

		/* If this line *is* a URL */
		// Get the base path from the URL, that is, the filename.
		Filename, err = url.PathUnescape(path.Base(URL))
		if err != nil {
			ErrHandler(err)
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
		// Add DestDir to mods dirs if it's not already there.
		// Note that we don't want to add directories like '.' and 'config'
		if DestDir != "." && DestDir != "config" && !ContainsStr(modDirs, DestDir) {
			modDirs = append(modDirs, DestDir)
		}

		// If this is a file that the user must download manually.
		if userDownload {
			/* We use this regex to remove anyhing before the first group of whitespace
			this is to avoid the scenario where the URL contains a pair of curly braces */
			re := regexp.MustCompile(`[^\s]*[\s]+`)
			/* Set Filename to the text inside the first pair of curly braces.
			e.g. For 'https://somehost/somedir/filetodownload.jar {SomeMod.jar}',
			Filename is "SomeMod.jar". */
			Filename = ExtractFromDelims(re.ReplaceAllString(Line, ""), "{}")[0]

			// TODO: Here we are not supporting the behaviour of operating without a
			// cache.
			if !PathExists(filepath.Join(modCache, Filename)) {
				fmt.Printf("Please download %s from the URL below and place it in %s\n",
					Filename, DownloadsDirectory)
				fmt.Println(URL)

				// This string will store the path to the downloaded file.
				var Download string = filepath.Join(DownloadsDirectory, Filename)
				var DownloadFileSize int64
				/* Loop to check if the user has downloaded the file. Once they have,
				move it to the cache. */
				for {
					// Do nothing for 500ms, we don't want to be pointlessly wasting CPU cycles.
					time.Sleep(500 * time.Millisecond)

					FileInfo, err := os.Stat(Download)
					if err != nil {
						// Try again if the file doesn't exist
						continue
					}

					DownloadFileSize = FileInfo.Size()
					// Sleep for 250ms to see if the file size has changed
					time.Sleep(250 * time.Millisecond)
					// Note that we don't need to handle err here since we already do that above.
					FileInfo, _ = os.Stat(Download)

					/* If the file size has changed since we checked 250ms ago or it's still
					zero, continue since the file is probably still downloading. */
					if DownloadFileSize != FileInfo.Size() || FileInfo.Size() == 0 {
						continue
					}

					// Move the download to the cache.
					os.Rename(Download, filepath.Join(modCache, Filename))
					// Terminate loop
					break
				}
			}
		}

		// If this line is part of the update block.
		if updating {
			if hasUpdated {
				// Go to the next line if we've already updated
				continue
			} else {
				// Otherwise, download the URL.
				fmt.Printf("Updating %s ...\n", Filename)
				DownloadFileSilent(URL, DestDir)
				continue
			}
		}

		// If we don't want to alwaysFetch this file
		if !always && !alwaysFetch {
			if PathExists(filepath.Join(modCache, Filename)) {
				GetFileFromCache(Filename, DestDir)
			} else {
				if !urlIsReachable(URL) {
					fmt.Printf("URL: %s is unreachable!\n", URL)
					continue
				}
				fmt.Printf("[%s] Downloading %s ...\n", DestDir, Filename)
				DownloadFileSilent(URL, modCache)
				GetFileFromCache(Filename, DestDir)
			}
		}

		if alwaysFetch {
			DownloadFile(URL, DestDir)
		}
	}

	// Slice containing all mods
	var mods []string
	// For each directory in modDirs slice, that is, any directory that we have
	// put mods in.
	for _, dir := range modDirs {
		// Get all the files in said directory
		modfiles, _ := GetFilesInDirectory(dir)
		// For each of those mods, keep a record of their relative path
		for i, mod := range modfiles {
			// e.g. modfiles[i] = "mods/SomeMod.jar"
			modfiles[i] = filepath.Join(dir, mod)
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
				if strings.Contains(line, encodedMod) {
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
