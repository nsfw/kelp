/************************************************************************/
/*									*/
/*  CanDemo.pde	-- Example CAN Sketch for chipKIT Max32/Network Shield	*/
/*									*/
/************************************************************************/
/*  Author:	Gene Apperson						*/
/*  Copyright (c) 2011, Digilent Inc.  	    			        */
/************************************************************************/
/*  This sketch is derived from a CAN demonstration program written     */
/*  by Fred Eady. It is essentially his program translated to use the   */
/*  chipKIT CAN low level library rather than using the Microchip C32   */
/*  Peripheral Library CAN functions directly.                          */
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
/*  Module Description:							*/
/*									*/
/*  This sketch is an example of using the CAN controllers on the       */
/*  on the chipKIT Max32 with the Network Shield. It illustrates the    */
/*  use of the low level CAN library provided for use with the Network  */
/*  shield.                                                             */
/*									*/
/*  This sketch assumes that CAN1 is looped back to CAN2 on the Network */
/*  shield. It initializes the two CAN modules, sends a packet          */
/*  containing a single character from CAN2 to CAN1, prints the         */
/*  character received by CAN1, then sends a packet containing a single */
/*  character from CAN1 to CAN2 and prints the character received by    */
/*  CAN2. The sending of packets back and forth is repeated for ever.   */
/*									*/
/************************************************************************/
/*  Revision History:							*/
/*									*/
/*  08/21/2011(GeneApperson): Created                                   */
/*									*/
/************************************************************************/


/* ------------------------------------------------------------ */
/*		Include File Definitions			*/
/* ------------------------------------------------------------ */

#include  <WProgram.h>

#include  "chipKITCAN.h"

/* ------------------------------------------------------------ */
/*		Local Type and Constant Definitions		*/
/* ------------------------------------------------------------ */

/* Network Node Addresses
*/
#define	node1can1	0x101L
#define node2can1	0x201L
#define node1can2	0x102L
#define node2can2	0x202L

#define SYS_FREQ	(80000000L)
#define CAN_BUS_SPEED   250000		// CAN Speed

/* ------------------------------------------------------------ */
/*		Global Variables				*/
/* ------------------------------------------------------------ */

/* CAN controller interface object instances.
*/
CAN    canMod1(CAN::CAN1);    // this object uses CAN module 1
CAN    canMod2(CAN::CAN2);    // this object uses CAN module 2

/* ------------------------------------------------------------ */
/*		Local Variables					*/
/* ------------------------------------------------------------ */

/* CAN Message Buffers
*/
uint8_t  CAN1MessageFifoArea[2 * 8 * 16];
uint8_t  CAN2MessageFifoArea[2 * 8 * 16];

/* These are used as event flags by the interrupt service routines.
*/
static volatile bool isCAN1MsgReceived = false;
static volatile bool isCAN2MsgReceived = false;

/* ------------------------------------------------------------ */
/*		Forward Declarations				*/
/* ------------------------------------------------------------ */

void initCan1(uint32_t myaddr);
void initCan2(uint32_t myaddr);
void doCan1Interrupt();
void doCan2Interrupt();
void txCAN1(uint32_t rxnode);
void txCAN2(uint32_t rxnode);
void rxCAN1(void);
void rxCAN2(void);
void doCan1Interrupt();
void doCan2Interrupt();

/* ------------------------------------------------------------ */
/*		Procedure Definitions				*/
/* ------------------------------------------------------------ */
/***  setup
**
**  Parameters:
**    none
**
**  Return Value:
**    none
**
**  Errors:
**    none
**
**  Description:
**    Initialize the program for execution. Initialize the
**    CAN controller modules before use. Install the interrupt
**    service routines used to indicate packet reception.
**    Initialize the serial interface to print the activity
**    to the serial monitor.
*/

void
setup() {    

  /* Init each CAN controller module for use.
  */ 
  initCan1(node1can1);
  initCan2(node1can2);

  /* Install the interrupt service routines.
  */  
  canMod1.attachInterrupt(doCan1Interrupt);
  canMod2.attachInterrupt(doCan2Interrupt);

  /* Set up the serial monitor to show program activity.
  */  
  Serial.begin(9600);
  
}

/* ------------------------------------------------------------ */
/***  loop
**
**  Parameters:
**
**  Return Value:
**
**  Errors:
**
**  Description:
**    Program event loop. This function is called repeatedly forever
**    after setup has been executed. Send a packet from CAN2 to CAN1.
**    Receive the packet on CAN1 and print the result. Send a
**    packet from CAN1 to CAN2. Have CAN2 receive the packet and
**    print the result.
*/

