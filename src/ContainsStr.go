package main

// ContainsStr comment
func ContainsStr(array []string, text string) bool {
	if len(array) == 0 {
		return false
	}

	for _, i := range array {
		if i == text {
			return true
		}
	}
	return false
}
