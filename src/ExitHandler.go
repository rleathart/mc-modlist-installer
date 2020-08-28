package main

import (
	"bufio"
	"fmt"
	"os"
)

// ExitHandler exits
func ExitHandler(sayThis string, retVal int) {
	fmt.Print(sayThis)
	fmt.Print("Press Enter to exit ...")
	reader := bufio.NewReader(os.Stdin)
	reader.ReadString('\n')

	os.Exit(retVal)
}
