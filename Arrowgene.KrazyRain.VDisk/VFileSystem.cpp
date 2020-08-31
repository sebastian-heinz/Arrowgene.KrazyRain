#include "VFileSystem.h"

VFileSystem::VFileSystem()
{
}

VCompress* VFileSystem::GetCompress()
{
	return nullptr;
}

VFile* VFileSystem::OpenFile(const char* p_file_path)
{
	return nullptr;
}

int VFileSystem::AddVDisk(const char* p_disk_path)
{
	return 0;
}

int VFileSystem::FileExists(const char* p_file_path)
{
	return 0;
}

int VFileSystem::HasVDisk()
{
	return 0;
}

void VFileSystem::CloseFile(VFile* p_file)
{
}

VFileSystem::~VFileSystem()
{
}
