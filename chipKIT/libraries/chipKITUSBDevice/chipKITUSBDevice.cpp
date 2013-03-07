/************************************************************************/
/*																		*/
/*	chipKITUSBDevice.cpp	-- USB interface APIs to implement the      */
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
/*																		*/
/************************************************************************/
#include "WProgram.h"
#include "chipKITUSBDevice.h"

/*******************************************************************
 * Function:        USBDevice Constructor
 *
 * PreCondition:    None
 *
 * Output:          None
 *
 * Side Effects:    None
 *
 *
 * Note:            The Default Generic USB callback will be used.
 *******************************************************************/
USBDevice::USBDevice(void)
{
	ChipKITUSBSetUSBCallbackEventHandler(NULL);
}

/*******************************************************************
 * Function:        USBDevice Constructor
 *
 * PreCondition:    None
 *
 * Input:           A pointer to the callback routine.
 *                  The underlying USB stack will call this routine for 
 *                  Device specific callback functions.
 *
 * Output:          None
 *
 * Side Effects:    None
 *
 *
 * Note:            None
 *******************************************************************/
USBDevice::USBDevice(boolean (* PUSER_USB_CALLBACK_EVENT_HANDLER)(USB_EVENT event, void *pdata, word size) )
{
	ChipKITUSBSetUSBCallbackEventHandler(PUSER_USB_CALLBACK_EVENT_HANDLER);
}

/*******************************************************************
 * Function:        boolean DefaultCBEventHandler(
 *                        USB_EVENT event, void *pdata, WORD size)
 *
 * PreCondition:    None
 *
 * Input:           USB_EVENT event - the type of event
 *                  void *pdata - pointer to the event data
 *                  WORD size - size of the event data
 *
 * Output:          None
 *
 * Side Effects:    None
 *
 * Overview:        This function is called from the USB stack to
 *                  notify a user application that a USB event
 *                  occured.  This callback is in interrupt context
 *                  when the USB_INTERRUPT option is selected.
 *
 *                  This is a default handler that will get the USB stack up to the
 *                  configured state but will not handle any Device specific
 *                  events. Unaltered this can only handle the Generic USB Device.
 *
 * Note:            None
 *******************************************************************/
boolean USBDevice::DefaultCBEventHandler(USB_EVENT event, void *pdata, word size)
{
    return(DEFAULT_USER_USB_CALLBACK_EVENT_HANDLER(event, pdata, size));
}
 
/********************************************************************
 * Function:        void InitializeSystem(fWaitUntilCofigured)
 *
 * PreCondition:    None
 *
 * Input:           fWaitUntilCofigured - if true will block until the USB stack has reached 
 *                                          the configured state. Otherwise it will immediately return
 *
 * Output:          None
 *
 * Side Effects:    None
 *
 * Overview:        InitializeSystem is a centralize initialization
 *                  routine. All required USB initialization routines
 *                  are called from here.
 *
 *                  User application initialization routine should
 *                  also be called from here.                  
 *
 * Note:            None
 *******************************************************************/
void USBDevice::InitializeSystem(boolean fWaitUntilConfigured)
{
    ChipKITInitializeSystem(fWaitUntilConfigured);
}   

/**************************************************************************
  Function:
        void USBDeviceTasks(void)
    
  Summary:
    This function is the main state machine of the USB device side stack.
    This function should be called periodically to receive and transmit
    packets through the stack. This function should be called preferably
    once every 100us during the enumeration process. After the enumeration
    process this function still needs to be called periodically to respond
    to various situations on the bus but is more relaxed in its time
    requirements. This function should also be called at least as fast as
    the OUT data expected from the PC.

  Description:
    This function is the main state machine of the USB device side stack.
    This function should be called periodically to receive and transmit
    packets through the stack. This function should be called preferably
    once every 100us during the enumeration process. After the enumeration
    process this function still needs to be called periodically to respond
    to various situations on the bus but is more relaxed in its time
    requirements. This function should also be called at least as fast as
    the OUT data expected from the PC.

    Typical usage:
    <code>
    void main(void)
    {
        USBDeviceInit()
        while(1)
        {
            USBDeviceTasks();
            if((USBGetDeviceState() \< CONFIGURED_STATE) ||
               (USBIsDeviceSuspended() == TRUE))
            {
                //Either the device is not configured or we are suspended
                //  so we don't want to do execute any application code
                continue;   //go back to the top of the while loop
            }
            else
            {
                //Otherwise we are free to run user application code.
                UserApplication();
            }
        }
    }
    </code>

  Conditions:
    None
  Remarks:
    This function should be called preferably once every 100us during the
    enumeration process. After the enumeration process this function still
    needs to be called periodically to respond to various situations on the
    bus but is more relaxed in its time requirements.                      
  **************************************************************************/
