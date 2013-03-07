//*****************************************************************************************
//*	CAN LIBRARY AND DEMO APPLICATION FOR chipKIT MAX32 and chipKIT Network Shield
//* 
//* Compiled using MPLAB C32 Version 2.00
//*
//* Assembled and Modified for Digilent Inc. by Fred Eady
//* Revision 1.0
//* 08/19/2011
//* 
//* This CAN demo application provides a basic framework for intitalizing the CAN module, 
//* transmitting data and receiving data. The demo can be run on a single chipKIT CAN 
//* node or between a pair of chipKIT nodes. Each chipKIT node must be equipped with a
//* chipKIT Network Shield. The application will display an ASCII 1 or ASCII 2 on a 
//* 9600bps HyperTerminal session.
//*****************************************************************************************
//*****************************************************************************************
//* APPLICATION INCLUDES
//*****************************************************************************************
#include <p32xxxx.h>
#include <peripheral\system.h>
#include <sys/kmem.h>
#include <plib.h>
#include <compiler.h>
#include "GenericTypeDefs.h"
//*****************************************************************************************
//* FUNCTION DECLARATIONS
//*****************************************************************************************
void init_chipKIT(void);
void init_uart1(void);
void init_can1(UINT32 myaddr);
void init_can2(UINT32 myaddr);
void txCAN1(UINT32 rxnode);
void txCAN2(UINT32 rxnode);
void rxCAN1(void);
void rxCAN2(void);
//*****************************************************************************************
//* APPLICATION SPEEDS AND FEEDS
//*****************************************************************************************
#define GetSystemClock()            80000000UL
#define GetPeripheralClock()        80000000UL  
#define GetInstructionClock()       (GetSystemClock() / 2)
#define SYS_FREQ 					(80000000L)
#define CAN_BUS_SPEED 250000		// CAN Speed
//#define CAN1_BRPVAL 0x0F			// this value is calculated by CANSetSpeed 
//#define CAN2_BRPVAL 0x0F			// 0x07 for 500Kbps
									// 0x0F for 250Kbps
									// 0x03	for 1Mbps
									// Time Quanta per bit set for 10
