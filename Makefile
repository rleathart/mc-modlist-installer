PROJNAME = mcmli
BINDIR = release

.PHONY: release

build:
	@go build -o ${PROJNAME} ./src
run:
	@go run ./src

release:
	PATH="$$PATH:$$HOME/go/bin"
	env CGO_ENABLED=0 GOOS=freebsd GOARCH=amd64 go build -o ${BINDIR}/${PROJNAME}-freebsd-amd64 ./src
	env CGO_ENABLED=0 GOOS=linux GOARCH=amd64 go build -o ${BINDIR}/${PROJNAME}-linux-amd64 ./src
	env CGO_ENABLED=0 GOOS=darwin GOARCH=amd64 go build -o ${BINDIR}/${PROJNAME}-darwin-amd64 ./src
	rsrc -manifest src/mcmli-windows-amd64.exe.manifest -arch amd64 -o src/rsrc.syso
	env CGO_ENABLED=0 GOOS=windows GOARCH=amd64 go build -o ${BINDIR}/${PROJNAME}-windows-amd64.exe ./src

todo:
	@find . -type f -not -path '*bin*' -not -path '*obj*' -not -name 'Makefile' -exec grep -Ii 'TODO' {} \+