void USBDevice::DeviceTasks(void)
{
    ChipKITUSBDeviceTasks();
}

/**************************************************************************
    Function:
        void USBDeviceInit(void)
    
    Description:
        This function initializes the device stack it in the default state. The
        USB module will be completely reset including all of the internal
        variables, registers, and interrupt flags.
                
    Precondition:
        This function must be called before any of the other USB Device
        functions can be called, including USBDeviceTasks().
        
    Parameters:
        None
     
    Return Values:
        None
        
    Remarks:
        None
                                                          
  **************************************************************************/
void USBDevice::DeviceInit(void)
{
    USBDeviceInit();  
}

/***************************************************************************
  Function:
        BOOL USBGetSuspendState(void)
    
  Summary:
    This function indicates if this device is currently suspended. When a
    device is suspended it will not be able to transfer data over the bus.
  Description:
    This function indicates if this device is currently suspended. When a
    device is suspended it will not be able to transfer data over the bus.
    This function can be used by the application to skip over section of
    code that do not need to exectute if the device is unable to send data
    over the bus.
    
    Typical usage:
    <code>
       void main(void)
       {
           USBDeviceInit()
           while(1)
           {
               USBDeviceTasks();
               if((USBGetDeviceState() \< CONFIGURED_STATE) ||
                  (USBIsDeviceSuspended() == TRUE))
               {
                   //Either the device is not configured or we are suspended
                   //  so we don't want to do execute any application code
                   continue;   //go back to the top of the while loop
               }
               else
               {
                   //Otherwise we are free to run user application code.
                   UserApplication();
               }
           }
       }
    </code>
  Conditions:
    None
  Return Values:
    TRUE -   this device is suspended.
    FALSE -  this device is not suspended.
  Remarks:
    None                                                                    
  ***************************************************************************/
boolean USBDevice::GetSuspendState(void)
{
    return(ChipKITUSBGetSuspendState());
}

/*******************************************************************************
  Function:
        void USBEnableEndpoint(BYTE ep, BYTE options)
    
  Summary:
    This function will enable the specified endpoint with the specified
    options
  Description:
    This function will enable the specified endpoint with the specified
    options.
    
    Typical Usage:
    <code>
    void USBCBInitEP(void)
    {
        USBEnableEndpoint(MSD_DATA_IN_EP,USB_IN_ENABLED|USB_OUT_ENABLED|USB_HANDSHAKE_ENABLED|USB_DISALLOW_SETUP);
        USBMSDInit();
    }
    </code>
    
    In the above example endpoint number MSD_DATA_IN_EP is being configured
    for both IN and OUT traffic with handshaking enabled. Also since
    MSD_DATA_IN_EP is not endpoint 0 (MSD does not allow this), then we can
    explicitly disable SETUP packets on this endpoint.
  Conditions:
    None
  Input:
    BYTE ep -       the endpoint to be configured
    BYTE options -  optional settings for the endpoint. The options should
                    be ORed together to form a single options string. The
                    available optional settings for the endpoint. The
                    options should be ORed together to form a single options
                    string. The available options are the following\:
                    * USB_HANDSHAKE_ENABLED enables USB handshaking (ACK,
                      NAK)
                    * USB_HANDSHAKE_DISABLED disables USB handshaking (ACK,
                      NAK)
                    * USB_OUT_ENABLED enables the out direction
                    * USB_OUT_DISABLED disables the out direction
                    * USB_IN_ENABLED enables the in direction
                    * USB_IN_DISABLED disables the in direction
                    * USB_ALLOW_SETUP enables control transfers
                    * USB_DISALLOW_SETUP disables control transfers
                    * USB_STALL_ENDPOINT STALLs this endpoint
  Return:
    None
  Remarks:
    None                                                                                                          
  *****************************************************************************/
void USBDevice::EnableEndpoint(uint8_t ep, uint8_t options)
{
    USBEnableEndpoint(ep, options);
}