//*****************************************************************************************
//* NODE ID/ADDRESS TABLE
//*****************************************************************************************
#define	node1can1	0x101
#define node2can1	0x201
#define node1can2	0x102
#define node2can2	0x202
//*****************************************************************************************
//* PIC32MX795F512L CONFIGURATION 
//*****************************************************************************************
#pragma config FCANIO	= OFF			// Use Alternate Pins for chipKIT MAX32
#pragma config UPLLEN   = ON            // USB PLL Enabled
#pragma config FPLLMUL  = MUL_20        // PLL Multiplier
#pragma config UPLLIDIV = DIV_2         // USB PLL Input Divider
#pragma config FPLLIDIV = DIV_2         // PLL Input Divider
#pragma config FPLLODIV = DIV_1         // PLL Output Divider
#pragma config FPBDIV   = DIV_1         // Peripheral Clock divisor
#pragma config FWDTEN   = OFF           // Watchdog Timer
#pragma config WDTPS    = PS1           // Watchdog Timer Postscale
#pragma config FCKSM    = CSDCMD        // Clock Switching & Fail Safe Clock Monitor
#pragma config OSCIOFNC = OFF           // CLKO Enable
#pragma config POSCMOD  = HS            // Primary Oscillator
#pragma config IESO     = OFF           // Internal/External Switch-over
#pragma config FSOSCEN  = OFF           // Secondary Oscillator Enable (KLO was off)
#pragma config FNOSC    = PRIPLL        // Oscillator Selection
#pragma config CP       = OFF           // Code Protect
#pragma config BWP      = OFF           // Boot Flash Write Protect
#pragma config PWP      = OFF           // Program Flash Write Protect
#pragma config ICESEL   = ICS_PGx2      // ICE/ICD Comm Channel Select
#pragma config DEBUG    = ON            // Background Debugger Enable
//*****************************************************************************************
//* CAN MESSAGE AREAS
//*****************************************************************************************
UINT8 CAN1MessageFifoArea[2 * 8 * 16];
UINT8 CAN2MessageFifoArea[2 * 8 * 16];
//*****************************************************************************************
//* CAN INTERRUPT FLAGS
//*****************************************************************************************
static volatile BOOL isCAN1MsgReceived = FALSE;
static volatile BOOL isCAN2MsgReceived = FALSE;
//*****************************************************************************************
//* INITIALIZE chipKIT
//*****************************************************************************************
void init_chipKIT(void)
{
	init_uart1();
	init_can1(node1can1);
	init_can2(node1can2);
    SYSTEMConfig(SYS_FREQ, SYS_CFG_WAIT_STATES | SYS_CFG_PCACHE);
    INTConfigureSystem(INT_SYSTEM_CONFIG_MULT_VECTOR);
	AD1PCFGSET = 0xFFFFFFFF;
	INTEnableInterrupts();
}
//*****************************************************************************************
//* INITIALIZE UART1
//*****************************************************************************************
void init_uart1(void)
{
	UARTConfigure(UART1, UART_ENABLE_PINS_TX_RX_ONLY);
    //UARTSetFifoMode(UART1, UART_INTERRUPT_ON_TX_NOT_FULL | UART_INTERRUPT_ON_RX_NOT_EMPTY);
    UARTSetLineControl(UART1, UART_DATA_SIZE_8_BITS | UART_PARITY_NONE | UART_STOP_BITS_1);
    UARTSetDataRate(UART1, GetPeripheralClock(), 9600);
    UARTEnable(UART1, UART_ENABLE_FLAGS(UART_PERIPHERAL | UART_RX | UART_TX));
}
//*****************************************************************************************
//* INITIALIZE CAN1 
//*****************************************************************************************
void init_can1(UINT32 myaddr)
{
    CAN_BIT_CONFIG canBitConfig;

	/* Step 1: Switch the CAN module
	 * ON and switch it to Configuration
	 * mode. Wait till the switch is 
	 * complete */

	CANEnableModule(CAN1,TRUE);

	CANSetOperatingMode(CAN1, CAN_CONFIGURATION);
	while(CANGetOperatingMode(CAN1) != CAN_CONFIGURATION);			

	/* Step 2: Configure the CAN Module Clock. The
	 * CAN_BIT_CONFIG data structure is used
	 * for this purpose. The propagation, 
	 * phase segment 1 and phase segment 2
	 * are configured to have 3TQ. The CANSetSpeed()
     * function sets the baud. */
	
	canBitConfig.phaseSeg2Tq            = CAN_BIT_3TQ;
	canBitConfig.phaseSeg1Tq            = CAN_BIT_3TQ;
	canBitConfig.propagationSegTq       = CAN_BIT_3TQ;
	canBitConfig.phaseSeg2TimeSelect    = TRUE;
	canBitConfig.sample3Time            = TRUE;
    canBitConfig.syncJumpWidth          = CAN_BIT_2TQ;

   CANSetSpeed(CAN1,&canBitConfig,SYS_FREQ,CAN_BUS_SPEED);

	/* Step 3: Assign the buffer area to the
     * CAN module.
     */ 
/* Note the size of each Channel area.
 * It is 2 (Channels) * 8 (Messages Buffers) 
 * 16 (bytes/per message buffer) bytes. Each 
 * CAN module should have its own message 
 * area. */
	CANAssignMemoryBuffer(CAN1,CAN1MessageFifoArea,2 * 8 * 16);	

	/* Step 4: Configure channel 0 for TX and size of
     * 8 message buffers with RTR disabled and low medium
     * priority. Configure channel 1 for RX and size
     * of 8 message buffers and receive the full message.
     */

    CANConfigureChannelForTx(CAN1,CAN_CHANNEL0,8,CAN_TX_RTR_DISABLED,CAN_LOW_MEDIUM_PRIORITY);
    CANConfigureChannelForRx(CAN1,CAN_CHANNEL1,8,CAN_RX_FULL_RECEIVE);
	
	/* Step 5: Configure filters and mask. Configure
     * filter 0 to accept SID messages with ID 0x200.
     * Configure filter mask 0 to compare all the ID
     * bits and to filter by the ID type specified in
     * the filter configuration. Filter 0 accepted 
     * messages are stored in channel 1.  */

    CANConfigureFilter      (CAN1, CAN_FILTER0, myaddr, CAN_SID);    
    CANConfigureFilterMask  (CAN1, CAN_FILTER_MASK0, 0xFFF, CAN_SID, CAN_FILTER_MASK_IDE_TYPE);
    CANLinkFilterToChannel  (CAN1, CAN_FILTER0, CAN_FILTER_MASK0, CAN_CHANNEL1); 
    CANEnableFilter         (CAN1, CAN_FILTER0, TRUE);
	
	/* Step 6: Enable interrupt and events. Enable the receive
     * channel not empty event (channel event) and the receive
     * channel event (module event).
     * The interrrupt peripheral library is used to enable
     * the CAN interrupt to the CPU. */

    CANEnableChannelEvent(CAN1, CAN_CHANNEL1, CAN_RX_CHANNEL_NOT_EMPTY, TRUE);
    CANEnableModuleEvent(CAN1, CAN_RX_EVENT, TRUE);

    /* These functions are from interrupt peripheral
     * library. */
     
    INTSetVectorPriority(INT_CAN_1_VECTOR, INT_PRIORITY_LEVEL_4);
    INTSetVectorSubPriority(INT_CAN_1_VECTOR, INT_SUB_PRIORITY_LEVEL_0);
    INTEnable(INT_CAN1, INT_ENABLED);

	/* Step 7: Switch the CAN mode
	 * to normal mode. */

	CANSetOperatingMode(CAN1, CAN_NORMAL_OPERATION);
	while(CANGetOperatingMode(CAN1) != CAN_NORMAL_OPERATION);			

}
//*****************************************************************************************
//* INITIALIZE CAN2
//*****************************************************************************************
void init_can2(UINT32 myaddr)
{
    CAN_BIT_CONFIG canBitConfig;

	/* Step 1: Switch the CAN module
	 * ON and switch it to Configuration
	 * mode. Wait till the switch is 
	 * complete */

	CANEnableModule(CAN2,TRUE);

	CANSetOperatingMode(CAN2, CAN_CONFIGURATION);
	while(CANGetOperatingMode(CAN2) != CAN_CONFIGURATION);			

	/* Step 2: Configure the CAN Module Clock. The
	 * CAN_BIT_CONFIG data structure is used
	 * for this purpose. The propagation, 
	 * phase segment 1 and phase segment 2
	 * are configured to have 3TQ. The CANSetSpeed()
     * function sets the baud. */
	
	canBitConfig.phaseSeg2Tq            = CAN_BIT_3TQ;
	canBitConfig.phaseSeg1Tq            = CAN_BIT_3TQ;
	canBitConfig.propagationSegTq       = CAN_BIT_3TQ;
	canBitConfig.phaseSeg2TimeSelect    = TRUE;
	canBitConfig.sample3Time            = TRUE;
    canBitConfig.syncJumpWidth          = CAN_BIT_2TQ;

   CANSetSpeed(CAN2,&canBitConfig,SYS_FREQ,CAN_BUS_SPEED);

	/* Step 3: Assign the buffer area to the
     * CAN module.
     */ 
/* Note the size of each Channel area.
 * It is 2 (Channels) * 8 (Messages Buffers) 
 * 16 (bytes/per message buffer) bytes. Each 
 * CAN module should have its own message 
 * area. */
	CANAssignMemoryBuffer(CAN2,CAN2MessageFifoArea,2 * 8 * 16);	

	/* Step 4: Configure channel 0 for TX and size of
     * 8 message buffers with RTR disabled and low medium
     * priority. Configure channel 1 for RX and size
     * of 8 message buffers and receive the full message.
     */

    CANConfigureChannelForTx(CAN2,CAN_CHANNEL0,8,CAN_TX_RTR_DISABLED,CAN_LOW_MEDIUM_PRIORITY);
    CANConfigureChannelForRx(CAN2,CAN_CHANNEL1,8,CAN_RX_FULL_RECEIVE);
	
	/* Step 5: Configure filters and mask. Configure
     * filter 0 to accept SID messages with ID 0x200.
     * Configure filter mask 0 to compare all the ID
     * bits and to filter by the ID type specified in
     * the filter configuration. Filter 0 accepted 
     * messages are stored in channel 1.  */

    CANConfigureFilter      (CAN2, CAN_FILTER0, myaddr, CAN_SID);    
    CANConfigureFilterMask  (CAN2, CAN_FILTER_MASK0, 0xFFF, CAN_SID, CAN_FILTER_MASK_IDE_TYPE);
    CANLinkFilterToChannel  (CAN2, CAN_FILTER0, CAN_FILTER_MASK0, CAN_CHANNEL1); 
    CANEnableFilter         (CAN2, CAN_FILTER0, TRUE);
	
	/* Step 6: Enable interrupt and events. Enable the receive
     * channel not empty event (channel event) and the receive
     * channel event (module event).
     * The interrrupt peripheral library is used to enable
     * the CAN interrupt to the CPU. */

    CANEnableChannelEvent(CAN2, CAN_CHANNEL1, CAN_RX_CHANNEL_NOT_EMPTY, TRUE);
    CANEnableModuleEvent(CAN2, CAN_RX_EVENT, TRUE);

    /* These functions are from interrupt peripheral
     * library. */
     
    INTSetVectorPriority(INT_CAN_2_VECTOR, INT_PRIORITY_LEVEL_4);
    INTSetVectorSubPriority(INT_CAN_2_VECTOR, INT_SUB_PRIORITY_LEVEL_0);
    INTEnable(INT_CAN2, INT_ENABLED);

	/* Step 7: Switch the CAN mode
	 * to normal mode. */

	CANSetOperatingMode(CAN2, CAN_NORMAL_OPERATION);
	while(CANGetOperatingMode(CAN2) != CAN_NORMAL_OPERATION);			

}
//*****************************************************************************************
//* CAN1 TRANSMIT
//*****************************************************************************************
void txCAN1(UINT32 rxnode)
{
	CANTxMessageBuffer * message;
	message = CANGetTxMessageBuffer(CAN1,CAN_CHANNEL0);

	if(message != NULL)
      {
		// clear buffer
        message->messageWord[0] = 0;
        message->messageWord[1] = 0;
        message->messageWord[2] = 0;
        message->messageWord[3] = 0;

        message->msgSID.SID 	= rxnode;	//receiving node		
        message->msgEID.IDE 	= 0;			
        message->msgEID.DLC 	= 1;			
        message->data[0]        = 0x31;
        //message->data[1]        = 0x31;
        //message->data[2]        = 0x32;
        //message->data[3]        = 0x33;
        //message->data[4]        = 0x34;
        //message->data[5]        = 0x35;
        //message->data[6]        = 0x36;
        //message->data[7]        = 0x37;

        /* This function lets the CAN module
         * know that the message processing is done
         * and message is ready to be processed. */

        CANUpdateChannel(CAN1,CAN_CHANNEL0);

        /* Direct the CAN module to flush the
         * TX channel. This will send any pending
         * message in the TX channel. */

        CANFlushTxChannel(CAN1,CAN_CHANNEL0);
    }	

}
//*****************************************************************************************
//* CAN2 TRANSMIT
//*****************************************************************************************
void txCAN2(UINT32 rxnode)
{
	CANTxMessageBuffer * message;
	message = CANGetTxMessageBuffer(CAN2,CAN_CHANNEL0);

	if( message != NULL)
      {
        message->messageWord[0] = 0;
        message->messageWord[1] = 0;
        message->messageWord[2] = 0;
        message->messageWord[3] = 0;

        message->msgSID.SID 	= rxnode;	//receiving node	
        message->msgEID.IDE 	= 0;			
        message->msgEID.DLC 	= 1;			
        message->data[0]        = 0x32;
        //message->data[1]        = 0x31;
        //message->data[2]        = 0x32;
        //message->data[3]        = 0x33;
        //message->data[4]        = 0x34;
        //message->data[5]        = 0x35;
        //message->data[6]        = 0x36;
        //message->data[7]        = 0x37;

        /* This function lets the CAN module
         * know that the message processing is done
         * and message is ready to be processed. */

        CANUpdateChannel(CAN2,CAN_CHANNEL0);

        /* Direct the CAN module to flush the
         * TX channel. This will send any pending
         * message in the TX channel. */

        CANFlushTxChannel(CAN2,CAN_CHANNEL0);
    }	

}
//*****************************************************************************************
//* CAN1 RECEIVE
//*****************************************************************************************
void rxCAN1(void)
{
	CANRxMessageBuffer * message;

	if(isCAN1MsgReceived == FALSE)
	{
		/* CAN2 did not receive any message
		 * so exit the function. Note that the
		 * isCAN2MsgReceived flag is updated 
		 * by the CAN2 ISR. */

		return;
	}
	
	/* Message was received. Reset isCAN2MsgReceived flag
     * to catch the next message. */

	isCAN1MsgReceived = FALSE;	
	
	message = CANGetRxMessage(CAN1,CAN_CHANNEL1);

		if (UARTTransmitterIsReady(UART1))
		{
			UARTSendDataByte(UART1, message->data[0]);
		}

	/* Call the CANUpdateChannel() function to let
     * the CAN module know that the message processing
     * is done. Enable the event so that the CAN module
     * generates an interrupt when the event occurs.*/


    CANUpdateChannel(CAN1, CAN_CHANNEL1);
	CANEnableChannelEvent(CAN1, CAN_CHANNEL1, CAN_RX_CHANNEL_NOT_EMPTY, TRUE);

}
//*****************************************************************************************
//* CAN2 RECEIVE
//*****************************************************************************************
void rxCAN2(void)
{
	CANRxMessageBuffer * message;

	if(isCAN2MsgReceived == FALSE)
	{
		/* CAN2 did not receive any message
		 * so exit the function. Note that the
		 * isCAN2MsgReceived flag is updated 
		 * by the CAN2 ISR. */

		return;
	}
	
	/* Message was received. Reset isCAN2MsgReceived flag
     * to catch the next message. */

	isCAN2MsgReceived = FALSE;	
	
	message = CANGetRxMessage(CAN2,CAN_CHANNEL1);

		if (UARTTransmitterIsReady(UART1))
		{
			UARTSendDataByte(UART1, message->data[0]);
		}

	/* Call the CANUpdateChannel() function to let
     * the CAN module know that the message processing
     * is done. Enable the event so that the CAN module
     * generates an interrupt when the event occurs.*/


    CANUpdateChannel(CAN2, CAN_CHANNEL1);
	CANEnableChannelEvent(CAN2, CAN_CHANNEL1, CAN_RX_CHANNEL_NOT_EMPTY, TRUE);

}
//*****************************************************************************************
//* CAN1 INTERRUPT HANDLER
//*****************************************************************************************
void __attribute__((vector(46), interrupt(ipl4), nomips16)) CAN1InterruptHandler(void)
{
	/* This is the CAN1 Interrupt Handler.
	 * Note that there are many source events in the
	 * CAN1 module for this interrupt. These
	 * events are enabled by the  CANEnableModuleEvent()
     * function. In this example, only the RX_EVENT
	 * is enabled. */


	/* Check if the source of the interrupt is
	 * RX_EVENT. This is redundant since only this
     * event is enabled in this example but
     * this shows one scheme for handling events. */

	if((CANGetModuleEvent(CAN1) & CAN_RX_EVENT) != 0)
	{
		
		/* Within this, you can check which channel caused the 
		 * event by using the CANGetModuleEvent() function
         * which returns a code representing the highest priority
         * pending event. */ 
		
		if(CANGetPendingEventCode(CAN1) == CAN_CHANNEL1_EVENT)
		{
			/* This means that channel 1 caused the event.
			 * The CAN_RX_CHANNEL_NOT_EMPTY event is persistent. You
			 * could either read the channel in the ISR
			 * to clear the event condition or as done 
			 * here, disable the event source, and set
			 * an application flag to indicate that a message
			 * has been received. The event can be
			 * enabled by the application when it has processed
			 * one message.
			 *
			 * Note that leaving the event enabled would
			 * cause the CPU to keep executing the ISR since
			 * the CAN_RX_CHANNEL_NOT_EMPTY event is persistent (unless
			 * the not empty condition is cleared.) 
			 * */
			
            CANEnableChannelEvent(CAN1, CAN_CHANNEL1, CAN_RX_CHANNEL_NOT_EMPTY, FALSE);
			isCAN1MsgReceived = TRUE;	
		}
	}

   /* The CAN1 Interrupt flag is  cleared at the end of the 
	* interrupt routine. This is because the event source
    * that could have caused this interrupt  to occur 
    * (CAN_RX_CHANNEL_NOT_EMPTY) is disabled. Attempting to 
	* clear the CAN1 interrupt flag when the the CAN_RX_CHANNEL_NOT_EMPTY
    * interrupt is enabled will not have any effect because the 
	* base event is still present. */ 
	
	INTClearFlag(INT_CAN1);

}
//*****************************************************************************************
//* CAN2 INTERRUPT HANDLER
//*****************************************************************************************
void __attribute__((vector(47), interrupt(ipl4), nomips16)) CAN2InterruptHandler(void)
{
    
	/* This is the CAN2 Interrupt Handler. Note that there
     * are many events in the CAN2 module that can cause
     * this interrupt. These events are enabled by the  
     * CANEnableModuleEvent() function. In this example, 
     * only the RX_EVENT is enabled. */


	/* Check if the source of the interrupt is RX_EVENT. 
     * This is redundant  since only this event is enabled
     * in this example but this shows one scheme for handling
	 * interrupts. */

	if((CANGetModuleEvent(CAN2) & CAN_RX_EVENT) != 0)
	{
		
		/* Within this, you can check which event caused the 
		 * interrupt by using the CANGetPendingEventCode() function
         * to get a code representing the highest priority active
         * event.*/ 
		
		if(CANGetPendingEventCode(CAN2) == CAN_CHANNEL1_EVENT)
		{
			/* This means that channel 1 caused the event.
			 * The CAN_RX_CHANNEL_NOT_EMPTY event is persistent. You
			 * could either read the channel in the ISR
			 * to clear the event condition or as done 
			 * here, disable the event source, and set
			 * an application flag to indicate that a message
			 * has been received. The event can be
			 * enabled by the application when it has processed
			 * one message.
			 *
			 * Note that leaving the event enabled would
			 * cause the CPU to keep executing the ISR since
			 * the CAN_RX_CHANNEL_NOT_EMPTY event is persistent (unless
			 * the not empty condition is cleared.) 
			 * */
			
            CANEnableChannelEvent(CAN2, CAN_CHANNEL1, CAN_RX_CHANNEL_NOT_EMPTY, FALSE);
			isCAN2MsgReceived = TRUE;	
		}
	}

   /* The CAN2 Interrupt flag is  cleared at the end of the 
	* interrupt routine. This is because the event
    * that could have caused this interrupt  to occur 
    * (CAN_RX_CHANNEL_NOT_EMPTY) is disabled. Attempting to 
	* clear the CAN2 interrupt flag when the the CAN_RX_CHANNEL_NOT_EMPTY
    * interrupt is enabled will not have any effect because the 
	* base event is still present. */ 
	
	INTClearFlag(INT_CAN2);
}
//*****************************************************************************************
//* MAIN APPLICATION ENTRY
//*****************************************************************************************
void main(void)
{
	UINT32 loopy,leapy;
	init_chipKIT();

	do{
	// send ASCII 2 to CAN1	
	txCAN2(node1can1);
	// sloppy delay routine
		for(loopy=0;loopy<0xffff;++loopy)
			{
				leapy = 0x3F;
				while(--leapy);
			}
	// receive ASCII data from CAN2
		rxCAN1();
		
	// send ASCII 1 to CAN2
		txCAN1(node1can2);
	// sloppy delay routine
			for(loopy=0;loopy<0xffff;++loopy)
			{
				leapy = 0x3F;
				while(--leapy);
			}
	// receive ASCII data from CAN1
		rxCAN2();

	}while(1);
}
