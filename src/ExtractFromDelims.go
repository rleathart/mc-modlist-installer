package main

import (
	"regexp"
)

// ExtractFromDelims comment
func ExtractFromDelims(Text, Delims string) []string {
	// re := regexp.MustCompile(`.*` + Delims[0] + `(.*?)` + Delims[1] + `.*`)
	re := regexp.MustCompile(`.*`)
	if Delims == "[]" {
		re = regexp.MustCompile(`\[(.*?)\]`)
	}
	if Delims == "<>" {
		re = regexp.MustCompile(`<(.*?)>`)
	}

	var Matches []string

	for _, match := range re.FindAllStringSubmatch(Text, -1) {
		Matches = append(Matches, match[1])
	}

	return Matches
}
