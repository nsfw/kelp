/************************************************************************/
/*																		*/
/*	chipKITUSBHIDHost.h	-- USB HID Host Class                           */
/*                         HID Host Class thunk layer to the MAL        */
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
/*  Just a class wrapper of the MAL HID HOST code                       */
/*																		*/
/************************************************************************/
/*  Revision History:													*/
/*																		*/
/*	9/06/2011(KeithV): Created											*/
/*																		*/
/************************************************************************/
#ifndef _CHIPKITUSBHIDHOSTCLASS_H
#define _CHIPKITUSBHIDHOSTCLASS_H

#ifdef __cplusplus
     extern "C"
    {
    #undef BYTE             // Arduino defines BYTE as 0, not what we want for the MAL includes
    #define BYTE uint8_t    // for includes, make BYTE something Arduino will like     
#else
    #define uint8_t BYTE    // in the MAL .C files uint8_t is not defined, but BYTE is correct
#endif

// must have previously included ChipKITUSBHost.h in all .C or .CPP files that included this file
#include "USB/usb_host_hid_parser.h"
#include "USB/usb_host_hid.h"

#ifdef __cplusplus
    #undef BYTE
    #define BYTE 0      // put this back so Arduino Serial.print(xxx, BYTE) will work.
    }
#endif

#ifdef __cplusplus

    class ChipKITUSBHIDHost 
    {
    private:
    public:

        void Tasks(void);
        BOOL ApiFindBit(WORD usagePage, WORD usage, HIDReportTypeEnum type, uint8_t* Report_ID, uint8_t* Report_Length, uint8_t* Start_Bit);
        BOOL ApiFindValue(WORD usagePage, WORD usage, HIDReportTypeEnum type, uint8_t* Report_ID, uint8_t* Report_Length, uint8_t* Start_Bit, uint8_t* Bit_Length);
        uint8_t ApiGetCurrentInterfaceNum(void);
        BOOL ApiImportData(uint8_t * report, WORD reportLength, HID_USER_DATA_SIZE * buffer, HID_DATA_DETAILS * pDataDetails);
        BOOL HasUsage(HID_REPORTITEM * reportItem, WORD usagePage, WORD usage, WORD * pindex, uint8_t* count);
        BOOL DeviceDetect(uint8_t deviceAddress);
        uint8_t DeviceStatus(uint8_t deviceAddress);
        BOOL Initialize(uint8_t address, DWORD flags, uint8_t clientDriverID);
        uint8_t ResetDevice(uint8_t deviceAddress);
        uint8_t TerminateTransfer(uint8_t deviceAddress, uint8_t direction, uint8_t interfaceNum);
        uint8_t Transfer(uint8_t deviceAddress, uint8_t direction, uint8_t interfaceNum, WORD reportid, WORD size, uint8_t * data);
        BOOL TransferIsComplete(uint8_t deviceAddress, uint8_t * errorCode, uint8_t * byteCount);
        uint8_t Read(uint8_t deviceAddress, WORD reportid, uint8_t interfaceNum, WORD size, uint8_t * data);
        uint8_t Write(uint8_t deviceAddress, WORD reportid, uint8_t interfaceNum, WORD size, uint8_t * data);
        USB_HID_DEVICE_RPT_INFO * GetCurrentReportInfo(void);
        USB_HID_ITEM_LIST * GetItemListPointers(void);
    };

// pre-instantiated Class for the sketches
extern ChipKITUSBHIDHost USBHIDHost;

#endif
#endif