/*************************************************************************
  Function:
    USB_HANDLE USBTransferOnePacket(BYTE ep, BYTE dir, BYTE* data, BYTE len)
    
  Summary:
    Transfers a single packet (one transaction) of data on the USB bus.

  Description:
    The USBTransferOnePacket() function prepares a USB endpoint
    so that it may send data to the host (an IN transaction), or 
    receive data from the host (an OUT transaction).  The 
    USBTransferOnePacket() function can be used both to receive	and 
    send data to the host.  This function is the primary API function 
    provided by the USB stack firmware for sending or receiving application 
    data over the USB port.  

    The USBTransferOnePacket() is intended for use with all application 
    endpoints.  It is not used for sending or receiving applicaiton data 
    through endpoint 0 by using control transfers.  Separate API 
    functions, such as USBEP0Receive(), USBEP0SendRAMPtr(), and
    USBEP0SendROMPtr() are provided for this purpose.

    The	USBTransferOnePacket() writes to the Buffer Descriptor Table (BDT)
    entry associated with an endpoint buffer, and sets the UOWN bit, which 
    prepares the USB hardware to allow the transaction to complete.  The 
    application firmware can use the USBHandleBusy() macro to check the 
    status of the transaction, to see if the data has been successfully 
    transmitted yet.


    Typical Usage
    <code>
    //make sure that the we are in the configured state
    if(USBGetDeviceState() == CONFIGURED_STATE)
    {
        //make sure that the last transaction isn't busy by checking the handle
        if(!USBHandleBusy(USBInHandle))
        {
	        //Write the new data that we wish to send to the host to the INPacket[] array
	        INPacket[0] = USEFUL_APPLICATION_VALUE1;
	        INPacket[1] = USEFUL_APPLICATION_VALUE2;
	        //INPacket[2] = ... (fill in the rest of the packet data)
	      
            //Send the data contained in the INPacket[] array through endpoint "EP_NUM"
            USBInHandle = USBTransferOnePacket(EP_NUM,IN_TO_HOST,(BYTE*)&INPacket[0],sizeof(INPacket));
        }
    }
    </code>

  Conditions:
    Before calling USBTransferOnePacket(), the following should be true.
    1.  The USB stack has already been initialized (USBDeviceInit() was called).
    2.  A transaction is not already pending on the specified endpoint.  This
        is done by checking the previous request using the USBHandleBusy() 
        macro (see the typical usage example).
    3.  The host has already sent a set configuration request and the 
        enumeration process is complete.
        This can be checked by verifying that the USBGetDeviceState() 
        macro returns "CONFIGURED_STATE", prior to calling 
        USBTransferOnePacket().
 					
  Input:
    BYTE ep - The endpoint number that the data will be transmitted or 
	          received on
    BYTE dir - The direction of the transfer
               This value is either OUT_FROM_HOST or IN_TO_HOST
    BYTE* data - For IN transactions: pointer to the RAM buffer containing 
                 the data to be sent to the host.  For OUT transactions: pointer
                 to the RAM buffer that the received data should get written to.
   BYTE len - Length of the data needing to be sent (for IN transactions).
              For OUT transactions, the len parameter should normally be set
              to the endpoint size specified in the endpoint descriptor.    

  Return Values:
    USB_HANDLE - handle to the transfer.  The handle is a pointer to 
                 the BDT entry associated with this transaction.  The
                 status of the transaction (ex: if it is complete or still
                 pending) can be checked using the USBHandleBusy() macro
                 and supplying the USB_HANDLE provided by
                 USBTransferOnePacket().

  Remarks:
    If calling the USBTransferOnePacket() function from within the USBCBInitEP()
    callback function, the set configuration is still being processed and the
    USBDeviceState may not be == CONFIGURED_STATE yet.  In this	special case, 
    the USBTransferOnePacket() may still be called, but make sure that the 
    endpoint has been enabled and initialized by the USBEnableEndpoint() 
    function first.  
    
  *************************************************************************/
USB_HANDLE USBDevice::TransferOnePacket(uint8_t ep,uint8_t dir,uint8_t* data, uint8_t len)
{
    return(USBTransferOnePacket(ep, dir, data, len));
}

/********************************************************************
    Function:
        void USBStallEndpoint(BYTE ep, BYTE dir)
        
    Summary:
         STALLs the specified endpoint
    
    PreCondition:
        None
        
    Parameters:
        BYTE ep - the endpoint the data will be transmitted on
        BYTE dir - the direction of the transfer
        
    Return Values:
        None
        
    Remarks:
        None

 *******************************************************************/
void USBDevice::StallEndpoint(uint8_t ep, uint8_t dir)
{
    USBStallEndpoint(ep, dir);
}

/**************************************************************************
    Function:
        void USBCancelIO(BYTE endpoint)
    
    Description:
        This function cancels the transfers pending on the specified endpoint.
        This function can only be used after a SETUP packet is received and 
        before that setup packet is handled.  This is the time period in which
        the EVENT_EP0_REQUEST is thrown, before the event handler function
        returns to the stack.

    Precondition:
  
    Parameters:
        BYTE endpoint - the endpoint number you wish to cancel the transfers for
     
    Return Values:
        None
        
    Remarks:
        None
                                                          
  **************************************************************************/