void
loop() {
  
  /* Send an ASCII character from CAN2 to CAN1
  ** Note: The txCAN2 function initializes the transmit buffer.
  */
  txCAN2(node1can1);

  /* Receive the character from CAN2
  */
  delay(100);    //wait so that the character has time to be delivered  
  rxCAN1();

  /* Send an ASCII character from CAN1 to CAN2
  */
  txCAN1(node1can2);
  
  /* Receive the character from CAN1
  */
  delay(100);
  rxCAN2();
  
}

/* ------------------------------------------------------------ */
/*      CAN Utility Functions                                   */
/* ------------------------------------------------------------ */
/***  initCan1
**
**  Parameters:
**      myaddr    - network address
**
**  Return Value:
**      none
**
**  Errors:
**      none
**
**  Description:
**      Initialize the CAN controller. See inline comments
**      for description of the process.
*/

void
initCan1(uint32_t myaddr) {
  CAN::BIT_CONFIG canBitConfig;

  /* Step 1: Switch the CAN module
   * ON and switch it to Configuration
   * mode. Wait till the switch is 
   * complete */

  canMod1.enableModule(true);

  canMod1.setOperatingMode(CAN::CONFIGURATION);
  
  while(canMod1.getOperatingMode() != CAN::CONFIGURATION);			

  /* Step 2: Configure the CAN Module Clock. The
   * CAN::BIT_CONFIG data structure is used
   * for this purpose. The propagation, 
   * phase segment 1 and phase segment 2
   * are configured to have 3TQ. The CANSetSpeed()
   * function sets the baud. */
	
  canBitConfig.phaseSeg2Tq            = CAN::BIT_3TQ;
  canBitConfig.phaseSeg1Tq            = CAN::BIT_3TQ;
  canBitConfig.propagationSegTq       = CAN::BIT_3TQ;
  canBitConfig.phaseSeg2TimeSelect    = CAN::TRUE;
  canBitConfig.sample3Time            = CAN::TRUE;
  canBitConfig.syncJumpWidth          = CAN::BIT_2TQ;

  canMod1.setSpeed(&canBitConfig,SYS_FREQ,CAN_BUS_SPEED);

  /* Step 3: Assign the buffer area to the
   * CAN module.
   */ 
  /* Note the size of each Channel area.
   * It is 2 (Channels) * 8 (Messages Buffers) 
   * 16 (bytes/per message buffer) bytes. Each 
   * CAN module should have its own message 
   * area. */
   
  canMod1.assignMemoryBuffer(CAN1MessageFifoArea,2 * 8 * 16);	

  /* Step 4: Configure channel 0 for TX and size of
   * 8 message buffers with RTR disabled and low medium
   * priority. Configure channel 1 for RX and size
   * of 8 message buffers and receive the full message.
   */

  canMod1.configureChannelForTx(CAN::CHANNEL0,8,CAN::TX_RTR_DISABLED,CAN::LOW_MEDIUM_PRIORITY);
  canMod1.configureChannelForRx(CAN::CHANNEL1,8,CAN::RX_FULL_RECEIVE);
	
  /* Step 5: Configure filters and mask. Configure
   * filter 0 to accept SID messages with ID 0x200.
   * Configure filter mask 0 to compare all the ID
   * bits and to filter by the ID type specified in
   * the filter configuration. Filter 0 accepted 
   * messages are stored in channel 1.  */

  canMod1.configureFilter      (CAN::FILTER0, myaddr, CAN::SID);    
  canMod1.configureFilterMask  (CAN::FILTER_MASK0, 0xFFF, CAN::SID, CAN::FILTER_MASK_IDE_TYPE);
  canMod1.linkFilterToChannel  (CAN::FILTER0, CAN::FILTER_MASK0, CAN::CHANNEL1); 
  canMod1.enableFilter         (CAN::FILTER0, true);
	
  /* Step 6: Enable interrupt and events. Enable the receive
   * channel not empty event (channel event) and the receive
   * channel event (module event).
   * The interrrupt peripheral library is used to enable
   * the CAN interrupt to the CPU. */

  canMod1.enableChannelEvent(CAN::CHANNEL1, CAN::RX_CHANNEL_NOT_EMPTY, true);
  canMod1.enableModuleEvent(CAN::RX_EVENT, true);

  /* Step 7: Switch the CAN mode
   * to normal mode. */

  canMod1.setOperatingMode(CAN::NORMAL_OPERATION);
  while(canMod1.getOperatingMode() != CAN::NORMAL_OPERATION);			
  
}

