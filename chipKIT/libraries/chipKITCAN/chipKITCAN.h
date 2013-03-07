/************************************************************************/
/*																		*/
/*	chipKITCan.h    --  Interface Declarations for chipKITCan.cpp       */
/*																		*/
/************************************************************************/
/*	Author:     Gene Apperson                                           */
/*	Copyright (c) 2011, Digilent Inc. All rights reserved.              */
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
/*  File Description:													*/
/*																		*/
/*  This header file contains the declarations for the interface to the */
/*  CAN library for use with the Digilent Network Shield. This library  */
/*  is a wrapper for the CAN functions provided in the Microchp C32     */
/*  Peripheral Library provided as part of the Microchip C++ Compiler   */
/*  runtime support in the chipKIT/MPIDE system.                        */
/*																		*/
/************************************************************************/
/*  Revision History:													*/
/*																		*/
/*  08/20/2011(GeneApperson): Created                                   */
/*																		*/
/************************************************************************/

#if !defined(CHIPKIT_CAN_H)

/* ------------------------------------------------------------ */
/*                  Miscellaneous Declarations                  */
/* ------------------------------------------------------------ */


/* ------------------------------------------------------------ */
/*                  Type Declarations                           */
/* ------------------------------------------------------------ */


/* ------------------------------------------------------------ */
/*                  Object Class Declarations                   */
/* ------------------------------------------------------------ */

/* The CAN object class declares the object used to access the CAN
** controller modules in the PIC32MX795 microcontroller
**
** This object class uses a number of enumerated types for various
** parameters of its member functions. These types are declared
** within the scope of the object class declaration rather than
** being global type names. The class name and the scope resolution
** operator need to be used when accessing these types or the
** enumerated values.
**
** Example:
**  CAN     can1(CAN::CAN1);    //CAN object accessing CAN module 1
**  CAN::CHANNEL        chn;    //declaration of a CAN channel variable
**  CAN::CHANNEL_EVENT  evt;    //declaration of channel event variable
**
**  chn = CAN::CHANNEL0;        //assignment of channel number to the variable
**  evt = can1.getChannelEvent(CAN::CHANNEL7);  //passing channel number constant as parameter
**
*/ 

