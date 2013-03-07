                                             
This demo uses WinUSB class USB device driver
The WinUSB device driver is not preinstalled on most Windows computers.
This example will fail to run if the Microchip Custom WinUSB device driver is not installed. 

After loading the sketch, when you plug in your USB cable Windows will complain that
it does not have the device driver for the Microchip Custom USB device.

Go to device manager, select the Custom USB Devices, and select the Microchip Custom USB Device
Then select the option to install the driver manually. Say to browser your disk.

Point to the ...\libraries\chipKITUSBDevice\examples\GenericUSB\PCCode directory; this
is where the .inf file is located.

When installing, you will probably get a warning about the driver not being signed. Allow Windows
to install the driver anyway.

This driver is provided by Microchip; we have had some problems installing it on Windows XP. This might be fixed by restarting the computer.
If you are unable to get the driver installed, the demo will not work. However, the CustomHID demo is 
very easy to understand and works without any drivers being installed.