/* ------------------------------------------------------------ */
/***  initCan2
**
**  Parameters:
**      myaddr    - network address
**
**  Return Value:
**      none
**
**  Errors:
**      none
**
**  Description:
**      Initialize the CAN controller. See inline comments
**      for description of the process.
*/

void
initCan2(uint32_t myaddr) {
  CAN::BIT_CONFIG canBitConfig;

  /* Step 1: Switch the CAN module
   * ON and switch it to Configuration
   * mode. Wait till the switch is 
   * complete */

  canMod2.enableModule(true);

  canMod2.setOperatingMode(CAN::CONFIGURATION);
  while(canMod2.getOperatingMode() != CAN::CONFIGURATION);			

  /* Step 2: Configure the CAN Module Clock. The
   * CAN::BIT_CONFIG data structure is used
   * for this purpose. The propagation, 
   * phase segment 1 and phase segment 2
   * are configured to have 3TQ. The CANSetSpeed()
   * function sets the baud. */
	
  canBitConfig.phaseSeg2Tq            = CAN::BIT_3TQ;
  canBitConfig.phaseSeg1Tq            = CAN::BIT_3TQ;
  canBitConfig.propagationSegTq       = CAN::BIT_3TQ;
  canBitConfig.phaseSeg2TimeSelect    = CAN::TRUE;
  canBitConfig.sample3Time            = CAN::TRUE;
  canBitConfig.syncJumpWidth          = CAN::BIT_2TQ;

  canMod2.setSpeed(&canBitConfig,SYS_FREQ,CAN_BUS_SPEED);

  /* Step 3: Assign the buffer area to the
   * CAN module.
   */ 
  /* Note the size of each Channel area.
   * It is 2 (Channels) * 8 (Messages Buffers) 
   * 16 (bytes/per message buffer) bytes. Each 
   * CAN module should have its own message 
   * area. */
   
  canMod2.assignMemoryBuffer(CAN2MessageFifoArea,2 * 8 * 16);	

  /* Step 4: Configure channel 0 for TX and size of
   * 8 message buffers with RTR disabled and low medium
   * priority. Configure channel 1 for RX and size
   * of 8 message buffers and receive the full message.
   */

  canMod2.configureChannelForTx(CAN::CHANNEL0,8,CAN::TX_RTR_DISABLED,CAN::LOW_MEDIUM_PRIORITY);
  canMod2.configureChannelForRx(CAN::CHANNEL1,8,CAN::RX_FULL_RECEIVE);
	
  /* Step 5: Configure filters and mask. Configure
   * filter 0 to accept SID messages with ID 0x200.
   * Configure filter mask 0 to compare all the ID
   * bits and to filter by the ID type specified in
   * the filter configuration. Filter 0 accepted 
   * messages are stored in channel 1.  */

  canMod2.configureFilter      (CAN::FILTER0, myaddr, CAN::SID);    
  canMod2.configureFilterMask  (CAN::FILTER_MASK0, 0xFFF, CAN::SID, CAN::FILTER_MASK_IDE_TYPE);
  canMod2.linkFilterToChannel  (CAN::FILTER0, CAN::FILTER_MASK0, CAN::CHANNEL1); 
  canMod2.enableFilter         (CAN::FILTER0, true);
	
  /* Step 6: Enable interrupt and events. Enable the receive
   * channel not empty event (channel event) and the receive
   * channel event (module event).
   * The interrrupt peripheral library is used to enable
   * the CAN interrupt to the CPU. */

  canMod2.enableChannelEvent(CAN::CHANNEL1, CAN::RX_CHANNEL_NOT_EMPTY, true);
  canMod2.enableModuleEvent(CAN::RX_EVENT, true);

  /* Step 7: Switch the CAN mode
   * to normal mode. */

  canMod2.setOperatingMode(CAN::NORMAL_OPERATION);
  while(canMod2.getOperatingMode() != CAN::NORMAL_OPERATION);			

}

/* ------------------------------------------------------------ */
/***  txCAN1
**
**  Parameters:
**      rxnode    - address of network node to receive the packet
**
**  Return Value:
**      none
**
**  Errors:
**      none
**
**  Description:
**      Initialize a packet buffer with the packet header and the
**      packet payload. The payload in this case is a single
**      ASCII character (0x31 = '1'). Transmit the packet.
*/