class CAN {

public:

/* ------------------------------------------------------------ */
/*          CAN Object Class Type Declarations                  */
/* ------------------------------------------------------------ */

typedef enum {
	FALSE = 0,
	TRUE
} BOOL;

/* CAN module identifier.
*/
typedef enum {
    CAN1,
    CAN2,
    NUM_CAN_MODULES,
    MOD_NIL = 0xFF
} MODULE;

/* ------------------------------------------------------------ */
/* CAN channel identifier.
*/
typedef enum {
	CHANNEL0,       // Channel 0 ID
	CHANNEL1,       // Channel 1 ID
	CHANNEL2,       // Channel 2 ID
	CHANNEL3,       // Channel 3 ID
	CHANNEL4,       // Channel 4 ID
	CHANNEL5,       // Channel 5 ID
	CHANNEL6,       // Channel 6 ID
	CHANNEL7,       // Channel 7 ID
	CHANNEL8,       // Channel 8 ID
	CHANNEL9,       // Channel 9 ID
	CHANNEL10,      // Channel 10 ID
	CHANNEL11,      // Channel 11 ID
	CHANNEL12,      // Channel 12 ID
	CHANNEL13,      // Channel 13 ID
	CHANNEL14,      // Channel 14 ID
	CHANNEL15,      // Channel 15 ID
	CHANNEL16,      // Channel 16 ID
	CHANNEL17,      // Channel 17 ID
	CHANNEL18,      // Channel 18 ID
	CHANNEL19,      // Channel 19 ID
	CHANNEL20,      // Channel 20 ID
	CHANNEL21,      // Channel 21 ID
	CHANNEL22,      // Channel 22 ID
	CHANNEL23,      // Channel 23 ID
	CHANNEL24,      // Channel 24 ID
	CHANNEL25,      // Channel 25 ID
	CHANNEL26,      // Channel 26 ID
	CHANNEL27,      // Channel 27 ID
	CHANNEL28,      // Channel 28 ID
	CHANNEL29,      // Channel 29 ID
	CHANNEL30,      // Channel 30 ID
	CHANNEL31,      // Channel 31 ID
	ALL_CHANNELS    // only used with the CANAbortPendingTx() function.
} CHANNEL;

/* ------------------------------------------------------------ */
/* CAN module operating modes.
**  NORMAL_OPERATION    - Normal operating mode. The CAN module
**                        transmits and receives messages
**  DISABLE             - Module disabled. The CAN module does not
**                        transmit or receive messages in this mode
**  LOOPBACK            - Loopback mode. In this mode, the CAN module
**                        TX is internally connected to RX
**  LISTEN_ONLY         - Listen Only. The CAN module captures all messages
**                        but does not acknowledge signal or participate in
**                        error signalling
**  CONFIGURATION       - Configuration mode. CAN module settings can be
**                        configured in this mode
**  LISTEN_ALL_MESSAGES - Listen all messages. CAN module listens to all
**                        messages regardless of errors
*/
typedef enum {
	NORMAL_OPERATION,           
	DISABLE,                   
	LOOPBACK,                   
	LISTEN_ONLY,                
	CONFIGURATION,              
	LISTEN_ALL_MESSAGES = 7 
} OP_MODE;

/* ------------------------------------------------------------ */
/* CAN Channel Event
** This enumerates all of the CAN TX and RX channel events. The
** members of this enumeration can be used to denable or disable
** channel events or as a mask to check if a channel event is
** enabled.
*/
typedef enum {
	RX_CHANNEL_NOT_EMPTY 	= 0x1,      // CAN RX Channel Not Empty Event
	RX_CHANNEL_HALF_FULL 	= 0x2,      // CAN RX Channel Half Full Event
	RX_CHANNEL_FULL 		= 0x4,      // CAN RX Channel Full Event
	RX_CHANNEL_OVERFLOW 	= 0x8,      // CAN RX Channel Overflow Event
	RX_CHANNEL_ANY_EVENT 	= 0xF,      // CAN RX Channel Any Event
	TX_CHANNEL_EMPTY 		= 0x100,    // CAN TX Channel Empty Event
	TX_CHANNEL_HALF_EMPTY 	= 0x200,    // CAN TX Channel Half Empty Event
	TX_CHANNEL_NOT_FULL		= 0x400,    // CAN TX Channel Not Full Event
	TX_CHANNEL_ANY_EVENT 	= 0x700     // CAN TX Channel Any Event
} CHANNEL_EVENT;

/* ------------------------------------------------------------ */
/* CAN Bit Time Quanta
** This enumeration gives the values that can be used to define
** the number of Time Quanta per bit.
*/
typedef enum {
	BIT_1TQ,    // 1-bit Time Quanta
	BIT_2TQ,    // 2-bit Time Quanta
	BIT_3TQ,    // 3-bit Time Quanta
	BIT_4TQ,    // 4-bit Time Quanta
	BIT_5TQ,    // 5-bit Time Quanta
	BIT_6TQ,    // 6-bit Time Quanta
	BIT_7TQ,    // 7-bit Time Quanta
	BIT_8TQ     // 8-bit Time Quanta
} BIT_TQ;

/* ------------------------------------------------------------ */
/* CAN Bit Configuration
** This structure is used to configure the bit parameters.
**  phaseSeg2Tq         - Number of Time quanta in Phase Segment 2. 
**                        This value can be between BIT_1TQ and BIT_8TQ
**  phaseSeg1Tq         - Number of Time quanta in Phase Segment 1. 
**                        This value can be between BIT_1TQ and BIT_8TQ
**  propagationSegTq    - Number of Time quanta in Propagation Segment 2.
**                        This value can be between BIT_1TQ and BIT_8TQ 
**  phaseSeg2TimeSelect - This determines if the Phase Segment 2  value is 
**                        specified by code or set automatically by the module. 
**                              TRUE - Phase Segment 2 can be set in code.
**                              FALSE - Phase Segment 2 is set by module. 
**  sample3Time         - This determines the number of times a bit is sampled by
**                        the CAN module.. 
**                              TRUE - bit is sampled 3 times.
**                              FALSE - bit is sampled once. 
**  syncJumpWidth       - This determines the Synchronization jump width Time quanta. 
**                        This value should be between BIT_1TQ and BIT_4TQ.
*/
typedef struct 
{
	BIT_TQ 	phaseSeg2Tq;
	BIT_TQ 	phaseSeg1Tq;
	BIT_TQ 	propagationSegTq;
	BOOL	phaseSeg2TimeSelect;
	BOOL 	sample3Time;
	BIT_TQ 	syncJumpWidth;
} BIT_CONFIG;

/* ------------------------------------------------------------ */
/* CAN Event Code
** This gives the event codes that can be returned by getPendingEventCode
*/
typedef enum {
	CHANNEL0_EVENT,     // An event on Channel 0 is active.
	CHANNEL1_EVENT,     // An event on Channel 1 is active.
	CHANNEL2_EVENT,     // An event on Channel 2 is active.
	CHANNEL3_EVENT,     // An event on Channel 3 is active.
	CHANNEL4_EVENT,     // An event on Channel 4 is active.
	CHANNEL5_EVENT,     // An event on Channel 5 is active.
	CHANNEL6_EVENT,     // An event on Channel 6 is active.
	CHANNEL7_EVENT,     // An event on Channel 7 is active.
	CHANNEL8_EVENT,     // An event on Channel 8 is active.
	CHANNEL9_EVENT,     // An event on Channel 9 is active.
	CHANNEL10_EVENT,    // An event on Channel 10  is active.
	CHANNEL11_EVENT,    // An event on Channel 11  is active.
	CHANNEL12_EVENT,    // An event on Channel 12  is active.
	CHANNEL13_EVENT,    // An event on Channel 13  is active.
	CHANNEL14_EVENT,    // An event on Channel 14  is active.
	CHANNEL15_EVENT,    // An event on Channel 15  is active.
	CHANNEL16_EVENT,    // An event on Channel 16  is active.
	CHANNEL17_EVENT,    // An event on Channel 17  is active.
	CHANNEL18_EVENT,    // An event on Channel 18  is active.
	CHANNEL19_EVENT,    // An event on Channel 19  is active.
	CHANNEL20_EVENT,    // An event on Channel 20  is active.
	CHANNEL21_EVENT,    // An event on Channel 21  is active.
	CHANNEL22_EVENT,    // An event on Channel 22  is active.
	CHANNEL23_EVENT,    // An event on Channel 23  is active.
	CHANNEL24_EVENT,    // An event on Channel 24  is active.
	CHANNEL25_EVENT,    // An event on Channel 25  is active.
	CHANNEL26_EVENT,    // An event on Channel 26  is active.
	CHANNEL27_EVENT,    // An event on Channel 27  is active.
	CHANNEL28_EVENT,    // An event on Channel 28  is active.
	CHANNEL29_EVENT,    // An event on Channel 29  is active.
	CHANNEL30_EVENT,    // An event on Channel 30  is active.
	CHANNEL31_EVENT,    // An event on Channel 31  is active.
	NO_EVENT = 0x40,    // No Event is active.
	ERROR_EVENT,        // CAN Bus Error Event is active.
	WAKEUP_EVENT,       // CAN Bus Wakeup Event is active.
	RX_CHANNEL_OVERFLOW_EVENT,  // CAN Receive Channel Overflow Event is active.
	ADDRESS_ERROR_EVENT,        // CAN Address Error Event is active.
	BUS_BANDWIDTH_ERROR,        // CAN Bus Bandwidth Error is active.
	TIMESTAMP_TIMER_EVENT,      // CAN Timestamp Timer Overflow event is active.
	MODE_CHANGE_EVENT,          // CAN Module Mode Change is active.
	INVALID_MESSAGE_RECEIVED_EVENT  // CAN Invalid Message Received Event active.
} EVENT_CODE;

/* ------------------------------------------------------------ */
/* CAN Filter Identifiers
*/
typedef enum {
	FILTER0,        // CAN Filter 0 
	FILTER1,        // CAN Filter 1
	FILTER2,        // CAN Filter 2
	FILTER3,        // CAN Filter 3
	FILTER4,        // CAN Filter 4
	FILTER5,        // CAN Filter 5
	FILTER6,        // CAN Filter 6
	FILTER7,        // CAN Filter 7
	FILTER8,        // CAN Filter 8
	FILTER9,        // CAN Filter 9
	FILTER10,       // CAN Filter 10
	FILTER11,       // CAN Filter 11
	FILTER12,       // CAN Filter 12
	FILTER13,       // CAN Filter 13
	FILTER14,       // CAN Filter 14
	FILTER15,       // CAN Filter 15
	FILTER16,       // CAN Filter 16
	FILTER17,       // CAN Filter 17
	FILTER18,       // CAN Filter 18
	FILTER19,       // CAN Filter 19
	FILTER20,       // CAN Filter 20
	FILTER21,       // CAN Filter 21
	FILTER22,       // CAN Filter 22
	FILTER23,       // CAN Filter 23
	FILTER24,       // CAN Filter 24
	FILTER25,       // CAN Filter 25
	FILTER26,       // CAN Filter 26
	FILTER27,       // CAN Filter 27
	FILTER28,       // CAN Filter 28
	FILTER29,       // CAN Filter 29
	FILTER30,       // CAN Filter 30
	FILTER31,       // CAN Filter 31
    
    /* Total number of filters in the module.
    */  
    NUMBER_OF_FILTERS
} FILTER;

/* ------------------------------------------------------------ */
/* CAN Filter Masks
*/
typedef enum {
	FILTER_MASK0,   // CAN Filter Mask 0
	FILTER_MASK1,   // CAN Filter Mask 1
	FILTER_MASK2,   // CAN Filter Mask 2
	FILTER_MASK3,    // CAN Filter Mask 3
    
    /* Total number os filter masks in the module.
    */  
    NUMBER_OF_FILTER_MASKS
} FILTER_MASK;

/* ------------------------------------------------------------ */
/* CAN ID Type
** This identifies the type of CAN id. A standard id is 11 bits
** long, an extended id is 29 bits long.
*/
typedef enum {
	EID,    // CAN Extended ID
	SID     // CAN Standard ID
} ID_TYPE;

/* ------------------------------------------------------------ */
/* CAN Remote Transmit Request (RTR)
** Used to specify the status of the Remote Transmit Request
** feature on a TX channel. This allows a node on the bus to
** request a transmission from another node on the network. The
** responding node must have an RTR enabled TX channel in order
** to respond to the request.
*/
typedef enum {
	TX_RTR_ENABLED,   // CAN TX Channel RTR Feature is enabled.
	TX_RTR_DISABLED   // CAN TX Channel RTR Feature is disabled.
} TX_RTR;

/* ------------------------------------------------------------ */
/* CAN Receive Data Only Mode
** Selects Data Only Receive Mode or Full Receive Mode
** In Data Only mode, the receiver only stores the data part of a message
** In Full Receive mode, the receiver stores the entire message
** (ID field plus data payload)
*/
typedef enum {
	RX_DATA_ONLY,      // CAN RX Channel Data Only Mode is enabled.
	RX_FULL_RECEIVE    // CAN RX Channel Full Receive Mode is enabled.
} RX_DATA_MODE;

/* ------------------------------------------------------------ */
/* CAN Filter Mask Type
*/
typedef enum {
	FILTER_MASK_IDE_TYPE,   // Processes only type of message id'd by filter
	FILTER_MASK_ANY_TYPE    // Processes any type (SID or EID) of message.
} FILTER_MASK_TYPE;

/* ------------------------------------------------------------ */
/* CAN Transmit Channel Priority
*/
typedef enum {
	LOWEST_PRIORITY,
	LOW_MEDIUM_PRIORITY,
	HIGH_MEDIUM_PRIORITY,
	HIGHEST_PRIORITY
} TXCHANNEL_PRIORITY;

/* ------------------------------------------------------------ */
/* CAN Device Net Filter Size
*/
typedef enum {
	DNET_FILTER_DISABLE,        // Device Net Filtering is disabled.
	DNET_FILTER_SIZE_1_BIT,     // Device Net Filter is 1 bit long.
    DNET_FILTER_SIZE_2_BIT,     // Device Net Filter is 2 bits long.
    DNET_FILTER_SIZE_3_BIT,     // Device Net Filter is 3 bits long.
	DNET_FILTER_SIZE_4_BIT,     // Device Net Filter is 4 bits long.
	DNET_FILTER_SIZE_5_BIT,     // Device Net Filter is 5 bits long.
	DNET_FILTER_SIZE_6_BIT,     // Device Net Filter is 6 bits long.
	DNET_FILTER_SIZE_7_BIT,     // Device Net Filter is 7 bits long.
	DNET_FILTER_SIZE_8_BIT,     // Device Net Filter is 8 bits long.
	DNET_FILTER_SIZE_9_BIT,     // Device Net Filter is 9 bits long.
	DNET_FILTER_SIZE_10_BIT,    // Device Net Filter is 10 bits long.
	DNET_FILTER_SIZE_11_BIT,    // Device Net Filter is 11 bits long.
	DNET_FILTER_SIZE_12_BIT,    // Device Net Filter is 12 bits long.
	DNET_FILTER_SIZE_13_BIT,    // Device Net Filter is 13 bits long.
	DNET_FILTER_SIZE_14_BIT,    // Device Net Filter is 14 bits long.
	DNET_FILTER_SIZE_15_BIT,    // Device Net Filter is 15 bits long.
	DNET_FILTER_SIZE_16_BIT,    // Device Net Filter is 16 bits long.
	DNET_FILTER_SIZE_17_BIT,    // Device Net Filter is 17 bits long.
	DNET_FILTER_SIZE_18_BIT,    // Device Net Filter is 18 bits long.	
} DNET_FILTER_SIZE;

/* ------------------------------------------------------------ */
/* CAN Module Events
**
**  TX_EVENT      TX channel event. This event will occur
**                when any of the TX Channel events are active.
**  
**  RX_EVENT      RX channel event. This event will occur
**                when any of the RX Channel events are active.
**
**  TIMESTAMP_TIMER_OVERFLOW_EVENT
**                CAN Timer Stamp Timer Overflow event occurs.
**                This event occurs when the Timestamp Timer has
**                overflowed. 
**
**  OPERATION_MODE_CHANGE_EVENT
**                CAN Operation Mode Change Event. This event
**                occurs when the CAN module has changed it's
**                operating mode successfully.
**
**  RX_OVERFLOW_EVENT
**                CAN RX Channel Overflow Event. This event occurs
**                when any of the RX Channel has overflowed.
**
**  SYSTEM_ERROR_EVENT
**                CAN System Error Event. This event occurs when
**                CAN module tries to access an invalid Device RAM
**                location.
**
**  BUS_ERROR_EVENT
**                CAN Bus Error Event. This event occurs when the 
**                CAN module cannot access the system bus.
**
**  BUS_ACTIVITY_WAKEUP_EVENT
**                CAN Bus Actvity Wakeup. This event occurs when the
**                device is in sleep mode and bus activity is detected
**                on the CAN bus.
**
**  INVALID_RX_MESSAGE_EVENT
**                CAN Bus Invalid RX Message Event. This event occurs
**                when the CAN module receives an Invalid message.
*/
typedef enum {
	TX_EVENT 						= 0x1,
	RX_EVENT 						= 0x2,
	TIMESTAMP_TIMER_OVERFLOW_EVENT 	= 0x4,
	OPERATION_MODE_CHANGE_EVENT 	= 0x8,
    RX_OVERFLOW_EVENT 				= 0x800,
	SYSTEM_ERROR_EVENT 				= 0x1000,
	BUS_ERROR_EVENT 				= 0x2000,
	BUS_ACTIVITY_WAKEUP_EVENT		= 0x4000,
	INVALID_RX_MESSAGE_EVENT 		= 0x8000
} MODULE_EVENT;

/* ------------------------------------------------------------ */
/* CAN Error States
**
**  TX_RX_WARNING_STATE     CAN Module is in a TX or RX warning state.
**  RX_WARNING_STATE        CAN Module is in a RX warning state.
**  TX_WARNING_STATE        CAN Module is in a TX warning state.
**  RX_BUS_PASSIVE_STATE    CAN RX is in a Bus Passive state.
**  TX_BUS_PASSIVE_STATE    CAN TX is in a Bus Passive state.
**  TX_BUS_OFF_STATE        CAN TX is in Bus Off state.
*/
typedef enum {
	TX_RX_WARNING_STATE 	= 0x10000,
	RX_WARNING_STATE 		= 0x20000,
	TX_WARNING_STATE 		= 0x40000,
	RX_BUS_PASSIVE_STATE 	= 0x80000,
    TX_BUS_PASSIVE_STATE 	= 0x100000,
	TX_BUS_OFF_STATE 		= 0x200000
} ERROR_STATE;

/* ------------------------------------------------------------ */
/* CAN Module Features
** This identifies the can be configured during initialization
** of the CAN module.
**
**  STOP_IN_IDLE            Specifies if the CAN module is running
**                          while CPU is in IDLE mode.
**  RX_TIMESTAMP            Specifies if the CAN module will timestamp
**                          every received message.
**  WAKEUP_BUS_FILTER       Specifies if the CAN Wake up on bus activity
**                          RX line filter is enabled.
*/
typedef enum {
	STOP_IN_IDLE = 0x2000,
	RX_TIMESTAMP = 0x100000,
	WAKEUP_BUS_FILTER = 0x400000
} MODULE_FEATURES;

/* ------------------------------------------------------------ */
/* CAN Channel Masks
*/
typedef enum {
	CHANNEL0_MASK  = 0x00000001,    // Channel 0 Mask
	CHANNEL1_MASK  = 0x00000002,    // Channel 1 Mask
	CHANNEL2_MASK  = 0x00000004,    // Channel 2 Mask
	CHANNEL3_MASK  = 0x00000008,    // Channel 3 Mask
	CHANNEL4_MASK  = 0x00000010,    // Channel 4 Mask
	CHANNEL5_MASK  = 0x00000020,    // Channel 5 Mask
	CHANNEL6_MASK  = 0x00000040,    // Channel 6 Mask
	CHANNEL7_MASK  = 0x00000080,    // Channel 7 Mask
	CHANNEL8_MASK  = 0x00000100,    // Channel 8 Mask
	CHANNEL9_MASK  = 0x00000200,    // Channel 9 Mask
	CHANNEL10_MASK = 0x00000400,    // Channel 10 Mask
	CHANNEL11_MASK = 0x00000800,    // Channel 11 Mask
	CHANNEL12_MASK = 0x00001000,    // Channel 12 Mask
	CHANNEL13_MASK = 0x00002000,    // Channel 13 Mask
	CHANNEL14_MASK = 0x00004000,    // Channel 14 Mask
	CHANNEL15_MASK = 0x00008000,    // Channel 15 Mask
	CHANNEL16_MASK = 0x00010000,    // Channel 16 Mask
	CHANNEL17_MASK = 0x00020000,    // Channel 17 Mask
	CHANNEL18_MASK = 0x00040000,    // Channel 18 Mask
	CHANNEL19_MASK = 0x00080000,    // Channel 19 Mask
	CHANNEL20_MASK = 0x00100000,    // Channel 20 Mask
	CHANNEL21_MASK = 0x00200000,    // Channel 21 Mask
	CHANNEL22_MASK = 0x00400000,    // Channel 22 Mask
	CHANNEL23_MASK = 0x00800000,    // Channel 23 Mask
	CHANNEL24_MASK = 0x01000000,    // Channel 24 Mask
	CHANNEL25_MASK = 0x02000000,    // Channel 25 Mask
	CHANNEL26_MASK = 0x04000000,    // Channel 26 Mask
	CHANNEL27_MASK = 0x08000000,    // Channel 27 Mask
	CHANNEL28_MASK = 0x10000000,    // Channel 28 Mask
	CHANNEL29_MASK = 0x20000000,    // Channel 29 Mask
	CHANNEL30_MASK = 0x40000000,    // Channel 30 Mask
	CHANNEL31_MASK = 0x80000000,    // Channel 31 Mask
	ANYCHANNEL_MASK = 0xFFFFFFFF    // Channel any channel Mask
} CHANNEL_MASK;

/* ------------------------------------------------------------ */
/* CAN TX Channel Condition
** This identifies the possible TX channel conditions.
**
**  TX_CHANNEL_TRANSMITTING         TX Channel is currently Transmitting.
**  TX_CHANNEL_ERROR                TX Channel Error has occurred.
**  TX_CHANNEL_ARBITRATION_LOST     TX Channel lost arbitration.
*/
typedef enum {
	TX_CHANNEL_TRANSMITTING = 0x8,
	TX_CHANNEL_ERROR        = 0x10,
	TX_CHANNEL_ARBITRATION_LOST = 0x20
} TX_CHANNEL_CONDITION;

/* ------------------------------------------------------------ */
/* CAN TX Message Standard ID
** Part of the transmit message structure.
** This value should be between 0x0 - 0x7FF.
*/
typedef	struct 
{
	unsigned SID:11;
	unsigned :21;
} TX_MSG_SID;

/* ------------------------------------------------------------ */
/* CAN Messgage EID
** This is part of the message structure for TX and RX messages.
*/
typedef struct 
{
	unsigned DLC:4;     // Data Length Control. Specifies the size of the
                        // data payload section of the CAN packet. Valid
                        // values are 0x0 - 0x8.
	unsigned RB0:1;     // Reserved bit. Should be always 0.
	unsigned :3;
	unsigned RB1:1;     // Reserved bit. Should be always 0.
	unsigned RTR:1;     // Remote Transmit Request bit. Should be set for
                        // RTR messages, clear otherwise.
	unsigned EID:18;    // CAN TX and RX Extended ID field. Valid values
                        // are in range 0x0 - 0x3FFFF. 
	unsigned IDE:1;     // Identifier bit. If 0 means that message is SID.
                        // If 1 means that message is EID type.
	unsigned SRR:1;     // Susbtitute Remote request bit. This bit should
                        // always be clear for an EID message. It is ignored
                        // for an SID message.
	unsigned :2;
} MSG_EID;

/* ------------------------------------------------------------ */
/* CAN Transmit Buffer
*/
typedef union {
    /* This is the CAN TX message as a structure.
    */
	struct {
		TX_MSG_SID  msgSID;      // This is SID portion of the CAN TX message.        
		MSG_EID     msgEID;      // This is EID portion of the CAN TX message.        
		uint8_t     data[8];     // This is the data portion of the CAN TX message.
	};

    /* This is CAN TX message organized as a set of 32 bit words.
    */
	uint32_t    messageWord[4];

} TxMessageBuffer;

/* ------------------------------------------------------------ */
/* CAN RX Message SID
** This structure defines the SID section of the RX message
*/
typedef	struct {     
	unsigned SID:11;    // SID of the Received CAN Message.    
	unsigned FILHIT:5;  // Filter which accepted this message.    
	unsigned CMSGTS:16; // Time stamp of the received message. This is
                        // valid only if the Timestamping is enabled.
} RX_MSG_SID;

/* ------------------------------------------------------------ */
/* CAN RX Message Buffer
** This structure defines the receive message buffer.
** A message received as a full message contains the header
** A message received data only contains on the message payload.
*/
typedef union {
    /* This structure is used for a full message.
    */
	struct {        
		RX_MSG_SID  msgSID;     // This is SID portion of the CAN RX message.        
		MSG_EID     msgEID;     // This is EID portion of the CAN RX message        
		uint8_t     data[8];    // This is the data payload section of the 
                                // received message.
	};

    /* This is used if the message buffer is to be read from a 
    ** Data-Only type of CAN RX Channel.
    */
    uint8_t     dataOnlyMsgData[8];

    /* This is the CAN RX message organized as a set of 32 bit words.
    */
	uint32_t    messageWord[4];

} RxMessageBuffer;

/* ------------------------------------------------------------ */
/*          CAN Object Class Member Variables                   */
/* ------------------------------------------------------------ */
private:
    MODULE  mod;


/* ------------------------------------------------------------ */
/*          CAN Object Class Member Functions                  */
/* ------------------------------------------------------------ */
public:
            CAN(MODULE modNew);
           ~CAN();

