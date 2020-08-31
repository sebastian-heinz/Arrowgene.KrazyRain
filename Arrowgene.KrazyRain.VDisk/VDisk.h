#ifndef VDISK_H
#define VDISK_H

#include "framework.h"
#include "VCompress.h"
#include "VFile.h"

class VCompress;
class VFile;
class VDISK_API_EXPORT VDisk
{

public:
	VDisk(VCompress* p_compress);
	char* GetCurDir();
	VFile* OpenFile(const char* p_file_name);
	VFile* Search(int p_unknown);
	int AddFile(const char* p_file_name, int p_un);
	int ChangeDir(const char* p_directory_path);
	int IsNameExist(const char* p_file_path);
	int IsOpen();
	int MakeDir(const char* p_directory_name);
	int NewDisk(const char* p_disk_name);
	int OpenDisk(char const* p_disk_name, int p_unknown);
	int OptimizeDisk(char const* p_disk_name);
	int Remove(const char* p_name);
	int Rename(const char* p_name_old, const char* p_name_new);
	int SearchCount();
	void CloseDisk();
	void CloseFile(VFile* p_file);
	~VDisk();
};

#endif