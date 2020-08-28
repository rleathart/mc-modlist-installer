package main

import (
	"log"
)

// ErrHandler comment
func ErrHandler(err error) {
	if err != nil {
		log.Println(err)
		ExitHandler("Cannot continue. ", 1)
	}
}
