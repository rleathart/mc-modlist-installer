.PHONY: release

build:
	@dotnet build
run:
	@dotnet run

todo:
	@find . -type f -name '*.cs' -exec grep 'TODO' {} \+

release:
	@rm -r release
	@mkdir release
	dotnet publish -r osx-x64 --self-contained=false
	dotnet publish -r linux-x64 --self-contained=false
	dotnet publish -r win-x64 --self-contained=false
	@#
	mv ./bin/Debug/netcoreapp3.1/osx-x64/mcmli release/install_macOS
	mv ./bin/Debug/netcoreapp3.1/linux-x64/mcmli release/install_linux
	mv ./bin/Debug/netcoreapp3.1/win-x64/mcmli.exe release/install.exe
	@#
	dotnet publish -r osx-x64   --self-contained=true -p:PublishTrimmed=true -p:PublishSingleFile=true -p:IncludeNativeLibrariesInSingleFile=true
	dotnet publish -r linux-x64 --self-contained=true -p:PublishTrimmed=true -p:PublishSingleFile=true -p:IncludeNativeLibrariesInSingleFile=true
	dotnet publish -r win-x64   --self-contained=true -p:PublishTrimmed=true -p:PublishSingleFile=true -p:IncludeNativeLibrariesInSingleFile=true
	@#
	mv ./bin/Debug/netcoreapp3.1/osx-x64/mcmli release/install-static_macOS
	mv ./bin/Debug/netcoreapp3.1/linux-x64/mcmli release/install-static_linux
	mv ./bin/Debug/netcoreapp3.1/win-x64/mcmli.exe release/install-static.exe