void USBDevice::CancelIO(uint8_t endpoint) 
{
    USBCancelIO(endpoint);
}

/********************************************************************
  Function:
        BOOL USBGetRemoteWakeupStatus(void)
    
  Summary:
    This function indicates if remote wakeup has been enabled by the host.
    Devices that support remote wakeup should use this function to
    determine if it should send a remote wakeup.

  Description:
    This function indicates if remote wakeup has been enabled by the host.
    Devices that support remote wakeup should use this function to
    determine if it should send a remote wakeup.
    
    If a device does not support remote wakeup (the Remote wakeup bit, bit
    5, of the bmAttributes field of the Configuration descriptor is set to
    1), then it should not send a remote wakeup command to the PC and this
    function is not of any use to the device. If a device does support
    remote wakeup then it should use this function as described below.
    
    If this function returns FALSE and the device is suspended, it should
    not issue a remote wakeup (resume).
    
    If this function returns TRUE and the device is suspended, it should
    issue a remote wakeup (resume).
    
    A device can add remote wakeup support by having the _RWU symbol added
    in the configuration descriptor (located in the usb_descriptors.c file
    in the project). This done in the 8th byte of the configuration
    descriptor. For example:

    <code lang="c">
    ROM BYTE configDescriptor1[]={
        0x09,                           // Size 
        USB_DESCRIPTOR_CONFIGURATION,   // descriptor type 
        DESC_CONFIG_WORD(0x0022),       // Total length 
        1,                              // Number of interfaces 
        1,                              // Index value of this cfg 
        0,                              // Configuration string index 
        _DEFAULT | _SELF | _RWU,        // Attributes, see usb_device.h 
        50,                             // Max power consumption in 2X mA(100mA)
        
        //The rest of the configuration descriptor should follow
    </code>

    For more information about remote wakeup, see the following section of
    the USB v2.0 specification available at www.usb.org: 
        * Section 9.2.5.2
        * Table 9-10 
        * Section 7.1.7.7 
        * Section 9.4.5

  Conditions:
    None

  Return Values:
    TRUE -   Remote Wakeup has been enabled by the host
    FALSE -  Remote Wakeup is not currently enabled

  Remarks:
    None
                                                                                                                                                                                                                                                                                                                       
  *******************************************************************/
boolean USBDevice::GetRemoteWakeupStatus(void)
{
    return(ChipKITUSBGetRemoteWakeupStatus());
}

/***************************************************************************
  Function:
        USB_DEVICE_STATE USBGetDeviceState(void)
    
  Summary:
    This function will return the current state of the device on the USB.
    This function should return CONFIGURED_STATE before an application
    tries to send information on the bus.
  Description:
    This function returns the current state of the device on the USB. This
    \function is used to determine when the device is ready to communicate
    on the bus. Applications should not try to send or receive data until
    this function returns CONFIGURED_STATE.
    
    It is also important that applications yield as much time as possible
    to the USBDeviceTasks() function as possible while the this function
    \returns any value between ATTACHED_STATE through CONFIGURED_STATE.
    
    For more information about the various device states, please refer to
    the USB specification section 9.1 available from www.usb.org.
    
    Typical usage:
    <code>
    void main(void)
    {
        USBDeviceInit()
        while(1)
        {
            USBDeviceTasks();
            if((USBGetDeviceState() \< CONFIGURED_STATE) ||
               (USBIsDeviceSuspended() == TRUE))
            {
                //Either the device is not configured or we are suspended
                //  so we don't want to do execute any application code
                continue;   //go back to the top of the while loop
            }
            else
            {
                //Otherwise we are free to run user application code.
                UserApplication();
            }
        }
    }
    </code>
  Conditions:
    None
  Return Values:
    USB_DEVICE_STATE - the current state of the device on the bus

  Remarks:
    None                                                                    
  ***************************************************************************/
USB_DEVICE_STATE USBDevice::GetDeviceState(void)
{
    return(ChipKITUSBGetDeviceState());
}

/*******************************************************************************
  Function:
        BOOL USBIsDeviceSuspended(void)
    
  Summary:
    This function indicates if the USB module is in suspend mode.

  Description:
    This function indicates if the USB module is in suspend mode.  This function
    does NOT indicate that a suspend request has been received.  It only
    reflects the state of the USB module.
   
    Typical Usage:
    <code>
    if(USBIsDeviceSuspended() == TRUE)
    {
        return;
    }
    // otherwise do some application specific tasks
    </code>
    
  Conditions:
    None
  Input:
    None
  Return:
    None
  Remarks:
    None                                                                                                          
  *****************************************************************************/
