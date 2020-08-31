#include "VDisk.h"

VDisk::VDisk(VCompress* p_compress)
{
}

char* VDisk::GetCurDir()
{
	return nullptr;
}

VFile* VDisk::OpenFile(const char* p_file_name)
{
	return nullptr;
}

VFile* VDisk::Search(int p_unknown)
{
	return nullptr;
}

int VDisk::AddFile(const char* p_file_name, int p_un)
{
	return 0;
}

int VDisk::ChangeDir(const char* p_directory_path)
{
	return 0;
}

int VDisk::IsNameExist(const char* p_file_path)
{
	return 0;
}

int VDisk::IsOpen()
{
	return 0;
}

int VDisk::MakeDir(const char* p_directory_name)
{
	return 0;
}

int VDisk::NewDisk(const char* p_disk_name)
{
	return 0;
}

int VDisk::OpenDisk(char const* p_disk_name, int p_unknown)
{
	return 0;
}

int VDisk::OptimizeDisk(char const* p_disk_name)
{
	return 0;
}

int VDisk::Remove(const char* p_name)
{
	return 0;
}

int VDisk::Rename(const char* p_name_old, const char* p_name_new)
{
	return 0;
}

int VDisk::SearchCount()
{
	return 0;
}

void VDisk::CloseDisk()
{
}

void VDisk::CloseFile(VFile* p_file)
{
}

VDisk::~VDisk()
{
}
