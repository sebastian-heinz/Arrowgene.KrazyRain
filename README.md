Arrowgene.KrazyRain
===

# Game Assets
Asset acces is managed by `VDISK.DLL`.
The main assets are `*SNP` files which is a zip like archive format.

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
## KRPATCHER.exe
```
fromlauncher version #32770
hiderun
```
```
127.0.0.1 patch.krazyrain.com
127.0.0.1 krazyrain.com
127.0.0.1 launcher.krazyrain.com
```


Version:
```
0x01020304 = 1.2.3.4
```