boolean USBDevice::IsDeviceSuspended(void)
{
    return(ChipKITUSBIsDeviceSuspended());
}

/*******************************************************************************
  Function:
        void USBSoftDetach(void);
    
  Summary:
    This function performs a detach from the USB bus via software.

  Description:
    This function performs a detach from the USB bus via software.
    
  Conditions:
    None
  Input:
    None
  Return:
    None
  Remarks:
    Caution should be used when detaching from the bus.  Some PC drivers and 
    programs may require additional time after a detach before a device can be 
    reattached to the bus.                                                                                                          
  *****************************************************************************/
void USBDevice::SoftDetach(void)
{
    ChipKITUSBSoftDetach();
}

/*************************************************************************
  Function:
    BOOL USBHandleBusy(USB_HANDLE handle)
    
  Summary:
    Checks to see if the input handle is busy

  Description:
    Checks to see if the input handle is busy

    Typical Usage
    <code>
    //make sure that the last transfer isn't busy by checking the handle
    if(!USBHandleBusy(USBGenericInHandle))
    {
        //Send the data contained in the INPacket[] array out on
        //  endpoint USBGEN_EP_NUM
        USBGenericInHandle = USBGenWrite(USBGEN_EP_NUM,(BYTE*)&INPacket[0],sizeof(INPacket));
    }
    </code>

  Conditions:
    None
  Input:
    USB_HANDLE handle -  handle of the transfer that you want to check the
                         status of
  Return Values:
    TRUE -   The specified handle is busy
    FALSE -  The specified handle is free and available for a transfer
  Remarks:
    None                                                                  
  *************************************************************************/
boolean USBDevice::HandleBusy(USB_HANDLE handle)
{
    return(ChipKITUSBHandleBusy(handle));
}

/********************************************************************
    Function:
        WORD USBHandleGetLength(USB_HANDLE handle)
        
    Summary:
        Retrieves the length of the destination buffer of the input
        handle
        
    Description:
        Retrieves the length of the destination buffer of the input
        handle

    PreCondition:
        None
        
    Parameters:
        USB_HANDLE handle - the handle to the transfer you want the
        address for.
        
    Return Values:
        WORD - length of the current buffer that the input handle
        points to.  If the transfer is complete then this is the 
        length of the data transmitted or the length of data
        actually received.
        
    Remarks:
        None
 
 *******************************************************************/
word USBDevice::HandleGetLength(USB_HANDLE handle)
{
    return(ChipKITUSBHandleGetLength(handle));
}

/********************************************************************
    Function:
        WORD USBHandleGetAddr(USB_HANDLE)
        
    Summary:
        Retrieves the address of the destination buffer of the input
        handle
        
    Description:
        Retrieves the address of the destination buffer of the input
        handle

    PreCondition:
        None
        
    Parameters:
        USB_HANDLE handle - the handle to the transfer you want the
        address for.
        
    Return Values:
        WORD - address of the current buffer that the input handle
        points to.
       
    Remarks:
        None
 
 *******************************************************************/
void * USBDevice::HandleGetAddr(USB_HANDLE handle)
{
    return(ChipKITUSBHandleGetAddr(handle));
}

/********************************************************************
    Function:
        void USBEP0Transmit(BYTE options)
        
    Summary:
        Sets the address of the data to send over the
        control endpoint
        
    PreCondition:
        None
        
    Paramters:
        options - the various options that you want
                  when sending the control data. Options are:
                       USB_EP0_ROM
                       USB_EP0_RAM
                       USB_EP0_BUSY
                       USB_EP0_INCLUDE_ZERO
                       USB_EP0_NO_DATA
                       USB_EP0_NO_OPTIONS
                       
    Return Values:
        None
    
    Remarks:
        None
 
 *******************************************************************/
void USBDevice::EP0Transmit(uint8_t options)
{
    ChipKITUSBEP0Transmit(options);
}

/*************************************************************************
  Function:
        void USBEP0SendRAMPtr(BYTE* src, WORD size, BYTE Options)
    
  Summary:
    Sets the source, size, and options of the data you wish to send from a
    RAM source
  Conditions:
    None
  Input:
    src -      address of the data to send
    size -     the size of the data needing to be transmitted
    options -  the various options that you want when sending the control
               data. Options are\:
               * USB_EP0_ROM
               * USB_EP0_RAM
               * USB_EP0_BUSY
               * USB_EP0_INCLUDE_ZERO
               * USB_EP0_NO_DATA
               * USB_EP0_NO_OPTIONS
  Remarks:
    None                                                                  
  *************************************************************************/
