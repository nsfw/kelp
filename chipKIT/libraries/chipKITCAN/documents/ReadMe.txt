Digilent chipKIT Network Shield CAN Library Release Notes
Revision date:  08/22/11

The following describes the initial release of the Digilent chipKIT CAN
library for use with the chipKIT Max32 and Network Shield.


Library Contents:

chipKITCAN.h		- interface declarations for the library
chipKITCAN.cpp		- the library

In examples:

CanDemo			- an example sketch showing communication
			  between CAN1 and CAN2 on the Network Shield

In Documents:

ReadMe.txt		- this document
chipKIT-CAN.c		- an example program written in C calling
			  the Microchip CAN peripheral library. The
			  CanDemo example is this program rewritten
			  to use the chipKIT CAN library.
chipKIT-CAN.pdf		- A document describing this example
CAN-Plib-Help.chm	- The Microchip CAN peripheral library CAN
			  help file.


Compiling the Library

Include the library in a sketch the normal way. If you get a number of compile
errors in chipKITCAN.cpp that begin with an error like this one:

chipKITCAN.cpp:90:58: error: Vector number must be an integer between 0 and 63

It means that you have the Uno32 selected as the target board. Select the Max32
as the target board.


Programming Documentation

The CAN library is a wrapper around the CAN functions provided by the Microchip C32
peripheral library. The peripheral library functions make up a low level, device
driver interface to the CAN controllers in the PIC32MX795 microcontroller on the
chipKIT Max32 board. Refer to the Microchip help file: CAN-Plib-Help.chm for
documentation on the use of the Digilent library.

The chipKIT CAN library provides a one-ton-one mapping of the functions in the
Microchip library. The chipKIT library provides a C++ object interface. The Microchip
library provides a C function call interface. The following describe the differences
between the two so that the Microchip documentation can be used:

1) Function Names

The chipKIT library functions are methods of the CAN object class. They use the
Arduino/chipKIT function naming convention of starting functions names with a 
lower case letter. The Microchip library functions all start with CAN at the
beginning of the name.

The chipKIT library function: CAN::assignMemoryBuffer corresponds to the peripheral
library function CANAssignMemoryBufffer. Refer to this function in the help file.

2) Data Type Names

The library uses a number of enumerations to define data types and member names
that are passed as parameters to the various functions. These enumerations have
been declared within the scope of the CAN object class. The C++ scope resolution
operator is used to access these symbol names. All chipKIT library data type and
member names are prefixed with: CAN::

The data type and member names in the peripheral library all begin with CAN_

The chipKIT library data type: CAN::CHANNEL corresponds to the CAN_CHANNEL data 
type in the peripheral library documentation.

The same applies to the member names of the enumerations. In the chipKIT library,
the CAN::CHANNEL enumeration has members with names of this form: CAN::CHANNEL0.
The corresponding member name in the peripheral library is CAN_CHANNEL0.

3) CAN Object Constructor

A single constructor is provided for the CAN object class. This constructor accepts
a parameter specifying which CAN controller module is to be used.

Example:

The following statement constructs a CAN object to use CAN controller module CAN1.

CAN	canMod1(CAN::CAN1);

4) CAN Module Parameter

The functions in the peripheral library have as their first parameter, a value
that specifies which CAN controller to use. This parameter is supplied by the
object class, and so is not provided to the CAN class member functions.

Example:

Assuming that the above statement has been used to construct an object referring
to cAN1, the following statement will reset a channel on that CAN controller

canMod1.resetChannel(CAN::CHANNEL0);

The corresponding call in the peripheral library is:

CANResetChannel(CAN1, CAN_CHANNEL0);

5) Variable Declarations

To declare a variable to hold a data value associated with the CAN object class,
use the CAN:: scope resolution to prefix the type name.

Example:

This statement declares a variable to hold a CHANNEL value.

CAN::CHANNEL	chan;

This statement declares a CHANNEL variable and initializes it to a CHANNEL number.

CAN::CHANNEL	chan = CAN::CHANNEL0;

Refer to either the chipKITCAN.h file or the Microchip help file to see what
data types are used.

6) Interrupts

The CAN functions in the peripheral library don't explicitly use interrupts.
The INT peripheral library is used to set up interrupts when using the
peripheral library.

The chipKIT library supports interrupts by proving two methods that aren't part
of the set of functions of the peripheral library:

CAN::attachInterrupt(void (*pfn)());
CAN::detachInterrupt();

attachInterrupt works similarly to the standard Arduino/chipKIT attachInterrupt
function. Pass the name of the function to be called to service the interrupt
as the parameter to CAN::attachInterrupt. This function will enable the interrupt
appropriate for the CAN controller.

Example:

canMod1.attachInterrupt(myInterruptFunction);

detachInterrupt disable the interrupt and removes the user function.



