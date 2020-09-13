PROJNAME = mcmli
BINDIR = release

.PHONY: release

build:
	@go build -o ${PROJNAME} ./src
run:
	@go run ./src

release:
	env CGO_ENABLED=0 GOOS=freebsd GOARCH=amd64 go build -o ${BINDIR}/${PROJNAME} ./src
	cd release; gzip mcmli && mv mcmli.gz mcmli-freebsd-amd64.gz
	env CGO_ENABLED=0 GOOS=linux GOARCH=amd64 go build -o ${BINDIR}/${PROJNAME} ./src
	cd release; gzip mcmli && mv mcmli.gz mcmli-linux-amd64.gz
	env CGO_ENABLED=0 GOOS=darwin GOARCH=amd64 go build -o ${BINDIR}/${PROJNAME} ./src
	cd release; gzip mcmli && mv mcmli.gz mcmli-macOS-amd64.gz
	~/go/bin/rsrc -manifest src/mcmli-windows-amd64.exe.manifest -arch amd64 -o src/rsrc.syso
	env CGO_ENABLED=0 GOOS=windows GOARCH=amd64 go build -o ${BINDIR}/${PROJNAME}.exe ./src
	cd release; zip mcmli-windows-amd64.zip mcmli.exe
	rm release/mcmli.exe

todo:
	@find . -type f -name '*.go' -exec grep -Ii 'TODO' {} \+