void USBDevice::EP0SendRAMPtr(uint8_t* src, word size, uint8_t Options)
{
    ChipKITUSBEP0SendRAMPtr(src, size, Options);
}

/**************************************************************************
  Function:
        void USBEP0SendROMPtr(BYTE* src, WORD size, BYTE Options)
    
  Summary:
    Sets the source, size, and options of the data you wish to send from a
    ROM source
  Conditions:
    None
  Input:
    src -      address of the data to send
    size -     the size of the data needing to be transmitted
    options -  the various options that you want when sending the control
               data. Options are\:
               * USB_EP0_ROM
               * USB_EP0_RAM
               * USB_EP0_BUSY
               * USB_EP0_INCLUDE_ZERO
               * USB_EP0_NO_DATA
               * USB_EP0_NO_OPTIONS
  Remarks:
    None                                                                   
  **************************************************************************/
void USBDevice::EP0SendROMPtr(uint8_t* src, word size, uint8_t Options)
{
    ChipKITUSBEP0SendROMPtr(src, size, Options);
}

/***************************************************************************
  Function:
    void USBEP0Receive(BYTE* dest, WORD size, void (*function))
  Summary:
    Sets the destination, size, and a function to call on the completion of
    the next control write.
  Conditions:
    None
  Input:
    dest -        address of where the incoming data will go (make sure that this
                  address is directly accessable by the USB module for parts with
                  dedicated USB RAM this address must be in that space)
    size -        the size of the data being received (is almost always going tobe
                  presented by the preceeding setup packet SetupPkt.wLength)
    (*function) - a function that you want called once the data is received. If
                  this is specificed as NULL then no function is called.
  Remarks:
    None                                                                    
  ***************************************************************************/
void USBDevice::EP0Receive(uint8_t* dest, word size, void (*function))
{
    ChipKITUSBEP0Receive(dest, size, function);
}

/*************************************************************************
  Function:
    USB_HANDLE USBTransferOnePacket(BYTE ep, BYTE dir, BYTE* data, BYTE len)
    
  Summary:
    Transfers a single packet (one transaction) of data on the USB bus.

  Description:
    The USBTransferOnePacket() function prepares a USB endpoint
    so that it may send data to the host (an IN transaction), or 
    receive data from the host (an OUT transaction).  The 
    USBTransferOnePacket() function can be used both to receive	and 
    send data to the host.  This function is the primary API function 
    provided by the USB stack firmware for sending or receiving application 
    data over the USB port.  

    The USBTransferOnePacket() is intended for use with all application 
    endpoints.  It is not used for sending or receiving applicaiton data 
    through endpoint 0 by using control transfers.  Separate API 
    functions, such as USBEP0Receive(), USBEP0SendRAMPtr(), and
    USBEP0SendROMPtr() are provided for this purpose.

    The	USBTransferOnePacket() writes to the Buffer Descriptor Table (BDT)
    entry associated with an endpoint buffer, and sets the UOWN bit, which 
    prepares the USB hardware to allow the transaction to complete.  The 
    application firmware can use the USBHandleBusy() macro to check the 
    status of the transaction, to see if the data has been successfully 
    transmitted yet.


    Typical Usage
    <code>
    //make sure that the we are in the configured state
    if(USBGetDeviceState() == CONFIGURED_STATE)
    {
        //make sure that the last transaction isn't busy by checking the handle
        if(!USBHandleBusy(USBInHandle))
        {
	        //Write the new data that we wish to send to the host to the INPacket[] array
	        INPacket[0] = USEFUL_APPLICATION_VALUE1;
	        INPacket[1] = USEFUL_APPLICATION_VALUE2;
	        //INPacket[2] = ... (fill in the rest of the packet data)
	      
            //Send the data contained in the INPacket[] array through endpoint "EP_NUM"
            USBInHandle = USBTransferOnePacket(EP_NUM,IN_TO_HOST,(BYTE*)&INPacket[0],sizeof(INPacket));
        }
    }
    </code>

  Conditions:
    Before calling USBTransferOnePacket(), the following should be true.
    1.  The USB stack has already been initialized (USBDeviceInit() was called).
    2.  A transaction is not already pending on the specified endpoint.  This
        is done by checking the previous request using the USBHandleBusy() 
        macro (see the typical usage example).
    3.  The host has already sent a set configuration request and the 
        enumeration process is complete.
        This can be checked by verifying that the USBGetDeviceState() 
        macro returns "CONFIGURED_STATE", prior to calling 
        USBTransferOnePacket().
 					
  Input:
    BYTE ep - The endpoint number that the data will be transmitted or 
	          received on
    BYTE dir - The direction of the transfer
               This value is either OUT_FROM_HOST or IN_TO_HOST
    BYTE* data - For IN transactions: pointer to the RAM buffer containing 
                 the data to be sent to the host.  For OUT transactions: pointer
                 to the RAM buffer that the received data should get written to.
   BYTE len - Length of the data needing to be sent (for IN transactions).
              For OUT transactions, the len parameter should normally be set
              to the endpoint size specified in the endpoint descriptor.    

  Return Values:
    USB_HANDLE - handle to the transfer.  The handle is a pointer to 
                 the BDT entry associated with this transaction.  The
                 status of the transaction (ex: if it is complete or still
                 pending) can be checked using the USBHandleBusy() macro
                 and supplying the USB_HANDLE provided by
                 USBTransferOnePacket().

  Remarks:
    If calling the USBTransferOnePacket() function from within the USBCBInitEP()
    callback function, the set configuration is still being processed and the
    USBDeviceState may not be == CONFIGURED_STATE yet.  In this	special case, 
    the USBTransferOnePacket() may still be called, but make sure that the 
    endpoint has been enabled and initialized by the USBEnableEndpoint() 
    function first.  
    
  *************************************************************************/