void
txCAN1(uint32_t rxnode) {
  
  CAN::TxMessageBuffer * message;
  
  message = canMod1.getTxMessageBuffer(CAN::CHANNEL0);

  if (message != NULL) {
    // clear buffer
    message->messageWord[0] = 0;
    message->messageWord[1] = 0;
    message->messageWord[2] = 0;
    message->messageWord[3] = 0;

    message->msgSID.SID   = rxnode;	//receiving node		
    message->msgEID.IDE   = 0;			
    message->msgEID.DLC   = 1;			
    message->data[0]      = 0x31;
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
     
     canMod1.updateChannel(CAN::CHANNEL0);

    /* Direct the CAN module to flush the
     * TX channel. This will send any pending
     * message in the TX channel. */

    canMod1.flushTxChannel(CAN::CHANNEL0);
  }	

}

/* ------------------------------------------------------------ */
/***  txCAN2
**
**  Parameters:
**      rxnode    - address of network node to receive the packet
**
**  Return Value:
**      none
**
**  Errors:
**      none
**
**  Description:
**      Initialize a packet buffer with the packet header and the
**      packet payload. The payload in this case is a single
**      ASCII character (0x32 = '2'). Transmit the packet.
*/

void
txCAN2(uint32_t rxnode) {
  
  CAN::TxMessageBuffer * message;
  
  message = canMod2.getTxMessageBuffer(CAN::CHANNEL0);

  if (message != NULL) {
    // clear buffer
    message->messageWord[0] = 0;
    message->messageWord[1] = 0;
    message->messageWord[2] = 0;
    message->messageWord[3] = 0;

    message->msgSID.SID    = rxnode;	//receiving node	
    message->msgEID.IDE    = 0;			
    message->msgEID.DLC    = 1;			
    message->data[0]       = 0x32;
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

    canMod2.updateChannel(CAN::CHANNEL0);

    /* Direct the CAN module to flush the
     * TX channel. This will send any pending
     * message in the TX channel. */

    canMod2.flushTxChannel(CAN::CHANNEL0);
  }	

}

/* ------------------------------------------------------------ */
/***  rxCAN1
**
**  Parameters:
**        none
**
**  Return Value:
**        none
**
**  Errors:
**        none
**
**  Description:
**      Check to see if a packet has been received. If so, read
**      the packet and print the packet payload to the serial
**      monitor.
*/

void
rxCAN1(void) {
  
  CAN::RxMessageBuffer * message;

  if (isCAN1MsgReceived == false) { 
    /* CAN2 did not receive any message
     * so exit the function. Note that the
     * isCAN2MsgReceived flag is updated 
     * by the CAN2 ISR. */
    return;
  }
	
  /* Message was received. Reset isCAN2MsgReceived flag
   * to catch the next message. */

  isCAN1MsgReceived = false;	
	
  message = canMod1.getRxMessage(CAN::CHANNEL1);

  /* Print the first byte of the packet payload area
   * as an ASCII character on the serial monitor. */
   
  Serial.print(byte(message->data[0]));

  /* Call the CAN::updateChannel() function to let
   * the CAN module know that the message processing
   * is done. Enable the event so that the CAN module
   * generates an interrupt when the event occurs.*/

  canMod1.updateChannel(CAN::CHANNEL1);
  canMod1.enableChannelEvent(CAN::CHANNEL1, CAN::RX_CHANNEL_NOT_EMPTY, true);

}

/* ------------------------------------------------------------ */
/***  rxCAN2
**
**  Parameters:
**        none
**
**  Return Value:
**        none
**
**  Errors:
**        none
**
**  Description:
**      Check to see if a packet has been received. If so, read
**      the packet and print the packet payload to the serial
**      monitor.
*/

void
rxCAN2(void) {
  
  CAN::RxMessageBuffer * message;

  if (isCAN2MsgReceived == false) {
    /* CAN2 did not receive any message
     * so exit the function. Note that the
     * isCAN2MsgReceived flag is updated 
     * by the CAN2 ISR. */
    return;
  }
	
  /* Message was received. Reset isCAN2MsgReceived flag
   * to catch the next message. */

  isCAN2MsgReceived = false;	
	
  message = canMod2.getRxMessage(CAN::CHANNEL1);

  /* Print the first byte of the packet payload area
   * as an ASCII character on the serial monitor. */

  Serial.print(byte(message->data[0]));

  /* Call the CAN::updateChannel() function to let
   * the CAN module know that the message processing
   * is done. Enable the event so that the CAN module
   * generates an interrupt when the event occurs.*/

  canMod2.updateChannel(CAN::CHANNEL1);
  canMod2.enableChannelEvent(CAN::CHANNEL1, CAN::RX_CHANNEL_NOT_EMPTY, true);

}

