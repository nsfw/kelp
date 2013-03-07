/************************************************************************/
/*																		*/
/*	chipKITUSBMSDHost.h	-- USB Mass Storage Device Host Class           */
/*                         MSD Host Class thunk layer to the MAL        */
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
/*  Just a class wrapper of the MAL MDS HOST code                       */
/*																		*/
/************************************************************************/
/*  Revision History:													*/
/*																		*/
/*	9/08/2011(KeithV): Created											*/
/*																		*/
/************************************************************************/
#ifndef _CHIPKITUSBMSDHOSTCLASS_H
#define _CHIPKITUSBMSDHOSTCLASS_H

#ifdef __cplusplus
     extern "C"
    {
    #undef BYTE             // Arduino defines BYTE as 0, not what we want for the MAL includes
    #define BYTE uint8_t    // for includes, make BYTE something Arduino will like     
#else
    #define uint8_t BYTE    // in the MAL .C files uint8_t is not defined, but BYTE is correct
#endif

// must have previously included ChipKITUSBHost.h in all .C or .CPP files that included this file
#include "USB/usb_host_msd.h"
#include "USB/usb_host_msd_scsi.h"

#ifdef __cplusplus
    #undef BYTE
    #define BYTE 0      // put this back so Arduino Serial.print(xxx, BYTE) will work.
    }
#endif

#ifdef __cplusplus

    class ChipKITUSBMSDHost 
    {
    private:
    public:

        uint8_t DeviceStatus(uint8_t deviceAddress);
        BOOL EventHandler(uint8_t address, USB_EVENT event, void * data, DWORD size);
        BOOL Initialize(uint8_t address, DWORD flags, uint8_t clientDriverID);
        uint8_t ResetDevice(uint8_t deviceAddress);
        BOOL SCSIEventHandler(uint8_t address, USB_EVENT event, void * data, DWORD size);
        BOOL SCSIInitialize(uint8_t address, DWORD flags, uint8_t clientDriverID);
        uint8_t SCSISectorRead(DWORD sectorAddress, uint8_t * dataBuffer);
        uint8_t SCSISectorWrite(DWORD sectorAddress, uint8_t * dataBuffer, uint8_t allowWriteToZero);
        void TerminateTransfer(uint8_t deviceAddress);
        BOOL TransferIsComplete(uint8_t deviceAddress, uint8_t * errorCode, DWORD * byteCount);
        uint8_t Transfer(uint8_t deviceAddress, uint8_t deviceLUN, uint8_t direction, uint8_t * commandBlock, uint8_t commandBlockLength, uint8_t * data, DWORD dataLength);
        uint8_t Read(uint8_t deviceAddress, uint8_t deviceLUN, uint8_t * commandBlock, uint8_t commandBlockLength, uint8_t * data, DWORD dataLength);
        uint8_t Write(uint8_t deviceAddress, uint8_t deviceLUN, uint8_t * commandBlock, uint8_t commandBlockLength, uint8_t * data, DWORD dataLength);

        // undocumented, yet needed
        void Tasks(void);
        uint8_t SCSIWriteProtectState(void);
        MEDIA_INFORMATION * SCSIMediaInitialize(void);
        uint8_t SCSIMediaDetect(void);
    };

// the pre-instantiated Class for the sketches
extern ChipKITUSBMSDHost USBMSDHost;

#endif
#endif