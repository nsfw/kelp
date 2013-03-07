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

#include "chipKITUSBHost.h"
#include "chipKITUSBMSDHost.h"

//******************************************************************************
//******************************************************************************
// Thunks to the MSD USB HOST code in the MAL
//******************************************************************************
//******************************************************************************

uint8_t ChipKITUSBMSDHost::DeviceStatus(uint8_t deviceAddress)
{
    return(USBHostMSDDeviceStatus(deviceAddress));
}

BOOL ChipKITUSBMSDHost::EventHandler(uint8_t address, USB_EVENT event, void * data, DWORD size)
{
    return(USBHostMSDEventHandler(address, event, data, size));
}

BOOL ChipKITUSBMSDHost::Initialize(uint8_t address, DWORD flags, uint8_t clientDriverID)
{
    return(USBHostMSDInitialize(address, flags, clientDriverID));
}

uint8_t ChipKITUSBMSDHost::ResetDevice(uint8_t deviceAddress)
{
    return(USBHostMSDResetDevice(deviceAddress));
}

BOOL ChipKITUSBMSDHost::SCSIEventHandler(uint8_t address, USB_EVENT event, void * data, DWORD size)
{
    return(USBHostMSDSCSIEventHandler(address, event, data, size));
}

BOOL ChipKITUSBMSDHost::SCSIInitialize(uint8_t address, DWORD flags, uint8_t clientDriverID)
{
    return(USBHostMSDSCSIInitialize(address, flags, clientDriverID));
}

uint8_t ChipKITUSBMSDHost::SCSISectorRead(DWORD sectorAddress, uint8_t * dataBuffer)
{
    return(USBHostMSDSCSISectorRead(sectorAddress, dataBuffer));
}

uint8_t ChipKITUSBMSDHost::SCSISectorWrite(DWORD sectorAddress, uint8_t * dataBuffer, uint8_t allowWriteToZero)
{
    return(USBHostMSDSCSISectorWrite(sectorAddress,dataBuffer, allowWriteToZero));
}

void ChipKITUSBMSDHost::TerminateTransfer(uint8_t deviceAddress)
{
    USBHostMSDTerminateTransfer(deviceAddress);
}

BOOL ChipKITUSBMSDHost::TransferIsComplete(uint8_t deviceAddress, uint8_t * errorCode, DWORD * byteCount)
{
    return(USBHostMSDTransferIsComplete(deviceAddress, errorCode, byteCount));
}

uint8_t ChipKITUSBMSDHost::Transfer(uint8_t deviceAddress, uint8_t deviceLUN, uint8_t direction, uint8_t * commandBlock, uint8_t commandBlockLength, uint8_t * data, DWORD dataLength)
{
    return(USBHostMSDTransfer(deviceAddress, deviceLUN, direction, commandBlock, commandBlockLength, data, dataLength));
}

uint8_t ChipKITUSBMSDHost::Read(uint8_t deviceAddress, uint8_t deviceLUN, uint8_t * commandBlock, uint8_t commandBlockLength, uint8_t * data, DWORD dataLength )
{
        return(USBHostMSDTransfer(deviceAddress, deviceLUN, 1, commandBlock, commandBlockLength, data, dataLength));
}

uint8_t ChipKITUSBMSDHost::Write(uint8_t deviceAddress, uint8_t deviceLUN, uint8_t * commandBlock, uint8_t commandBlockLength, uint8_t * data, DWORD dataLength )
{
        return(USBHostMSDTransfer(deviceAddress, deviceLUN, 0, commandBlock, commandBlockLength, data, dataLength));
}

//******************************************************************************
//******************************************************************************
// Undocumented yet needed thunks for both the MAL examples in the MDD File System.
//******************************************************************************
//******************************************************************************

void ChipKITUSBMSDHost::Tasks(void)
{
    USBHostMSDTasks();
}

uint8_t ChipKITUSBMSDHost::SCSIWriteProtectState(void)
{
    return(USBHostMSDSCSIWriteProtectState());
}

MEDIA_INFORMATION * ChipKITUSBMSDHost::SCSIMediaInitialize(void)
{
    return(USBHostMSDSCSIMediaInitialize());
}

uint8_t ChipKITUSBMSDHost::SCSIMediaDetect(void)
{
    return(USBHostMSDSCSIMediaDetect());
}

//******************************************************************************
//******************************************************************************
// Instantiate the MSD Class for the sketches
//******************************************************************************
//******************************************************************************
ChipKITUSBMSDHost USBMSDHost;

