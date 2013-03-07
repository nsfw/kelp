/************************************************************************/
/*																		*/
/*	chipKITUSBDevice.h	-- USB interface APIs to implement the          */
/*                  Arduino software compatible USB Device Library      */
/*					using the chipKIT Max32 and chipKIT Network Shield	*/
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
/*																		*/
/*	This library is explicity targeting the chipKIT Max32 				*/
/*	PIC32MX795F512L MCU using chipKIT Network Shield 			        */
/*	It exposes the Microchip USB Device Library	for use with MPIDE      */		
/*	use with MPIDE sketches												*/
/*																		*/
/************************************************************************/
/*  Revision History:													*/
/*																		*/
/*	8/08/2011(KeithV): Created											*/
/*	9/13/2011(KeithV): Updated to not break BYTE in Arduino Serial.print*/
/*																		*/
/************************************************************************/

#ifndef USBDevice_h
#define USBDevice_h

#ifdef __cplusplus
    #include "WProgram.h"
    extern "C"
    {
    #undef BYTE             // Arduino defines BYTE as 0, not what we want for the MAL includes
    #define BYTE uint8_t    // for includes, make BYTE something Arduino will like     
#else
    #define uint8_t BYTE    // in the MAL .C files uint8_t is not defined, but BYTE is correct
#endif

#define USB_SUPPORT_DEVICE
#define ROM 
#include "GenericTypeDefs.h"
#include "USB/usb_common.h"
#include "USB/usb_ch9.h"
#include "USB/usb_device.h"
#include "USB/usb_function_generic.h"
#include "USB/usb_hal_pic32.h"

	void ChipKITUSBSetUSBCallbackEventHandler(boolean (* PUSER_USB_CALLBACK_EVENT_HANDLER)(USB_EVENT event, void *pdata, word size));
    void ChipKITInitializeSystem(boolean fWaitUntilConfigured); 
    boolean DEFAULT_USER_USB_CALLBACK_EVENT_HANDLER(USB_EVENT event, void *pdata, word size); 
    void ChipKITUSBDeviceTasks(void);
    void USBDeviceInit(void);
    boolean ChipKITUSBGetSuspendState(void);
    void USBEnableEndpoint(uint8_t ep, uint8_t options);
    USB_HANDLE USBTransferOnePacket(uint8_t ep,uint8_t dir,uint8_t* data, uint8_t len);
    void USBStallEndpoint(uint8_t ep, uint8_t dir);
    void USBCancelIO(uint8_t endpoint);     
    boolean ChipKITUSBGetRemoteWakeupStatus(void);
    USB_DEVICE_STATE ChipKITUSBGetDeviceState(void);
    boolean ChipKITUSBIsDeviceSuspended(void);
    void ChipKITUSBSoftDetach(void);
    boolean ChipKITUSBHandleBusy(USB_HANDLE handle);
    word ChipKITUSBHandleGetLength(USB_HANDLE handle);
    void * ChipKITUSBHandleGetAddr(USB_HANDLE handle);
    void ChipKITUSBEP0Transmit(uint8_t options);
    void ChipKITUSBEP0SendRAMPtr(uint8_t* src, word size, uint8_t Options);
    void ChipKITUSBEP0SendROMPtr(uint8_t* src, word size, uint8_t Options);
    void ChipKITUSBEP0Receive(uint8_t* dest, word size, void (*function));
    USB_HANDLE ChipKITUSBTxOnePacket(uint8_t ep, uint8_t* data, word len);
    USB_HANDLE ChipKITUSBRxOnePacket(uint8_t ep, uint8_t* data, word len);
    void ChipKITUSBDeviceDetach(void);
    void ChipKITUSBDeviceAttach(void);


#ifdef __cplusplus
    #undef BYTE
    #define BYTE 0      // put this back so Arduino Serial.print(xxx, BYTE) will work.
    }
#endif

class USBDevice
{
  
  public:
      USBDevice(void);
	  USBDevice(boolean (* PUSER_USB_CALLBACK_EVENT_HANDLER)(USB_EVENT event, void *pdata, word size));
      void InitializeSystem(boolean fWaitUntilConfigured);   
      boolean DefaultCBEventHandler(USB_EVENT event, void *pdata, word size); 
      void DeviceTasks(void);
      void DeviceInit(void);
      boolean GetSuspendState(void);
      void EnableEndpoint(uint8_t ep, uint8_t options);
      USB_HANDLE TransferOnePacket(uint8_t ep,uint8_t dir,uint8_t* data, uint8_t len);
      void StallEndpoint(uint8_t ep, uint8_t dir);
      void CancelIO(uint8_t endpoint); 
      boolean GetRemoteWakeupStatus(void);
      USB_DEVICE_STATE GetDeviceState(void);
      boolean IsDeviceSuspended(void);
      void SoftDetach(void);
      boolean HandleBusy(USB_HANDLE handle);
      word HandleGetLength(USB_HANDLE handle);
      void * HandleGetAddr(USB_HANDLE);
      void EP0Transmit(uint8_t options);
      void EP0SendRAMPtr(uint8_t* src, word size, uint8_t Options);
      void EP0SendROMPtr(uint8_t* src, word size, uint8_t Options);
      void EP0Receive(uint8_t* dest, word size, void (*function));
      USB_HANDLE TxOnePacket(uint8_t ep, uint8_t* data, word len);
      USB_HANDLE GenWrite(uint8_t ep, uint8_t* data, word len);
      USB_HANDLE RxOnePacket(uint8_t ep, uint8_t* data, word len);
      USB_HANDLE GenRead(uint8_t ep, uint8_t* data, word len);
      void DeviceDetach(void);
      void DeviceAttach(void);
  
  private:
    
};

#endif
