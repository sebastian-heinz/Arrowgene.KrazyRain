#pragma once

const int MAX_UDP = 500;

DWORD find_process_id_psapi(const std::wstring& process_name)
{
	DWORD a_processes[1024];
	DWORD cb_needed;
	DWORD c_processes;
	unsigned int i;
	if (!EnumProcesses(a_processes, sizeof(a_processes), &cb_needed))
	{
		return 0;
	}
	c_processes = cb_needed / sizeof(DWORD);
	for (i = 0; i < c_processes; i++)
	{
		DWORD process_id = a_processes[i];
		if (process_id == 0)
		{
			continue;
		}
		HANDLE process_handle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, FALSE, process_id);
		if (process_handle == NULL)
		{
			continue;
		}
		HMODULE handle;
		DWORD cb_needed;
		if (EnumProcessModules(process_handle, &handle, sizeof(handle), &cb_needed))
		{
			TCHAR sz_process_name[MAX_PATH] = TEXT("<unknown>");
			GetModuleBaseName(process_handle, handle, sz_process_name, sizeof(sz_process_name) / sizeof(TCHAR));
			if (!_tcscmp(sz_process_name, process_name.c_str()))
			{
				CloseHandle(process_handle);
				return process_id;
			}
		}
		CloseHandle(process_handle);
	}
	return 0;
}

DWORD find_process_id_tlhelp32(const std::wstring& process_name)
{
	PROCESSENTRY32 process_info;
	process_info.dwSize = sizeof(process_info);
	HANDLE processes_snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, NULL);
	if (processes_snapshot == INVALID_HANDLE_VALUE) {
		return 0;
	}
	Process32First(processes_snapshot, &process_info);
	if (!process_name.compare(process_info.szExeFile))
	{
		CloseHandle(processes_snapshot);
		return process_info.th32ProcessID;
	}
	while (Process32Next(processes_snapshot, &process_info))
	{
		if (!process_name.compare(process_info.szExeFile))
		{
			CloseHandle(processes_snapshot);
			return process_info.th32ProcessID;
		}
	}
	CloseHandle(processes_snapshot);
	return 0;
}

std::wstring get_exe_path() {
	WCHAR exePath[MAX_PATH + 1];
	DWORD pathLen = GetModuleFileNameW(NULL, exePath, MAX_PATH);
	if (pathLen <= 0) {
		return NULL;
	}
	std::wstring path = std::wstring(exePath);
	return path;
}

std::wstring get_exe_name() {
	std::wstring path = get_exe_path();
	size_t idx = path.find_last_of(L"/\\");
	if (idx == std::wstring::npos)
	{
		return NULL;
	}
	idx++;
	size_t len = path.length();
	if (idx >= len) {
		return NULL;
	}
	std::wstring name = path.substr(idx);
	return name;
}

std::wstring get_exe_dir() {
	std::wstring path = get_exe_path();
	size_t idx = path.find_last_of(L"/\\");
	if (idx == std::wstring::npos)
	{
		return NULL;
	}
	idx++;
	size_t len = path.length();
	if (idx >= len) {
		return NULL;
	}
	std::wstring dir = path.substr(0, idx);
	return dir;
}

std::wstring s_2_ws(const std::string& str)
{
	using convert_typeX = std::codecvt_utf8<wchar_t>;
	std::wstring_convert<convert_typeX, wchar_t> converterX;

	return converterX.from_bytes(str);
}

std::string ws_2_s(const std::wstring& wstr)
{
	using convert_typeX = std::codecvt_utf8<wchar_t>;
	std::wstring_convert<convert_typeX, wchar_t> converterX;

	return converterX.to_bytes(wstr);
}

template <typename I> std::string to_hex(I* bytes, int size, bool stop_at_null) {
	static const char hex[16] = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B','C','D','E','F' };
	std::string str;
	for (int i = 0; i < size; ++i) {
		const char ch = bytes[i];
		if (stop_at_null && ch == 0) {
			break;
		}
		str.append(&hex[(ch & 0xF0) >> 4], 1);
		str.append(&hex[ch & 0xF], 1);
		str.append("-");
	}
	return str;
}

template <typename I> std::string to_ascii(I* bytes, int size, bool stop_at_null) {
	std::string str;
	for (int i = 0; i < size; ++i) {
		const char ch = bytes[i];
		if (ch >= 32 && ch <= 127) {
			str.append(&ch, 1);
		}
		else {
			if (stop_at_null && ch == 0) {
				break;
			}
			str.append(".");
		}
	}
	return str;
}

template <typename I>void show(I* bytes, int size, bool stop_at_null) {
	fprintf(stdout, "\n");
	fprintf(stdout, "---------\n");
	fprintf(stdout, "Size: %d\n", size);
	fprintf(stdout, "%s\n", to_ascii(bytes, size, stop_at_null).c_str());
	fprintf(stdout, "%s\n", to_hex(bytes, size, stop_at_null).c_str());
	fprintf(stdout, "---------\n");
	fprintf(stdout, "\n");
}

void write_file(char const* filename, BYTE* fileData, DWORD fileLen)
{
	std::ofstream ofile(filename, std::ios::binary);
	ofile.write((char*)fileData, fileLen);
}

inline bool file_exists(std::wstring file_name) {
	struct _stat file;
	return _wstat(file_name.c_str(), &file) == 0;
}

inline bool file_exists(std::string file_name) {
	struct _stat file;
	return _stat(file_name.c_str(), &file) == 0;
}