#ifndef VFILESYSTEM_H
#define VFILESYSTEM_H

#include "framework.h"
#include "VCompress.h"
#include "VFile.h"

class VDISK_API_EXPORT VFileSystem
{
public:
	VFileSystem();
	VCompress* GetCompress();
	VFile* OpenFile(const char* p_file_path);
	int AddVDisk(const char* p_disk_path);
	int FileExists(const char* p_file_path);
	int HasVDisk();
	void CloseFile(VFile* p_file);
	~VFileSystem();
};

#endif