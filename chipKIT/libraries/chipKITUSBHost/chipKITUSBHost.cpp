/************************************************************************/
/*																		*/
/*	chipKITUSBHost.c	-- USB Host Class                               */
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
/*  Thunks to theUSB HOST controller MAL code.						    */
/*																		*/
/************************************************************************/
/*  Revision History:													*/
/*																		*/
/*	9/07/2011(KeithV): Created											*/
/*																		*/
/************************************************************************/

#include "chipKITUSBHost.h"

static USB_CLIENT_EVENT_HANDLER fUSBHostEventHandler = NULL;

#define MAX_ALLOWED_CURRENT             (500)         // Maximum power we can supply in mA

/****************************************************************************
  Function:
    BOOL USB_ApplicationEventHandler( uint8_t address, USB_EVENT event, void *data, DWORD size )

  Description:
    Handles all of the events for the HID device.
    It calls the event handler set in the ChipKITUSBHost::Begin

  Precondition:
    None

  Parameters:
        address  Address of the USB device generating the event  
        event  Event that occurred  
        data  Optional pointer to data for the event  
        size  Size of the data pointed to by *data  

  Return Values:
    True if handled, false otherwise

  Remarks:
    This is the magic name that the MAL USB host controller code will
    call as the callback. This is set in usb_config.h and the function
    name should not be changed.

***************************************************************************/
BOOL USB_ApplicationEventHandler( uint8_t address, USB_EVENT event, void *data, DWORD size )
{
    if(fUSBHostEventHandler != NULL)
    {
        return(fUSBHostEventHandler(address, event, data, size));
    }

    return(FALSE);
}

/****************************************************************************
  Function:
    USBHostDefaultEventHandler(uint8_t address, USB_EVENT event, void * data, DWORD size)

  Description:
    The default handler, it handles startup and VBUS commands.
 
  Precondition:
    None

  Parameters:
        address  Address of the USB device generating the event  
        event  Event that occurred  
        data  Optional pointer to data for the event  
        size  Size of the data pointed to by *data  

  Return Values:
    True if handled, false otherwise

  Remarks:
    This is not required to use, but might be helpful to handle common events

***************************************************************************/
BOOL USBHostDefaultEventHandler(uint8_t address, USB_EVENT event, void * data, DWORD size)
{
    switch( event )
    {
        case EVENT_VBUS_REQUEST_POWER:
            // The data pointer points to a byte that represents the amount of power
            // requested in mA, divided by two.  If the device wants too much power,
            // we reject it.
            if (((USB_VBUS_POWER_EVENT_DATA*)data)->current <= (MAX_ALLOWED_CURRENT / 2))
            {
                LATBbits.LATB5 = TRUE;  // turn on the power to the USB port
                return TRUE;
            }
            break;

        case EVENT_VBUS_RELEASE_POWER:
            // Turn off Vbus power.
            LATBbits.LATB5 = FALSE;
            return TRUE;
            break;

        case EVENT_HUB_ATTACH:
            return TRUE;
            break;

        case EVENT_UNSUPPORTED_DEVICE:
            return TRUE;
            break;

        case EVENT_CANNOT_ENUMERATE:
            return TRUE;
            break;

        case EVENT_CLIENT_INIT_ERROR:
            return TRUE;
            break;

        case EVENT_OUT_OF_MEMORY:
            return TRUE;
            break;

        case EVENT_UNSPECIFIED_ERROR:   // This should never be generated.
            return TRUE;
            break;

        default:
            break;
    }
    return FALSE;
}

/****************************************************************************
  Function:
    void ChipKITUSBHost::Begin(void)

  Description:
    Initializes the USB Host subsystem and sets the event handler
 
  Precondition:
    None

  Parameters:
    None
  Return Values:
    True if handled, false otherwise

  Remarks:
    Sets USBHostDefaultEventHandler as the event handler

***************************************************************************/
void ChipKITUSBHost::Begin(void)
{
    Begin(USBHostDefaultEventHandler);
}

/****************************************************************************
  Function:
    void ChipKITUSBHost::Begin(USB_CLIENT_EVENT_HANDLER fEventHandler)

  Description:
    Initializes the USB Host subsystem and sets the event handler
 
  Precondition:
    None

  Parameters:
    fEventHandler - the event handler to use; the host controller will call this when an event comes in from the device

  Return Values:
    True if handled, false otherwise

  Remarks:
    Sets the event handler to that specifed by fEventHandler

***************************************************************************/
void ChipKITUSBHost::Begin(USB_CLIENT_EVENT_HANDLER fEventHandler)
{
    int  value;
    
    fUSBHostEventHandler = fEventHandler;

    // turn power on to the USB Host connector
    TRISBbits.TRISB5=0;
    // LATBbits.LATB5 = TRUE;
    LATBbits.LATB5 = FALSE;  // turn it off as we will read the VBUS REQUEST EVENT.
   
    // Initialize USB layers
    USBHost.Init(0);
}

