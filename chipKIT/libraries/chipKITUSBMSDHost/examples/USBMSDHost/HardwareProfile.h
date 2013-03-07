// HardwareProfile.h

#ifndef _HARDWARE_PROFILE_H_
#define _HARDWARE_PROFILE_H_


// ******************* CPU Speed defintions ************************************
//  This section is required by some of the peripheral libraries and software
//  libraries in order to know what the speed of the processor is to properly
//  configure the hardware modules to run at the proper speeds
// *****************************************************************************


    #define USB_A0_SILICON_WORK_AROUND


// ******************* MDD File System Required Definitions ********************
// Select your MDD File System interface type
// This library currently only supports a single physical interface layer
// In this example we are going to use the USB so we only need the USB definition
// *****************************************************************************
#define USE_USB_INTERFACE               // USB host MSD library


// ******************* Debugging interface hardware settings *******************
//  This section is not required by any of the libraries.  This is a
//  demo specific implmentation to assist in debugging.  
// *****************************************************************************
// Define the baud rate constants

    #include <p32xxxx.h>
    #include <plib.h>

#endif  