USB_HANDLE USBDevice::TxOnePacket(uint8_t ep, uint8_t* data, word len)
{
    return(ChipKITUSBTxOnePacket(ep, data, len));
}

/*************************************************************************
  Function:
    USB_HANDLE USBTransferOnePacket(BYTE ep, BYTE dir, BYTE* data, BYTE len)
    
  Summary:
    Transfers a single packet (one transaction) of data on the USB bus.

  Description:
    The USBTransferOnePacket() function prepares a USB endpoint
    so that it may send data to the host (an IN transaction), or 
    receive data from the host (an OUT transaction).  The 
    USBTransferOnePacket() function can be used both to receive	and 
    send data to the host.  This function is the primary API function 
    provided by the USB stack firmware for sending or receiving application 
    data over the USB port.  

    The USBTransferOnePacket() is intended for use with all application 
    endpoints.  It is not used for sending or receiving applicaiton data 
    through endpoint 0 by using control transfers.  Separate API 
    functions, such as USBEP0Receive(), USBEP0SendRAMPtr(), and
    USBEP0SendROMPtr() are provided for this purpose.

    The	USBTransferOnePacket() writes to the Buffer Descriptor Table (BDT)
    entry associated with an endpoint buffer, and sets the UOWN bit, which 
    prepares the USB hardware to allow the transaction to complete.  The 
    application firmware can use the USBHandleBusy() macro to check the 
    status of the transaction, to see if the data has been successfully 
    transmitted yet.


    Typical Usage
    <code>
    //make sure that the we are in the configured state
    if(USBGetDeviceState() == CONFIGURED_STATE)
    {
        //make sure that the last transaction isn't busy by checking the handle
        if(!USBHandleBusy(USBInHandle))
        {
	        //Write the new data that we wish to send to the host to the INPacket[] array
	        INPacket[0] = USEFUL_APPLICATION_VALUE1;
	        INPacket[1] = USEFUL_APPLICATION_VALUE2;
	        //INPacket[2] = ... (fill in the rest of the packet data)
	      
            //Send the data contained in the INPacket[] array through endpoint "EP_NUM"
            USBInHandle = USBTransferOnePacket(EP_NUM,IN_TO_HOST,(BYTE*)&INPacket[0],sizeof(INPacket));
        }
    }
    </code>

  Conditions:
    Before calling USBTransferOnePacket(), the following should be true.
    1.  The USB stack has already been initialized (USBDeviceInit() was called).
    2.  A transaction is not already pending on the specified endpoint.  This
        is done by checking the previous request using the USBHandleBusy() 
        macro (see the typical usage example).
    3.  The host has already sent a set configuration request and the 
        enumeration process is complete.
        This can be checked by verifying that the USBGetDeviceState() 
        macro returns "CONFIGURED_STATE", prior to calling 
        USBTransferOnePacket().
 					
  Input:
    BYTE ep - The endpoint number that the data will be transmitted or 
	          received on
    BYTE dir - The direction of the transfer
               This value is either OUT_FROM_HOST or IN_TO_HOST
    BYTE* data - For IN transactions: pointer to the RAM buffer containing 
                 the data to be sent to the host.  For OUT transactions: pointer
                 to the RAM buffer that the received data should get written to.
   BYTE len - Length of the data needing to be sent (for IN transactions).
              For OUT transactions, the len parameter should normally be set
              to the endpoint size specified in the endpoint descriptor.    

  Return Values:
    USB_HANDLE - handle to the transfer.  The handle is a pointer to 
                 the BDT entry associated with this transaction.  The
                 status of the transaction (ex: if it is complete or still
                 pending) can be checked using the USBHandleBusy() macro
                 and supplying the USB_HANDLE provided by
                 USBTransferOnePacket().

  Remarks:
    If calling the USBTransferOnePacket() function from within the USBCBInitEP()
    callback function, the set configuration is still being processed and the
    USBDeviceState may not be == CONFIGURED_STATE yet.  In this	special case, 
    the USBTransferOnePacket() may still be called, but make sure that the 
    endpoint has been enabled and initialized by the USBEnableEndpoint() 
    function first.  
    
  *************************************************************************/
