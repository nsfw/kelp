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
#ifndef _CHIPKITUSBMDDFSCLASS_H
#define _CHIPKITUSBMDDFSCLASS_H

#ifdef __cplusplus
//     extern "C"
//    {
    #undef BYTE             // Arduino defines BYTE as 0, not what we want for the MAL includes
    #define BYTE uint8_t    // for includes, make BYTE something Arduino will like     
#else
    #define uint8_t BYTE    // in the MAL .C files uint8_t is not defined, but BYTE is correct
#endif

// must have previously included ChipKITUSBHost.h and ChipKITUSBMSDHost in all .C or .CPP files that included this file
#include "MDD File System/FSIO.h"
#include "MDD File System/FSDefs.h"

#ifdef __cplusplus
    #undef BYTE
    #define BYTE 0      // put this back so Arduino Serial.print(xxx, BYTE) will work.
//    }
#endif

#ifdef __cplusplus

    class ChipKITMDDFS
    {
    private:
    public:
        int Init(void);
        FSFILE * fopen(const char * fileName, const char *mode);
        int fclose(FSFILE *fo);
        void rewind(FSFILE *fo);
        size_t fread(void *ptr, size_t size, size_t n, FSFILE *stream);
        int fseek(FSFILE *stream, long offset, int whence);
        long ftell(FSFILE *fo);
        int fEOF(FSFILE * stream);
        int format(char mode, long int serialNumber, char * volumeID);
        int attrib(FSFILE * file, unsigned char attributes);
        int rename(const char * fileName, FSFILE * fo);
        int remove(const char * fileName);
        size_t fwrite(const void *ptr, size_t size, size_t n, FSFILE *stream);
        int chdir(char * path);
        char * getcwd(char * path, int numbchars);
        int mkdir(char * path);
        int rmdir(char * path, unsigned char rmsubdirs);
        int SetClockVars(unsigned int year, unsigned char month, unsigned char day, unsigned char hour, unsigned char minute, unsigned char second);
        int FindFirst(const char * fileName, unsigned int attr, SearchRec * rec);
        int FindNext(SearchRec * rec); 
        int error(void);
        int CreateMBR(unsigned long firstSector, unsigned long numSectors);
        void GetDiskProperties(FS_DISK_PROPERTIES* properties);
    };

// pre-instantiated class for sketches
extern ChipKITMDDFS MDDFS;

#endif
#endif