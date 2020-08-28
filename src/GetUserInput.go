package main

import (
	"fmt"
)

// GetUserInput comment
func GetUserInput(prompt string) string {
	fmt.Print(prompt)
	var input string
	fmt.Scanln(&input)

	return input
}