    /* Interrupt methods
    */
    void        attachInterrupt(void (*pfn)());
    void        detachInterrupt();

    /* Configuration Methods
    */
    void        assignMemoryBuffer(void * buf, uint32_t size);
    void        setOperatingMode(OP_MODE opm);
    OP_MODE     getOperatingMode();
    void        enableFeature(MODULE_FEATURES features, bool enable);
    void        deviceNetFilter(DNET_FILTER_SIZE dncnt);
    void        setTimeStampValue(uint32_t val);
    uint32_t    getTimeStampValue();
    void        setTimeStampPrescalar(uint32_t pre);
    void        enableModule(bool enable);
    void        setSpeed(const BIT_CONFIG * cfg, uint32_t clk, uint32_t spd);
    bool        isActive();
    void        resetChannel(CHANNEL chn);
    bool        isChannelReset(CHANNEL chn);
    void        updateChannel(CHANNEL chn);

    /* Event Management Methods
    */
    void        enableModuleEvent(MODULE_EVENT evt, bool enable);
    MODULE_EVENT getModuleEvent();
    void        clearModuleEvent(MODULE_EVENT evt);
    void        enableChannelEvent(CHANNEL chn, CHANNEL_EVENT evt, bool enable);
    EVENT_CODE  getPendingEventCode();
    CHANNEL_MASK getAllChannelEventStatus();
    CHANNEL_MASK getAllChannelOverflowStatus();
    CHANNEL_EVENT getChannelEvent(CHANNEL chn);
    void        clearChannelEvent(CHANNEL chn, CHANNEL_EVENT evt);

