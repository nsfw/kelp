#include <chipKITUSBHost.h>
#include <chipKITUSBHIDHost.h>

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
/*  A demonstration of a mouse HID USB Host device      		*/
/*	The Left mouse button will toggle the LED                       */
/*  The Right mouse button will toggle blinking the LED                 */
/*	An HID Mouse is requred for the demo				*/
/************************************************************************/
/*  Revision History:							*/
/*									*/
/*	9/07/2011(KeithV): Created					*/
/*									*/
/************************************************************************/

void App_Detect_Device(void);
void App_ProcessInputReport(void);
BOOL USB_HID_DataCollectionHandler(void);

// *****************************************************************************
// *****************************************************************************
// defines
// *****************************************************************************
// *****************************************************************************

#define USAGE_PAGE_BUTTONS              (0x09)
#define USAGE_PAGE_GEN_DESKTOP          (0x01)
#define MAX_ERROR_COUNTER               (10)
#define LEDPIN                          (13)
#define BLINKPERIOD                     (500) // every half second to blink

// *****************************************************************************
// *****************************************************************************
// Typedefs
// *****************************************************************************
// *****************************************************************************
typedef enum _APP_STATE
{
    DEVICE_NOT_CONNECTED,
    DEVICE_CONNECTED, /* Device Enumerated  - Report Descriptor Parsed */
    READY_TO_TX_RX_REPORT,
    GET_INPUT_REPORT, /* perform operation on received report */
    INPUT_REPORT_PENDING,
    ERROR_REPORTED 
} APP_STATE;

typedef struct _HID_REPORT_BUFFER
{
    WORD  Report_ID;
    WORD  ReportSize;
//    BYTE* ReportData;
    uint8_t  ReportData[4];
    WORD  ReportPollRate;
}   HID_REPORT_BUFFER;


// *****************************************************************************
// *****************************************************************************
// global variables
// *****************************************************************************
// *****************************************************************************

APP_STATE App_State_Mouse = DEVICE_NOT_CONNECTED;

HID_DATA_DETAILS Appl_Mouse_Buttons_Details;
HID_DATA_DETAILS Appl_XY_Axis_Details;

HID_REPORT_BUFFER  Appl_raw_report_buffer;

HID_USER_DATA_SIZE Appl_Button_report_buffer[16];
HID_USER_DATA_SIZE Appl_XY_report_buffer[16];

BOOL DisplayConnectOnce = FALSE;
BOOL DisplayDeatachOnce = FALSE;
BOOL ReportBufferUpdated;
uint8_t NumOfBytesRcvd;
uint8_t  ErrorDriver;
uint8_t  ErrorCounter;

unsigned int curLedState = LOW;
bool fBlink = false;
unsigned long timeBlink = 0;

//******************************************************************************
//******************************************************************************
// USB Support Functions
//******************************************************************************
//******************************************************************************

/****************************************************************************
  Function:
    BOOL MyHIDEventHandler( uint8_t address, USB_EVENT event, void *data, DWORD size )

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
BOOL MyHIDEventHandler( uint8_t address, USB_EVENT event, void *data, DWORD size )
{
    BOOL fRet = FALSE;

    // call the default handler for common host controller stuff
    fRet = USBHost.DefaultEventHandler(address, event, data, size);

    switch( event )
    {
     	    case EVENT_HID_RPT_DESC_PARSED:
		        return(USB_HID_DataCollectionHandler());
		        break;

            default:
                break;
    }

    return(fRet);
}

/****************************************************************************
  Function:
    void App_ProcessInputReport(void)

  Description:
    This function processes input report received from HID device.

  Precondition:
    None

  Parameters:
    None

  Return Values:
    None

  Remarks:
    None
***************************************************************************/
void App_ProcessInputReport(void)
{
    uint8_t  data; // this just shows how to load the X-Y data, we do nothing with it

   /* process input report received from device */
    USBHIDHost.ApiImportData(Appl_raw_report_buffer.ReportData, Appl_raw_report_buffer.ReportSize
                          ,Appl_Button_report_buffer, &Appl_Mouse_Buttons_Details);
    USBHIDHost.ApiImportData(Appl_raw_report_buffer.ReportData, Appl_raw_report_buffer.ReportSize
                          ,Appl_XY_report_buffer, &Appl_XY_Axis_Details);
 
 // X-axis
    data = (Appl_XY_report_buffer[0] & 0xF0) >> 4;
    data = (Appl_XY_report_buffer[0] & 0x0F);

 // Y-axis
    data = (Appl_XY_report_buffer[1] & 0xF0) >> 4;
    data = (Appl_XY_report_buffer[1] & 0x0F);
    
    // if the left mouse button is hit, then toggle the led
    if(Appl_Button_report_buffer[0] == 1)
    {
        fBlink = false;
        curLedState ^= HIGH;
		digitalWrite(LEDPIN, curLedState);
    }

    // if the right button was hit, start blinking
    if(Appl_Button_report_buffer[1] == 1)
    {
		fBlink = !fBlink;
    }
}

