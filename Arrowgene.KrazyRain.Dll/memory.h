#pragma once

template <typename I> void ReadMemory(LPVOID address, I& value, int byteNum)
{
	unsigned long OldProtection;
	VirtualProtect(address, byteNum, PAGE_EXECUTE_READWRITE, &OldProtection);
	value = *(I*)address;
	VirtualProtect(address, byteNum, OldProtection, &OldProtection);
}

template <typename I> void ChangeMemory(LPVOID address, I value, int byteNum)
{
	unsigned long OldProtection;
	VirtualProtect(address, byteNum, PAGE_EXECUTE_READWRITE, &OldProtection);
	*(I*)address = value;
	VirtualProtect(address, byteNum, OldProtection, &OldProtection);
}

template <typename I> void WriteMemory(LPVOID address, I value, int byteNum)
{
	unsigned long OldProtection;
	VirtualProtect(address, byteNum, PAGE_EXECUTE_READWRITE, &OldProtection);
	memcpy(address, value, byteNum);
	VirtualProtect(address, byteNum, OldProtection, &OldProtection);
}

template <typename I> void ReadMemory(uintptr_t address, I& value, int byteNum)
{
	unsigned long OldProtection;
	VirtualProtect((LPVOID)(address), byteNum, PAGE_EXECUTE_READWRITE, &OldProtection);
	value = *(I*)address;
	VirtualProtect((LPVOID)(address), byteNum, OldProtection, NULL);
}

template <typename I> void ChangeMemory(uintptr_t address, I value, int byteNum)
{
	unsigned long OldProtection;
	VirtualProtect((LPVOID)(address), byteNum, PAGE_EXECUTE_READWRITE, &OldProtection);
	*(I*)address = value;
	VirtualProtect((LPVOID)(address), byteNum, OldProtection, NULL);
}

template <typename I> void WriteMemory(uintptr_t address, I value, int byteNum)
{
	unsigned long OldProtection;
	VirtualProtect((LPVOID)(address), byteNum, PAGE_EXECUTE_READWRITE, &OldProtection);
	memcpy((LPVOID)address, value, byteNum);
	VirtualProtect((LPVOID)(address), byteNum, OldProtection, NULL);
}

void hook_fn(DWORD baseAddr, DWORD offset, LPVOID fnAddr) {
	DWORD patchHookAddr = baseAddr + offset;
	DWORD relativeFnHookAddr = (DWORD)((char*)fnAddr - (char*)(patchHookAddr + 1 + 4));
	const char* patchInitStart = "\xE8";
	WriteMemory((LPVOID)patchHookAddr, patchInitStart, 1);
	BYTE bRelativeHookInitAddr[4];
	memcpy(bRelativeHookInitAddr, &relativeFnHookAddr, 4);
	WriteMemory((LPVOID)(patchHookAddr + 1), bRelativeHookInitAddr, 4);
}