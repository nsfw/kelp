/************************************************************************/
/*																		*/
/*	chipKITMDDFS.cpp	-- Memory Disk Drive File System Class          */
/*                         MAL MDD File System thunk layer              */
/*																		*/
/************************************************************************/
/*	Author: 	Keith Vogel 											*/
/*	Copyright 2011, Digilent Inc.										*/
/************************************************************************/
/*
  This library is free software; you can redistribute it and/or
  modify it under the terms of the GNU Lesser General Public
  License as published by the Free Software Foundation; either
  version 2.1 of the License, or (at your option) any later version.

  This library is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
  Lesser General Public License for more details.

  You should have received a copy of the GNU Lesser General Public
  License along with this library; if not, write to the Free Software
  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/
/************************************************************************/
/*  Module Description: 												*/
/*  Just a class wrapper of the MAL MDD File System code                */
/*																		*/
/************************************************************************/
/*  Revision History:													*/
/*																		*/
/*	9/06/2011(KeithV): Created											*/
/*																		*/
/************************************************************************/

#include "chipKITUSBHost.h"
#include "chipKITUSBMSDHost.h"
#include "chipKITMDDFS.h"

//******************************************************************************
//******************************************************************************
// Thunks to the MDD File System code
//******************************************************************************
//******************************************************************************

int ChipKITMDDFS::Init(void)
{
    return(FSInit());
}

FSFILE * ChipKITMDDFS::fopen(const char * fileName, const char *mode)
{
    return(FSfopen(fileName, mode));
}

int ChipKITMDDFS::fclose(FSFILE *fo)
{
    return(FSfclose(fo));
}

void ChipKITMDDFS::rewind (FSFILE *fo)
{
    FSrewind (fo);
}

size_t ChipKITMDDFS::fread(void *ptr, size_t size, size_t n, FSFILE *stream)
{
    return(FSfread(ptr, size, n, stream));
}

int ChipKITMDDFS::fseek(FSFILE *stream, long offset, int whence)
{
    return(FSfseek(stream, offset, whence));
}

long ChipKITMDDFS::ftell(FSFILE *fo)
{
    return(FSftell(fo));
}

// if I make it feof there is a compile conflict, so make it fEOF to avoid the conflict
int ChipKITMDDFS::fEOF(FSFILE * stream)
{
    return(FSfeof(stream));
}

int ChipKITMDDFS::format(char mode, long int serialNumber, char * volumeID)
{
    return(FSformat(mode, serialNumber, volumeID));
}

int ChipKITMDDFS::attrib(FSFILE * file, unsigned char attributes)
{
    return(FSattrib(file, attributes));
}

int ChipKITMDDFS::rename(const char * fileName, FSFILE * fo)
{
    return(FSrename(fileName, fo));
}

int ChipKITMDDFS::remove(const char * fileName)
{
    return(FSremove(fileName));
}

size_t ChipKITMDDFS::fwrite(const void *ptr, size_t size, size_t n, FSFILE *stream)
{
    return(FSfwrite(ptr, size, n, stream));
}

int ChipKITMDDFS::chdir(char * path)
{
    return(FSchdir(path));
}

char * ChipKITMDDFS::getcwd(char * path, int numbchars)
{
    return(FSgetcwd(path, numbchars));
}

int ChipKITMDDFS::mkdir(char * path)
{
    return(FSmkdir(path));
}

int ChipKITMDDFS::rmdir(char * path, unsigned char rmsubdirs)
{
    return(FSrmdir(path, rmsubdirs));
}

int ChipKITMDDFS::SetClockVars(unsigned int year, unsigned char month, unsigned char day, unsigned char hour, unsigned char minute, unsigned char second)
{
    return(SetClockVars(year, month, day, hour, minute, second));
}

int ChipKITMDDFS::FindFirst(const char * fileName, unsigned int attr, SearchRec * rec)
{
    return(FindFirst(fileName, attr, rec));
}

int ChipKITMDDFS::FindNext(SearchRec * rec)
{
    return(FindNext(rec));
}

int ChipKITMDDFS::error(void)
{
    return(FSerror());
}

int ChipKITMDDFS::CreateMBR(unsigned long firstSector, unsigned long numSectors)
{
    return(FSCreateMBR(firstSector, numSectors));
}

void ChipKITMDDFS::GetDiskProperties(FS_DISK_PROPERTIES* properties)
{
   FSGetDiskProperties(properties);
}

//******************************************************************************
//******************************************************************************
// Instantiate the ChipKITMDDFS Class
//******************************************************************************
//******************************************************************************
ChipKITMDDFS MDDFS;