    /* Message Transmit Functions
    */
    void        configureChannelForTx(CHANNEL chn, uint32_t size, TX_RTR rtren, TXCHANNEL_PRIORITY pr);
    void        abortPendingTx(CHANNEL chn);
    void        flushTxChannel(CHANNEL chn);
    TX_CHANNEL_CONDITION getTxChannelCondition(CHANNEL chn);
    TxMessageBuffer * getTxMessageBuffer(CHANNEL chn);
    bool        isTxAborted(CHANNEL chn);

    /* CAN Message Receive Functions
    */
    void        configureChannelForRx(CHANNEL chn, uint32_t size, RX_DATA_MODE dataOnly);
    RxMessageBuffer * getRxMessage(CHANNEL chn);

    /* Message Filtering Functions
    */
    void        configureFilterMask(FILTER_MASK mask, uint32_t maskbits, ID_TYPE id, FILTER_MASK_TYPE mide);
    void        configureFilter(FILTER filter, uint32_t id, ID_TYPE type);
    void        enableFilter(FILTER filter, bool enable);
    FILTER      getLatestFilterHit();
    void        linkFilterToChannel(FILTER filter, FILTER_MASK mask, CHANNEL chn);
    bool        isFilterDisabled(FILTER filter);

    /* Error State Tracking
    */
    uint32_t    getRxErrorCount();
    uint32_t    getTxErrorCount();
    ERROR_STATE getErrorState();

    /* Information Functions
    */
    uint32_t    totalModules();
    uint32_t    totalChannels();
    uint32_t    totalFilters();
    uint32_t    totalMasks();   
};


/* ------------------------------------------------------------ */
/*                  Variable Declarations                       */
/* ------------------------------------------------------------ */



/* ------------------------------------------------------------ */
/*                  Procedure Declarations                      */
/* ------------------------------------------------------------ */



/* ------------------------------------------------------------ */

#endif  //CHIPKIT_CAN_H

/************************************************************************/
