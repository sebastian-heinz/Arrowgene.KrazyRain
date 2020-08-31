#include "VCompress.h"

bool VCompress::Compress(unsigned char* p_in_buffer, unsigned long* p_in_count, const unsigned char* p_out_buffer, unsigned long* p_out_count, int p_unknown)
{
	return false;
}

bool VCompress::Uncompress(unsigned char* p_in_buffer, unsigned long* p_in_count, const unsigned char* p_out_buffer, unsigned long* p_out_count)
{
	return false;
}

VCompress& VCompress::operator=(const VCompress& p_other)
{
	//real = first.real;
	//imaginary = first.imaginary;
	return *this;
}

unsigned long VCompress::CompressBound(unsigned long p_unknown)
{
	return 0;
}

VCompress::~VCompress()
{
}
