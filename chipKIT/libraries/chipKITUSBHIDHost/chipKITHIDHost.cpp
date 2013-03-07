/************************************************************************/
/*																		*/
/*	chipKITUSBHIDHost.cpp	-- USB HID Host Class                       */
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

#include "chipKITUSBHost.h"
#include "chipKITUSBHIDHost.h"

//******************************************************************************
//******************************************************************************
// Thunks to the HID USB HOST code in the MAL
//******************************************************************************
//******************************************************************************

void ChipKITUSBHIDHost::Tasks(void)
{
    USBHostHIDTasks();
}

BOOL ChipKITUSBHIDHost::ApiFindBit(WORD usagePage, WORD usage, HIDReportTypeEnum type, uint8_t* Report_ID, uint8_t* Report_Length, uint8_t* Start_Bit)
{
    return(USBHostHID_ApiFindBit(usagePage, usage, type, Report_ID, Report_Length, Start_Bit));
}

BOOL ChipKITUSBHIDHost::ApiFindValue(WORD usagePage, WORD usage, HIDReportTypeEnum type, uint8_t* Report_ID, uint8_t* Report_Length, uint8_t* Start_Bit, uint8_t* Bit_Length)
{
    return(USBHostHID_ApiFindValue(usagePage, usage, type, Report_ID, Report_Length, Start_Bit, Bit_Length));
}

uint8_t ChipKITUSBHIDHost::ApiGetCurrentInterfaceNum(void)
{
    return(USBHostHID_ApiGetCurrentInterfaceNum());
}

BOOL ChipKITUSBHIDHost::ApiImportData(uint8_t * report, WORD reportLength, HID_USER_DATA_SIZE * buffer, HID_DATA_DETAILS * pDataDetails)
{
    return(USBHostHID_ApiImportData(report, reportLength, buffer, pDataDetails));
}

BOOL ChipKITUSBHIDHost::HasUsage(HID_REPORTITEM * reportItem, WORD usagePage, WORD usage, WORD * pindex, uint8_t* count)
{
    return(USBHostHID_HasUsage(reportItem, usagePage, usage, pindex, count));
}

BOOL ChipKITUSBHIDHost::DeviceDetect(uint8_t deviceAddress)
{
    return(USBHostHIDDeviceDetect(deviceAddress));
}

uint8_t ChipKITUSBHIDHost::DeviceStatus(uint8_t deviceAddress)
{
    return(USBHostHIDDeviceStatus(deviceAddress));
}

BOOL ChipKITUSBHIDHost::Initialize(uint8_t address, DWORD flags, uint8_t clientDriverID)
{
    return(USBHostHIDInitialize(address, flags, clientDriverID));
}

uint8_t ChipKITUSBHIDHost::ResetDevice(uint8_t deviceAddress)
{
    return(USBHostHIDResetDevice(deviceAddress));
}

uint8_t ChipKITUSBHIDHost::TerminateTransfer(uint8_t deviceAddress, uint8_t direction, uint8_t interfaceNum)
{
    return(USBHostHIDTerminateTransfer(deviceAddress, direction, interfaceNum));
}

uint8_t ChipKITUSBHIDHost::Transfer(uint8_t deviceAddress, uint8_t direction, uint8_t interfaceNum, WORD reportid, WORD size, uint8_t * data)
{
    return(USBHostHIDTransfer(deviceAddress, direction, interfaceNum, reportid, size, data));
}
uint8_t ChipKITUSBHIDHost::Read(uint8_t deviceAddress, WORD reportid, uint8_t interfaceNum, WORD size, uint8_t * data)
{
    return(USBHostHIDTransfer(deviceAddress, 1, interfaceNum, reportid, size, data));
}
uint8_t ChipKITUSBHIDHost::Write(uint8_t deviceAddress, WORD reportid, uint8_t interfaceNum, WORD size, uint8_t * data)
{
    return(USBHostHIDTransfer(deviceAddress, 0, interfaceNum, reportid, size, data));
}

BOOL ChipKITUSBHIDHost::TransferIsComplete(uint8_t deviceAddress, uint8_t * errorCode, uint8_t * byteCount)
{
    return(USBHostHIDTransferIsComplete(deviceAddress, errorCode, byteCount));
}

USB_HID_DEVICE_RPT_INFO * ChipKITUSBHIDHost::GetCurrentReportInfo(void)
{
    return(&deviceRptInfo);
}

USB_HID_ITEM_LIST * ChipKITUSBHIDHost::GetItemListPointers(void)
{
    return(&itemListPtrs);
}

//******************************************************************************
//******************************************************************************
// Instantiate the HID Class
//******************************************************************************
//******************************************************************************
ChipKITUSBHIDHost USBHIDHost;

