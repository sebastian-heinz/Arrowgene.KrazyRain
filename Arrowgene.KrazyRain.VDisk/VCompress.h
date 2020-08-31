#ifndef VCOMPRESS_H
#define VCOMPRESS_H

#include "framework.h"

class VDISK_API_EXPORT VCompress
{
public:
	VCompress(const VCompress& p_compress);
	VCompress();
	bool Compress(unsigned char* p_in_buffer, unsigned long* p_in_count, const unsigned char* p_out_buffer, unsigned long* p_out_count, int p_unknown);
	bool Uncompress(unsigned char* p_in_buffer, unsigned long* p_in_count, const unsigned char* p_out_buffer, unsigned long* p_out_count);
	VCompress& operator= (const VCompress& p_other);
	unsigned long CompressBound(unsigned long p_unknown);
	~VCompress();
};

#endif