/****************************************************************************
  Function:
    void App_Detect_Device(void)

  Description:
    This function monitors the status of device connected/disconnected

  Precondition:
    None

  Parameters:
    None

  Return Values:
    None

  Remarks:
    None
***************************************************************************/
void App_Detect_Device(void)
{
  if(!USBHIDHost.DeviceDetect(1))
  {
     App_State_Mouse = DEVICE_NOT_CONNECTED;
  }
}

/****************************************************************************
  Function:
    BOOL USB_HID_DataCollectionHandler(void)
  Description:
    This function is invoked by HID client , purpose is to collect the 
    details extracted from the report descriptor. HID client will store
    information extracted from the report descriptor in data structures.
    Application needs to create object for each report type it needs to 
    extract.
    For ex: HID_DATA_DETAILS Appl_ModifierKeysDetails;
    HID_DATA_DETAILS is defined in file usb_host_hid_appl_interface.h
    Each member of the structure must be initialized inside this function.
    Application interface layer provides functions :
    USBHostHID_ApiFindBit()
    USBHostHID_ApiFindValue()
    These functions can be used to fill in the details as shown in the demo
    code.

  Precondition:
    None

  Parameters:
    None

  Return Values:
    TRUE    - If the report details are collected successfully.
    FALSE   - If the application does not find the the supported format.

  Remarks:
    This Function name should be entered in the USB configuration tool
    in the field "Parsed Data Collection handler".
    If the application does not define this function , then HID cient 
    assumes that Application is aware of report format of the attached
    device.
***************************************************************************/
BOOL USB_HID_DataCollectionHandler(void)
{
  uint8_t NumOfReportItem = 0;
  uint8_t i;
  USB_HID_ITEM_LIST* pitemListPtrs;
  USB_HID_DEVICE_RPT_INFO* pDeviceRptinfo;
  HID_REPORTITEM *reportItem;
  HID_USAGEITEM *hidUsageItem;
  uint8_t usageIndex;
  uint8_t reportIndex;

  pDeviceRptinfo = USBHIDHost.GetCurrentReportInfo(); // Get current Report Info pointer
  pitemListPtrs = USBHIDHost.GetItemListPointers();   // Get pointer to list of item pointers

  BOOL status = FALSE;
   /* Find Report Item Index for Modifier Keys */
   /* Once report Item is located , extract information from data structures provided by the parser */
   NumOfReportItem = pDeviceRptinfo->reportItems;
   for(i=0;i<NumOfReportItem;i++)
    {
       reportItem = &pitemListPtrs->reportItemList[i];
       if((reportItem->reportType==hidReportInput) && (reportItem->dataModes == (HIDData_Variable|HIDData_Relative))&&
           (reportItem->globals.usagePage==USAGE_PAGE_GEN_DESKTOP))
        {
           /* We now know report item points to modifier keys */
           /* Now make sure usage Min & Max are as per application */
            usageIndex = reportItem->firstUsageItem;
            hidUsageItem = &pitemListPtrs->usageItemList[usageIndex];

            reportIndex = reportItem->globals.reportIndex;
            Appl_XY_Axis_Details.reportLength = (pitemListPtrs->reportList[reportIndex].inputBits + 7)/8;
            Appl_XY_Axis_Details.reportID = (uint8_t)reportItem->globals.reportID;
            Appl_XY_Axis_Details.bitOffset = (uint8_t)reportItem->startBit;
            Appl_XY_Axis_Details.bitLength = (uint8_t)reportItem->globals.reportsize;
            Appl_XY_Axis_Details.count=(uint8_t)reportItem->globals.reportCount;
            Appl_XY_Axis_Details.interfaceNum= USBHIDHost.ApiGetCurrentInterfaceNum();
        }
        else if((reportItem->reportType==hidReportInput) && (reportItem->dataModes == HIDData_Variable)&&
           (reportItem->globals.usagePage==USAGE_PAGE_BUTTONS))
        {
           /* We now know report item points to modifier keys */
           /* Now make sure usage Min & Max are as per application */
            usageIndex = reportItem->firstUsageItem;
            hidUsageItem = &pitemListPtrs->usageItemList[usageIndex];

            reportIndex = reportItem->globals.reportIndex;
            Appl_Mouse_Buttons_Details.reportLength = (pitemListPtrs->reportList[reportIndex].inputBits + 7)/8;
            Appl_Mouse_Buttons_Details.reportID = (uint8_t)reportItem->globals.reportID;
            Appl_Mouse_Buttons_Details.bitOffset = (uint8_t)reportItem->startBit;
            Appl_Mouse_Buttons_Details.bitLength = (uint8_t)reportItem->globals.reportsize;
            Appl_Mouse_Buttons_Details.count=(uint8_t)reportItem->globals.reportCount;
            Appl_Mouse_Buttons_Details.interfaceNum= USBHIDHost.ApiGetCurrentInterfaceNum();
        }
    }

   if(pDeviceRptinfo->reports == 1)
    {
        Appl_raw_report_buffer.Report_ID = 0;
        Appl_raw_report_buffer.ReportSize = (pitemListPtrs->reportList[reportIndex].inputBits + 7)/8;
//        Appl_raw_report_buffer.ReportData = (BYTE*)malloc(Appl_raw_report_buffer.ReportSize);
        Appl_raw_report_buffer.ReportPollRate = pDeviceRptinfo->reportPollingRate;
        status = TRUE;
    }

    return(status);
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
    USBHIDHost.Tasks();
}

