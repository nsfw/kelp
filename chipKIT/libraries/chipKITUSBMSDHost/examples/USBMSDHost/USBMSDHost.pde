#include <chipKITUSBHost.h>
#include <chipKITUSBMSDHost.h>
#include <chipKITMDDFS.h>

/************************************************************************/
/*									*/
/*	USBHIDHost.pde	-- USB HID Mouse HOST Sketch example            */
/*									*/
/************************************************************************/
/*	Author: 	Keith Vogel 					*/
/*	Copyright 2011, Digilent Inc.					*/
/************************************************************************/
/*
  This sketch is free software; you can redistribute it and/or
  modify it under the terms of the GNU Lesser General Public
  License as published by the Free Software Foundation; either
  version 2.1 of the License, or (at your option) any later version.

  This sketch is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
  Lesser General Public License for more details.

  You should have received a copy of the GNU Lesser General Public
  License along with this library; if not, write to the Free Software
  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/
/************************************************************************/
/*  Module Description: 						*/
/*  A demonstration of a thumb drive MSD USB Host device      		*/
/*	After inserting a thumb drive into the USB Host Port            */
/*  wait about 30 seconds and a file named TEST.TXT                     */
/*	should be written on the Thumb drive with			*/
/*	the contents of							*/
/*	"This is a test."						*/
/************************************************************************/
/*  Revision History:							*/
/*									*/
/*	9/07/2011(KeithV): Created					*/
/*									*/
/************************************************************************/

//******************************************************************************
//******************************************************************************
// Global Variables
//******************************************************************************
//******************************************************************************

FSFILE * myFile;
uint8_t myData[512];
size_t numBytes;
BOOL deviceAttached = FALSE;

//******************************************************************************
//******************************************************************************
// USB Support Functions
//******************************************************************************
//******************************************************************************

/****************************************************************************
  Function:
    BOOL MyMSDEventHandler( uint8_t address, USB_EVENT event, void *data, DWORD size )

  Description:
    Handles all of the events for the HID device

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
    We call the default one here so things like VBUS is automatically handled.

***************************************************************************/
BOOL MyMSDEventHandler( uint8_t address, USB_EVENT event, void *data, DWORD size )
{
    BOOL fRet = FALSE;

    // call the default handler for common host controller stuff
    fRet = USBHost.DefaultEventHandler(address, event, data, size);

    switch( event )
    {
        case EVENT_VBUS_RELEASE_POWER:

            //This means that the device was removed
            // this will get us out of the while loop
            // and start looking for a new thumb drive to be
            // plugged in.
            deviceAttached = FALSE;
            return TRUE;
            break;

        default:
            break;
    }

    return(fRet);
}

/****************************************************************************
  Function:
    void RunUSBTasks(void)

  Description:
    Runs periodic tasks to keep the USB stack alive and well

  Precondition:
    None

  Parameters:
    None
  Return Values:
    None

  Remarks:
    Call this at least once through the loop, or when we want the 
    USB Host controller to update itself internally

***************************************************************************/
void RunUSBTasks(void)
{
    USBHost.Tasks();
    USBMSDHost.Tasks();
}

//******************************************************************************
//******************************************************************************
// Required Sketch functions
//******************************************************************************
//******************************************************************************
void setup() {
  // put your setup code here, to run once:

    // initialize the USB HOST controller
    USBHost.Begin(MyMSDEventHandler);
}

void loop() {
  // put your main code here, to run repeatedly: 
  
    //USB stack process function
    RunUSBTasks();

    //if thumbdrive is plugged in
    if(USBMSDHost.SCSIMediaDetect())
    {
        deviceAttached = TRUE;

        //now a device is attached
        //See if the device is attached and in the right format
        if(MDDFS.Init())
        {
            //Opening a file in mode "w" will create the file if it doesn't
            //  exist.  If the file does exist it will delete the old file
            //  and create a new one that is blank.
            myFile = MDDFS.fopen("test.txt","w");

            //Write some data to the new file.
            MDDFS.fwrite("This is a test.",1,15,myFile);
                
            //Always make sure to close the file so that the data gets
            //  written to the drive.
            MDDFS.fclose(myFile);

            //Just sit here until the device is removed.
            while(deviceAttached == TRUE)
            {
                RunUSBTasks();
            }
        }
    }   
}
