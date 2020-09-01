#include <windows.h>
#include <iostream>
#include <psapi.h>
#include <tlhelp32.h>
#include <tchar.h>
#include <codecvt>
#include <fstream>
#include <vector>

#include "framework.h"
#include "util.h"
#include "memory.h"
#include "ini.h"

HMODULE base;
DWORD base_addr;

bool ini_console;
bool ini_disable_hackshield;
bool ini_offline;
bool ini_hook_vdisk;

typedef void* (__thiscall* func_vdisk_open_file)(void* thiz, const char* file);
func_vdisk_open_file real_vdisk_open_file;


void dummy() {

}

void* __fastcall vdisk_open_file(void* thiz, void* edx, const char* file) {
	fprintf(stdout, "vdisk_open_file: %s\n", file);
	void* ret = real_vdisk_open_file(thiz, file);
	return ret;
}

BOOL APIENTRY DllMain(HMODULE hModule,
	DWORD  ul_reason_for_call,
	LPVOID lpReserved
)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH: {
		// set default values
		ini_console = true;
		ini_disable_hackshield = true;
		ini_offline = true;
		ini_hook_vdisk = false;

		// load ini
		CSimpleIniA ini;
		ini.SetUnicode();
		if (ini.LoadFile("arrowgene.ini") < 0) {
			// create default ini
			ini.SetBoolValue("arrowgene", "console", ini_console);
			ini.SetBoolValue("arrowgene", "disable_hackshield", ini_disable_hackshield);
			ini.SetBoolValue("arrowgene", "offline", ini_offline);
			ini.SetBoolValue("arrowgene", "hook_vdisk", ini_hook_vdisk);
			if (ini.SaveFile("arrowgene.ini") < 0) {
				// ini create error
			}
		}

		ini_console = ini.GetBoolValue("arrowgene", "console", ini_console);
		ini_disable_hackshield = ini.GetBoolValue("arrowgene", "disable_hackshield", ini_disable_hackshield);
		ini_offline = ini.GetBoolValue("arrowgene", "offline", ini_offline);
		ini_hook_vdisk = ini.GetBoolValue("arrowgene", "hook_vdisk", ini_hook_vdisk);

		if (ini_console)
		{
			if (AllocConsole() == TRUE) {
				FILE* nfp[3];
				freopen_s(nfp + 0, "CONOUT$", "rb", stdin);
				freopen_s(nfp + 1, "CONOUT$", "wb", stdout);
				freopen_s(nfp + 2, "CONOUT$", "wb", stderr);
				std::ios::sync_with_stdio();
			}
			fprintf(stdout, "DLL_PROCESS_ATTACH\n");
		}

		std::wstring exe_name = get_exe_name();
		base = GetModuleHandle(exe_name.c_str());
		base_addr = (DWORD)base;

		if (ini_console) {
			fprintf(stdout, "base: %p \n", base);
			fprintf(stdout, "base_addr: %u \n", base_addr);
		}

		if (ini_disable_hackshield) {
			// Disable Hackshield
			WriteMemory(base_addr + 0x396824, "\x33\xC0\xC3", 3);
			WriteMemory(base_addr + 0x396AB2, "\x33\xC0\xC3", 3);
		}

		if (ini_offline) {
			// force offline mode
			WriteMemory(base_addr + 0x6BFB0, "\xB2\x00\x90", 3);
		}

		if (ini_hook_vdisk) {
			// hook vdisk
			ReadMemory(base_addr + 0x3D47C8, real_vdisk_open_file, 4);
			ChangeMemory(base_addr + 0x3D47C8, vdisk_open_file, 4);
		}
		break;
	}
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}

