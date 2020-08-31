#include "VFile.h"

VFile::VFile()
{
}

VFile::~VFile()
{
}

const char* VFile::GetBuffer()
{
	return nullptr;
}

const char* VFile::GetFileName() const
{
	return nullptr;
}

VDisk* VFile::GetDisk() const
{
	return nullptr;
}

VFile& VFile::operator=(const VFile& p_other)
{
	//real = first.real;
	//imaginary = first.imaginary;
	return *this;
}

int VFile::IsCompressed()
{
	return 0;
}

int VFile::IsDirectory()
{
	return 0;
}

int VFile::IsEOF() const
{
	return 0;
}

int VFile::IsFile()
{
	return 0;
}

int VFile::Seek(long p_unknown, int p_unknown1)
{
	return 0;
}

unsigned long VFile::GetCompressSize() const
{
	return 0;
}

unsigned long VFile::GetFileSize() const
{
	return 0;
}

unsigned long VFile::GetPos() const
{
	return 0;
}

unsigned long VFile::Read(void* p_buffer, unsigned long p_count)
{
	return 0;
}