//******************************************************************************
//******************************************************************************
// Required Sketch functions
//******************************************************************************
//******************************************************************************
void setup() {

  // put your setup code here, to run once:

    // initialize the USB HOST controller
    USBHost.Begin(MyHIDEventHandler);

    // set the LED as an output pin
    pinMode(LEDPIN, OUTPUT);
    digitalWrite(LEDPIN, curLedState);
}

void loop()
{
    uint8_t i;

    RunUSBTasks();
    App_Detect_Device();
    switch(App_State_Mouse)
    {
        case DEVICE_NOT_CONNECTED:
                        RunUSBTasks();
                        if(DisplayDeatachOnce == FALSE)
                        {
                        DisplayDeatachOnce = TRUE;
                        }
                        if(USBHIDHost.DeviceDetect(1)) /* True if report descriptor is parsed with no error */
                        {
                        App_State_Mouse = DEVICE_CONNECTED;
                        DisplayConnectOnce = FALSE;
                        }
            break;
        case DEVICE_CONNECTED:
                    App_State_Mouse = READY_TO_TX_RX_REPORT;
                    if(DisplayConnectOnce == FALSE)
                    {
                        DisplayConnectOnce = TRUE;
                        DisplayDeatachOnce = FALSE;
                    }

            break;
        case READY_TO_TX_RX_REPORT:
                        if(!USBHIDHost.DeviceDetect(1))
                        {
                        App_State_Mouse = DEVICE_NOT_CONNECTED;
                        }
                        else
                        {
                        App_State_Mouse = GET_INPUT_REPORT;
                        }

            break;
        case GET_INPUT_REPORT:
                    if(USBHIDHost.Read(1, Appl_raw_report_buffer.Report_ID,0,
                                                Appl_raw_report_buffer.ReportSize, Appl_raw_report_buffer.ReportData))
                    {
                        /* Host may be busy/error -- keep trying */
                    }
                    else
                    {
                        App_State_Mouse = INPUT_REPORT_PENDING;
                    }
                    RunUSBTasks();
            break;
        case INPUT_REPORT_PENDING:
                    if(USBHIDHost.TransferIsComplete(1, &ErrorDriver,&NumOfBytesRcvd))
                    {
                        if(ErrorDriver ||(NumOfBytesRcvd != Appl_raw_report_buffer.ReportSize ))
                        {
                            ErrorCounter++ ; 
                            if(MAX_ERROR_COUNTER <= ErrorDriver)
                                App_State_Mouse = ERROR_REPORTED;
                            else
                                App_State_Mouse = READY_TO_TX_RX_REPORT;
                        }
                        else
                        {
                            ErrorCounter = 0; 
                            ReportBufferUpdated = TRUE;
                            App_State_Mouse = READY_TO_TX_RX_REPORT;

                            if(DisplayConnectOnce == TRUE)
                            {
                                for(i=0;i<Appl_raw_report_buffer.ReportSize;i++)
                                {
                                    if(Appl_raw_report_buffer.ReportData[i] != 0)
                                    {
                                        DisplayConnectOnce = FALSE;
                                    }
                                }
                            }

                            App_ProcessInputReport();
                        }
                    }
            break;

        case ERROR_REPORTED:
            break;
        default:
            break;

    }

    // Blink the LED if the blink option is active
    if(fBlink)
    {
        if(millis() > timeBlink)
        {
            timeBlink = millis() + BLINKPERIOD;
            curLedState ^= HIGH;
            digitalWrite(LEDPIN, curLedState);
        }
    }
}

