An easy to use modlist installer for Minecraft Java Edition. Think of this as an
open source alternative to the Twitch Launcher.

## Usage for users

Usage is as simple as downloading the [latest release](https://github.com/rleathart/mcmli/releases)
for your operating system to a modpack directory and then running the executable.
The installer will ask you to specify a modlist URL: just copy and paste a link
to a modlist.
For reference, after running the installer your directory structure will look
something like:
```
./
├── config/
├── main.modlist
├── mcmli.exe
└── mods/
```

On macOS and Linux, you may have to set execute permissions for the file before
running:
```
cd /path/to/your/modpack/directory
chmod u+x mcmli
```
Now you can run the binary either by double clicking it, or in the CLI.

## Usage for authors

The installer reads modlists from all files with the `.modlist` extension in
alphabetical order. Typical use should only require one modlist which could be
called something like `main.modlist` or `<modpack-name>.modlist`.

### Modlist format

Perhaps the easiest way to understand the modlist format is to see an example:
```
<Update>
# Modlists can self update by using <Update> as the first
# control directive.
https://url/to/this/modlist

[mods]
# This line is a comment and will be ignored.
https://somesite/commonmod.jar # Comments after URLs must be preceeded by whitespace.
<Client-Only>
https://somesite/client-only-mod.jar
<Server-Only>
https://somesite/server-only-mod.jar

# Here we ask the user to download a file called 'OptiFine_1.12.2_HD_U_F5.jar'
# from the URL 'https://optifine.net/adloadx?f=OptiFine_1.12.2_HD_U_F5.jar'.
# Once they download it, the mod will be moved to the cache and then linked to the
# modpack directory.
[mods] <User-Download>
https://optifine.net/adloadx?f=OptiFine_1.12.2_HD_U_F5.jar {OptiFine_1.12.2_HD_U_F5.jar}

[config] <Always-Fetch>
https://somesite/commonconfig.cfg
```
We refer to text inside square `[]` or angle `<>` brackets as control directives.
Control directives may occur on lines by themselves or on the same line as other
control directives. Text inside `[]` brackets specifies the directory (relative
to the executable) in which to download the following URLs. Text inside `<>`
brackets specify options for the current 'context' (until the next set of `[]`
brackets). Valid options are:
-  `<Update>` Always download these files to the current directory and then
   resolve the modlist as usual.
-  `<Always-Fetch>` Always download these files regardless of whether or not
they are present already.
-  `<Smart-Fetch>` Only download these files if they do not exist (default behaviour).
-  `<Server-Only>` Only download these files if the current instance is a server.
-  `<Client-Only>` Only download these files if the current instance is a client.
-  `<Common>` Download these files for both servers and clients (default).
-  `User-Download` Prompt the user to download a file from the URL given. See above
   for an example.