USB_HANDLE USBDevice::GenWrite(uint8_t ep, uint8_t* data, word len)
{
    return(ChipKITUSBTxOnePacket(ep, data, len));
}

/********************************************************************
    Function:
        USB_HANDLE USBRxOnePacket(BYTE ep, BYTE* data, WORD len)
        
    Summary:
        Receives the specified data out the specified endpoint
        
    PreCondition:
        None
        
    Parameters:
        ep - the endpoint you want to receive the data into
        data - where the data will go when it arrives
        len - the length of the data that you wish to receive
        
    Return Values:
        None
        
    Remarks:
        None
  
 *******************************************************************/
USB_HANDLE USBDevice::RxOnePacket(uint8_t ep, byte* data, word len)
{
    return(ChipKITUSBRxOnePacket(ep, data, len));
}

/********************************************************************
    Function:
        USB_HANDLE USBRxOnePacket(BYTE ep, BYTE* data, WORD len)
        
    Summary:
        Receives the specified data out the specified endpoint
        
    PreCondition:
        None
        
    Parameters:
        ep - the endpoint you want to receive the data into
        data - where the data will go when it arrives
        len - the length of the data that you wish to receive
        
    Return Values:
        None
        
    Remarks:
        None
  
 *******************************************************************/
USB_HANDLE USBDevice::GenRead(uint8_t ep, uint8_t* data, word len)
{
    return(ChipKITUSBRxOnePacket(ep, data, len));
}

/**************************************************************************
    Function:
        void USBDeviceDetach(void)
   
    Summary:
        This function indicates to the USB module that the USB device has been
        detached from the bus.

    Description:
        This function indicates to the USB module that the USB device has been
        detached from the bus.  This function needs to be called in order for the
        device to start to properly prepare for the next attachment.
   
    Precondition:
        Should only be called when USB_INTERRUPT is defined.

    Parameters:
        None
     
    Return Values:
        None
        
    Remarks:
        None
                                                          
  **************************************************************************/
void USBDevice::DeviceDetach(void)
{
    ChipKITUSBDeviceDetach();
}

/**************************************************************************
    Function:
        void USBDeviceAttach(void)
    
    Summary:
        Checks if VBUS is present, and that the USB module is not already 
        initalized, and if so, enables the USB module so as to signal device 
        attachment to the USB host.   

    Description:
        This function indicates to the USB host that the USB device has been
        attached to the bus.  This function needs to be called in order for the
        device to start to enumerate on the bus.
                
    Precondition:
        Should only be called when USB_INTERRUPT is defined.  Also, should only 
        be called from the main() loop context.  Do not call USBDeviceAttach()
        from within an interrupt handler, as the USBDeviceAttach() function
        may modify global interrupt enable bits and settings.

        For normal USB devices:
        Make sure that if the module was previously on, that it has been turned off 
        for a long time (ex: 100ms+) before calling this function to re-enable the module.
        If the device turns off the D+ (for full speed) or D- (for low speed) ~1.5k ohm
        pull up resistor, and then turns it back on very quickly, common hosts will sometimes 
        reject this event, since no human could ever unplug and reattach a USB device in a 
        microseconds (or nanoseconds) timescale.  The host could simply treat this as some kind 
        of glitch and ignore the event altogether.  
    Parameters:
        None
     
    Return Values:
        None                                                        
****************************************************************************/
void USBDevice::DeviceAttach(void)
{
    ChipKITUSBDeviceAttach();
}