/* ------------------------------------------------------------ */
/*        Interrupt Handler Functions                           */
/* ------------------------------------------------------------ */
/***  doCan1Interrupt
**
**  Parameters:
**      none
**
**  Return Value:
**      none
**
**  Errors:
**      none
**
**  Description:
**      Interrupt service routine to handle interrupt level
**      events for CAN module 1.
*/

void
doCan1Interrupt() {
  /* This is the CAN1 Interrupt Handler.
   * This is not the actual Interrupt Service Routine,
   * but is the user interrupt handler installed by
   * CAN::attachInterrupt. This is called by the ISR.
   * Note that there are many events in the CAN1 module
   * that can cause this interrupt. These events are 
   * enabled by the CAN::enableModuleEvent() function.
   * In this example, only the CAN::RX_EVENT is enabled. */


  /* Check if the source of the interrupt is CAN::RX_EVENT. 
   * This is redundant  since only this event is enabled
   * in this example but this shows one scheme for handling
   * interrupts. */

  if ((canMod1.getModuleEvent() & CAN::RX_EVENT) != 0) {
		
    /* Within this, you can check which event caused the 
     * interrupt by using the CAN::getPendingEventCode() function
     * to get a code representing the highest priority active
     * event.*/ 
		
    if(canMod1.getPendingEventCode() == CAN::CHANNEL1_EVENT) {
      /* This means that channel 1 caused the event.
       * The CAN::RX_CHANNEL_NOT_EMPTY event is persistent. You
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
       * the CAN::RX_CHANNEL_NOT_EMPTY event is persistent (unless
       * the not empty condition is cleared.) 
       * */
			
      canMod1.enableChannelEvent(CAN::CHANNEL1, CAN::RX_CHANNEL_NOT_EMPTY, false);
      isCAN1MsgReceived = true;	
    }
  }

  /* The CAN1 Interrupt flag is cleared by the interrupt service routine
   * after this function returns. This will succeed because the event
   * that caused this interrupt to occur (CAN::RX_CHANNEL_NOT_EMPTY) is disabled.
   * The ISR's attempt to clear the CAN1 interrupt flag would fail if the
   * CAN::RX_CHANNEL_NOT_EMPTY event were still enabled because the base event
   * is still present. In this case, another interrupt would occur immediately */ 
	
}

/* ------------------------------------------------------------ */
/***  doCan2Interrupt
**
**  Parameters:
**      none
**
**  Return Value:
**      none
**
**  Errors:
**      none
**
**  Description:
**      Interrupt service routine to handle interrupt level
**      events for CAN module 2.
*/

void
doCan2Interrupt() {
  /* This is the CAN2 Interrupt Handler.
   * This is not the actual Interrupt Service Routine,
   * but is the user interrupt handler installd by
   * CAN::attachInterrupt. This is called by the ISR.
   * Note that there are many events in the CAN2 module
   * that can cause this interrupt. These events are 
   * enabled by the CAN::enableModuleEvent() function.
   * In this example, only the CAN::RX_EVENT is enabled. */


  /* Check if the source of the interrupt is CAN::RX_EVENT. 
   * This is redundant  since only this event is enabled
   * in this example but this shows one scheme for handling
   * interrupts. */

  if ((canMod2.getModuleEvent() & CAN::RX_EVENT) != 0) {
		
    /* Within this, you can check which event caused the 
     * interrupt by using the CAN::getPendingEventCode() function
     * to get a code representing the highest priority active
     * event.*/ 
		
    if(canMod2.getPendingEventCode() == CAN::CHANNEL1_EVENT) {
      /* This means that channel 1 caused the event.
       * The CAN::RX_CHANNEL_NOT_EMPTY event is persistent. You
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
       * the CAN::RX_CHANNEL_NOT_EMPTY event is persistent (unless
       * the not empty condition is cleared.) 
       * */
			
      canMod2.enableChannelEvent(CAN::CHANNEL1, CAN::RX_CHANNEL_NOT_EMPTY, false);
      isCAN2MsgReceived = true;	
    }
  }

  /* The CAN2 Interrupt flag is cleared by the interrupt service routine
   * after this function returns. This will succeed because the event
   * that caused this interrupt to occur (CAN::RX_CHANNEL_NOT_EMPTY) is disabled.
   * The ISR's attempt to clear the CAN2 interrupt flag would fail if the
   * CAN::RX_CHANNEL_NOT_EMPTY event were still enabled because the base event
   * is still present. In this case, another interrupt would occur immediately */	
  
}

/* ------------------------------------------------------------ */
/***  ProcName
**
**  Parameters:
**
**  Return Value:
**
**  Errors:
**
**  Description:
**
*/

/* ------------------------------------------------------------ */

/************************************************************************/