//******************************************************************************
//******************************************************************************
// Thunks to the underlying USB HOST MAL code
//******************************************************************************
//******************************************************************************
void ChipKITUSBHost::Tasks()
{
    USBHostTasks();
}

BOOL ChipKITUSBHost::DefaultEventHandler(uint8_t address, USB_EVENT event, void * data, DWORD size)
{
    return(USBHostDefaultEventHandler(address, event, data, size));
}

uint8_t ChipKITUSBHost::ClearEndpointErrors(uint8_t deviceAddress, uint8_t endpoint)
{
    return(USBHostClearEndpointErrors(deviceAddress, endpoint));
}

BOOL ChipKITUSBHost::DeviceSpecificClientDriver(uint8_t deviceAddress)
{
    return(USBHostDeviceSpecificClientDriver(deviceAddress));
}

uint8_t ChipKITUSBHost::DeviceStatus(uint8_t deviceAddress)
{
    return(USBHostDeviceStatus(deviceAddress));
}

uint8_t * ChipKITUSBHost::GetCurrentConfigurationDescriptor(uint8_t deviceAddress)
{
    return(USBHostGetCurrentConfigurationDescriptor(deviceAddress));
}

uint8_t * ChipKITUSBHost::GetDeviceDescriptor(uint8_t deviceAddress)
{
    return(USBHostGetDeviceDescriptor(deviceAddress));
}

uint8_t ChipKITUSBHost::GetStringDescriptor(uint8_t deviceAddress, WORD stringNumber, WORD LangID, uint8_t * stringDescriptor, WORD stringLength, uint8_t clientDriverID)
{
    return(USBHostGetStringDescriptor(deviceAddress, stringNumber,LangID, stringDescriptor, stringLength, clientDriverID));
}

BOOL ChipKITUSBHost::Init(unsigned long flags)
{
    return(USBHostInit(flags));
}

uint8_t ChipKITUSBHost::Read(uint8_t deviceAddress, uint8_t endpoint, uint8_t * data, DWORD size)
{
    return(USBHostRead(deviceAddress, endpoint, data, size));
}

uint8_t ChipKITUSBHost::ResetDevice(uint8_t deviceAddress)
{
    return(USBHostResetDevice(deviceAddress));
}

uint8_t ChipKITUSBHost::ResumeDevice(uint8_t deviceAddress)
{
    return(USBHostResumeDevice(deviceAddress));
}

uint8_t ChipKITUSBHost::SetDeviceConfiguration(uint8_t deviceAddress, uint8_t configuration)
{
    return(USBHostSetDeviceConfiguration(deviceAddress, configuration));
}

uint8_t ChipKITUSBHost::SetNAKTimeout(uint8_t deviceAddress, uint8_t endpoint, WORD flags, WORD timeoutCount)
{
    return(USBHostSetNAKTimeout(deviceAddress, endpoint, flags, timeoutCount));
}

uint8_t ChipKITUSBHost::SuspendDevice(uint8_t deviceAddress)
{
    return(USBHostSuspendDevice(deviceAddress));
}

void ChipKITUSBHost::TerminateTransfer(uint8_t deviceAddress, uint8_t endpoint)
{
    USBHostTerminateTransfer(deviceAddress, endpoint);
}

BOOL ChipKITUSBHost::TransferIsComplete(uint8_t deviceAddress, uint8_t endpoint, uint8_t * errorCode, DWORD * byteCount)
{
    return(USBHostTransferIsComplete(deviceAddress, endpoint, errorCode, byteCount));
}

uint8_t ChipKITUSBHost::VbusEvent(USB_EVENT vbusEvent, uint8_t hubAddress, uint8_t portNumber)
{
    return(USBHostVbusEvent(vbusEvent, hubAddress, portNumber));
}

uint8_t ChipKITUSBHost::Write(uint8_t deviceAddress, uint8_t endpoint, uint8_t * data, DWORD size)
{
    return(USBHostWrite(deviceAddress, endpoint, data, size));
}

//******************************************************************************
//******************************************************************************
// Instantiate the Host Class
//******************************************************************************
//******************************************************************************
ChipKITUSBHost USBHost;
