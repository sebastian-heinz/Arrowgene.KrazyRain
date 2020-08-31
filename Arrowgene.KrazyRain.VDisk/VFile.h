#ifndef VFILE_H
#define VFILE_H

#include "framework.h"
#include "VDisk.h"

class VDisk;
class VDISK_API_EXPORT VFile
{

public:
	VFile();
	~VFile();
	const char* GetBuffer();
	const char* GetFileName() const;
	VDisk* GetDisk() const;
	VFile& operator= (const VFile& p_other);
	int IsCompressed();
	int IsDirectory();
	int IsEOF() const;
	int IsFile();
	int Seek(long p_unknown, int p_unknown1);
	unsigned long GetCompressSize() const;
	unsigned long GetFileSize() const;
	unsigned long GetPos() const;
	unsigned long Read(void* p_buffer, unsigned long p_count);
};


#endif

