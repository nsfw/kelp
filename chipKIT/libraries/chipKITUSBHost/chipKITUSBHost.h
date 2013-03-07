/************************************************************************/
/*																		*/
/*	chipKITUSBHost.h	-- USB Host Class                               */
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
/*  Header file for the USB HOST controller code.						*/
/*																		*/
/************************************************************************/
/*  Revision History:													*/
/*																		*/
/*	9/07/2011(KeithV): Created											*/
/*																		*/
/************************************************************************/
#ifndef _CHIPKITUSBHOSTCLASS_H
#define _CHIPKITUSBHOSTCLASS_H

#ifdef __cplusplus
    #include "WProgram.h"
    extern "C"
    {
    #undef BYTE             // Arduino defines BYTE as 0, not what we want for the MAL includes
    #define BYTE uint8_t    // for includes, make BYTE something Arduino will like     
#else
    #define uint8_t BYTE    // in the MAL .C files uint8_t is not defined, but BYTE is correct
#endif

#include <stdlib.h>
#include <string.h>
#include <stdio.h>
#include "GenericTypeDefs.h"
#include "HardwareProfile.h"
#include "usb_config.h"
#include "USB/usb.h"

#ifdef __cplusplus
    #undef BYTE
    #define BYTE 0      // put this back so Arduino Serial.print(xxx, BYTE) will work.
    }
#endif

#ifdef __cplusplus

    class ChipKITUSBHost 
    {

    private:
 
    public:
        void Begin(void);
        void Begin(USB_CLIENT_EVENT_HANDLER fEventHandler);
        BOOL DefaultEventHandler(uint8_t address, USB_EVENT event, void * data, DWORD size);
        void Tasks();
        uint8_t ClearEndpointErrors(uint8_t deviceAddress, uint8_t endpoint);
        BOOL DeviceSpecificClientDriver(uint8_t deviceAddress);
        uint8_t DeviceStatus(uint8_t deviceAddress);
        uint8_t * GetCurrentConfigurationDescriptor(uint8_t deviceAddress);
        uint8_t * GetDeviceDescriptor(uint8_t deviceAddress);
        uint8_t GetStringDescriptor(uint8_t deviceAddress, WORD stringNumber, WORD LangID, uint8_t * stringDescriptor, WORD stringLength, uint8_t clientDriverID);
        BOOL Init(unsigned long flags);
        uint8_t Read(uint8_t deviceAddress, uint8_t endpoint, uint8_t * data, DWORD size);
        uint8_t ResetDevice(uint8_t deviceAddress);
        uint8_t ResumeDevice(uint8_t deviceAddress);
        uint8_t SetDeviceConfiguration(uint8_t deviceAddress, uint8_t configuration);
        uint8_t SetNAKTimeout(uint8_t deviceAddress, uint8_t endpoint, WORD flags, WORD timeoutCount);
        uint8_t SuspendDevice(uint8_t deviceAddress);
        void TerminateTransfer(uint8_t deviceAddress, uint8_t endpoint);
        BOOL TransferIsComplete(uint8_t deviceAddress, uint8_t endpoint, uint8_t * errorCode, DWORD * byteCount);
        uint8_t VbusEvent(USB_EVENT vbusEvent, uint8_t hubAddress, uint8_t portNumber);
        uint8_t Write(uint8_t deviceAddress, uint8_t endpoint, uint8_t * data, DWORD size);

    };

// pre-instantiated class for the sketches
extern ChipKITUSBHost USBHost;

#endif
#endif