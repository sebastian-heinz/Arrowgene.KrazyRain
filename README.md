Arrowgene.KrazyRain
===
Research for reviving and server emulation of the game KrazyRain

## Table of contents
- [Disclaimer](#disclaimer)
- [Files](#files)
- [Game Assets](#game-assets)
- [Executables](#executables)
  - [Game.exe](#gameexe)
  - [KRAZYRAIN.exe](#krazyrainexe)
  - [KRPATCHER.exe](#krpatcherexe)
- [Project](#project)
  - [Arrowgene.KrazyRain.Cmd](#arrowgenekrazyraincmd)
  
Disclaimer
===
The project is intended for educational purpose only, we strongly discourage operating a public server.
This repository does not distribute any game related assets or copyrighted material, 
pull requests containing such material will not be accepted.
All files have been created from scratch, and are subject to the copyright notice of this project.
If a part of this repository violates any copyright or license, please state which files and why,
including proof and it will be removed.


# Files
The Files folder contains a collection of ready to use files.
If you are here for a offline patch, you can just grab it from here:
https://github.com/sebastian-heinz/Arrowgene.KrazyRain/tree/master/Files


# Game Assets
Asset acces is managed by `VDISK.DLL`.
The main assets are `*.SNP` files which is a zip like archive format.

Header (24bytes)
| bytes  | type   | description |
| ------- |----- | ------------- |
| 8bytes | string | magic bytes format: `VDISK1.0` |
| 2bytes | uint16 | unknown |
| 1byte  | byte   | unknown |
| 4byte  | uint32 | file count |
| 4byte  | uint32 | folder count |
| 4byte  | uint32 | total bytes of file - 145 (entry headere) |

Entry Header (145 bytes) - repeated until end of file
| bytes  | type   | description |
| ------- |----- | ------------- |
| 1byte  | byte   | type (0=file, 1=folder) |
| 128bytes | string | entry name |
| 4byte  | uint32 | data length uncompressed |
| 4byte  | uint32 | data length compressed |
| 4byte  | uint32 | parent folder offset |
| 4byte  | uint32 | end of entry offset |
| Xbyte  | byte[] | file data |

Each `file data` binary blob starts with `0x78 0x01` which indicates a zlib compression (https://tools.ietf.org/html/rfc1950).
After decompressing you are left with the original file.
It is also possible to recreate the same file structure and remove the original `*.SNP` archives, the game will be able to read the original uncompressed files from disk.


# Executables

## Game.exe
The main executable of the game.

### Arguments
```
Game.exe fromlauncher run [Account] [Hash] [Byte]
: used to run the main game
- Account: maximum length of 16bytes
- Hash: maximum length of 32bytes
- Byte: a byte value (0-255)

Example run: 
Game.exe fromlauncher run Account Hash 1


Game.exe fromlauncher version #32770
: called by KRPATCHER.exe to receive version number of Game.exe

Game.exe fromlauncher test
: unknown purpose
```
### Patches
#### Disable HackShield
```
<$game.396824>
	xor eax, eax
	ret

<$game.396AB2>
	xor eax, eax
	ret
```

## KRAZYRAIN.EXE
Launcher of the Game.

### DNS Lookups
The launcher tries to connect to the following hosts:
```
127.0.0.1 patch.krazyrain.com
127.0.0.1 krazyrain.com
127.0.0.1 launcher.krazyrain.com
```
in order to redirect the requests, modify your `hosts` file accordingly.

### Arguments
```
KRAZYRAIN.exe fromlauncher version #32770
KRAZYRAIN.exe hiderun
```

### HTTP Requests
The launcher will request various `.ini` files.   

some of them contain the `FileVersion`:
```
0x01020304 = 1.2.3.4
```

#### LauncherVersion.ini
Contains `FileVersion` of `KRAZYRAIN.EXE`
```
0x01000015
```

#### PatcherVersion.ini
Contains `FileVersion` of `KRPATCHER.EXE`
```
0x1000109
```

#### LauncherCheck.ini
Not sure what it contains yet
```
-0x1
```

#### ClientVersion.ini
Contains a generic versioning of `Game.exe` in `x.x.x` format.
```
3.15.0
```

#### Launcher_check.snp
HTML file to displat while checking

#### Launcher.snp
HTML file to displat after check completed


## KRPATCHER.exe
Game Patcher that delivers updates.


# Project
Summary of projects

## Arrowgene.KrazyRain.Cmd
Main entry point to run individual parts.

Extract specific `.SNP` file:
`
vdisk-extract
D:\Games\KrazyRain\Data\DATA.SNP
D:\Games\KrazyRain\DATA_OUT
`
Note: ensure `DATA_OUT` exists.   

Extract all `.SNP` files in folder:
`
vdisk-extract-all
D:\Games\KrazyRain\Data
D:\Games\KrazyRain\DATA_OUT
`
Note: ensure `DATA_OUT` exists.   

Repack a folder into an `.SNP` archive:
`
vdisk-archive
D:\Games\KrazyRain\DATA_OUT
DATA
D:\Games\KrazyRain\DATA_OUT_SNP\DATA.SNP
`
Note: ensure `DATA_OUT_SNP` exists.
Note: This has only been tested with the `DATA.SNP` file, the implementation for creating a archive requires more work.


## Arrowgene.KrazyRain.GameServer
Server emulator to answer the `Game.exe` network requests

## Arrowgene.KrazyRain.PatchServer
PatchServer emulator, to answer the `KRPATCHER.exe` http requests.

## Arrowgene.KrazyRain.VirtualDisk
Implementation of the `VDISK.DLL` methods, to pack and unpack `.SNP` files.

## Arrowgene.KrazyRain.Dll
c++ DLL that is registered in the import table of `Arrowgene.Game.exe`. The DLL is able to patch the game on the fly, to disable Hackschield, enable offline mode or log VDisk access.
