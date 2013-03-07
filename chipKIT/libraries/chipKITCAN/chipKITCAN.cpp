/************************************************************************/
/*                                                                      */
/*  chipKITCan.cpp	--	CAN Library for Digilent chipKIT Network Shield */
/*                                                                      */
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
/*  Module Description: 												*/
/*                                                                      */
/*  This module is the CAN library for use with the Digilent chipKIT    */
/*  Network Shield. This library is a wrapper for the Microchip CAN     */
/*  functions in the Microchip C32 Peripheral Library that is available */
/*  as part of the Microchip C++ Compiler runtime support in the MPIDE  */
/*  system.                                                             */
/*                                                                      */
/************************************************************************/
/*  Revision History:													*/
/*                                                                      */
/*  08/20/2011(GeneApperson): Created                                   */
/*                                                                      */
/************************************************************************/


/* ------------------------------------------------------------ */
/*              Include File Definitions                        */
/* ------------------------------------------------------------ */

#include    <p32xxxx.h>
#include    <plib.h>
#include    <stdint.h>

#include    "chipKITCan.h"

/* ------------------------------------------------------------ */
/*              Local Type and Constant Definitions             */
/* ------------------------------------------------------------ */

typedef void (*voidFuncPtr)(void);
/* ------------------------------------------------------------ */
/*              Global Variables                                */
/* ------------------------------------------------------------ */


/* ------------------------------------------------------------ */
/*              Local Variables                                 */
/* ------------------------------------------------------------ */

volatile static voidFuncPtr pfnCan1Hook = 0;
volatile static voidFuncPtr pfnCan2Hook = 0;

/* ------------------------------------------------------------ */
/*              Forward Declarations                            */
/* ------------------------------------------------------------ */


/* ------------------------------------------------------------ */
/*              Procedure Definitions                           */
/* ------------------------------------------------------------ */
/***	Can1InterruptHandler
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/
extern "C" {

void __ISR(_CAN_1_VECTOR, ipl4) Can1InterruptHandler(void) {

    if (pfnCan1Hook != 0) {
        (*pfnCan1Hook)();
    }    

    IFS1bits.CAN1IF = 0;
}

}

/* ------------------------------------------------------------ */
/***	Can2InterruptHandler
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

extern "C" {

void __ISR(_CAN_2_VECTOR, ipl4) Can2InterruptHandler(void) {

    if (pfnCan2Hook != 0) {
        (*pfnCan2Hook)();
    }    

    IFS1bits.CAN2IF = 0;
}

}

/* ------------------------------------------------------------ */
/*              CAN Object Class Implementation                 */
/* ------------------------------------------------------------ */
/***	CAN::CAN
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

CAN::CAN(MODULE modNew) {

    mod = modNew;

}

/* ------------------------------------------------------------ */
/***	CAN::~CAN
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

CAN::~CAN() {

    mod = MOD_NIL;
}

/* ------------------------------------------------------------ */
/*                  Interrupt Functions                         */
/* ------------------------------------------------------------ */
/***	CAN::attachInterrupt
**
**	Parameters:
**      pfn     - pointer to the user interrupt handler function.
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

void
CAN::attachInterrupt(void (*pfn)()) {

    if (mod == CAN1) {
        /* Store the vector to the user's interrupt handler
        */
        pfnCan1Hook = pfn;

        /* Set up the interrupt and enable CAN1 interrupts
        */
        INTSetVectorPriority(INT_CAN_1_VECTOR, INT_PRIORITY_LEVEL_4);
        INTSetVectorSubPriority(INT_CAN_1_VECTOR, INT_SUB_PRIORITY_LEVEL_0);
        INTEnable(INT_CAN1, INT_ENABLED);
    }
    else if (mod == CAN2) {
        /* Store the vector to the user's interrupt handler
        */
        pfnCan2Hook = pfn;

        /* Set up the interrupt and enable CAN2 interrupts
        */
        INTSetVectorPriority(INT_CAN_2_VECTOR, INT_PRIORITY_LEVEL_4);
        INTSetVectorSubPriority(INT_CAN_2_VECTOR, INT_SUB_PRIORITY_LEVEL_0);
        INTEnable(INT_CAN2, INT_ENABLED);
    }

}

/* ------------------------------------------------------------ */
/***	CAN::detachInterrupt
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

void
CAN::detachInterrupt() {

    if (mod == CAN1) {
        /* Remove the user's interrupt handler.
        */
        pfnCan1Hook = 0;

        /* Turn CAN1 interrupts off
        */
        INTEnable(INT_CAN1, INT_DISABLED);
    }
    else if (mod == CAN2) {
        /* Remove the user's interrupt handler.
        */
        pfnCan2Hook = 0;

        /* Turn CAN2 interrupts off
        */
        INTEnable(INT_CAN2, INT_DISABLED);
    }

}

/* ------------------------------------------------------------ */
/*                  Configuration Functions                     */
/* ------------------------------------------------------------ */
/***	CAN::assignMemoryBuffer
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

void
CAN::assignMemoryBuffer(void * buf, uint32_t size) {

    CANAssignMemoryBuffer((CAN_MODULE)mod, buf, size);

}

/* ------------------------------------------------------------ */
/***	CAN::setOperatingMode
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

void
CAN::setOperatingMode(OP_MODE opm) {

    CANSetOperatingMode((CAN_MODULE)mod, (CAN_OP_MODE)opm);

}

/* ------------------------------------------------------------ */
/***	CAN::getOperatingMode
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

CAN::OP_MODE
CAN::getOperatingMode() {

    return (CAN::OP_MODE)CANGetOperatingMode((CAN_MODULE)mod);

}

/* ------------------------------------------------------------ */
/***	CAN::enableFeature
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

void
CAN::enableFeature(MODULE_FEATURES features, bool enable) {

    CANEnableFeature((CAN_MODULE)mod, (CAN_MODULE_FEATURES)features, (::BOOL)enable);

}

/* ------------------------------------------------------------ */
/***	CAN::deviceNetFilter
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

void
CAN::deviceNetFilter(DNET_FILTER_SIZE dncnt) {

    CANDeviceNetFilter((CAN_MODULE)mod, (CAN_DNET_FILTER_SIZE)dncnt);

}

/* ------------------------------------------------------------ */
/***	CAN::setTimeStampValue
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

void
CAN::setTimeStampValue(uint32_t val) {

    CANSetTimeStampValue((CAN_MODULE)mod, val);

}

/* ------------------------------------------------------------ */
/***	CAN::getTimeStampValue
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

uint32_t
CAN::getTimeStampValue() {

    return CANGetTimeStampValue((CAN_MODULE)mod);

}

/* ------------------------------------------------------------ */
/***	CAN::setTimeStampPrescaler
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

void
CAN::setTimeStampPrescalar(uint32_t pre) {

    CANSetTimeStampPrescalar((CAN_MODULE)mod, pre);

}

/* ------------------------------------------------------------ */
/***	CAN::enableModule
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

void
CAN::enableModule(bool enable) {

    CANEnableModule((CAN_MODULE)mod, (::BOOL)enable);

}

/* ------------------------------------------------------------ */
/***	CAN::setSpeed
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

void
CAN::setSpeed(const BIT_CONFIG * cfg, uint32_t clk, uint32_t spd) {

    CANSetSpeed((CAN_MODULE)mod, (const CAN_BIT_CONFIG *)cfg, clk, spd);

}

/* ------------------------------------------------------------ */
/***	CAN::isActive
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

bool
CAN::isActive() {

    return (bool)CANIsActive((CAN_MODULE)mod);

}

/* ------------------------------------------------------------ */
/***	CAN::resetChannel
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

void
CAN::resetChannel(CHANNEL chn) {

    CANResetChannel((CAN_MODULE)mod, (CAN_CHANNEL)chn);

}

/* ------------------------------------------------------------ */
/***	CAN::isChannelReset
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

bool
CAN::isChannelReset(CHANNEL chn) {

    return CANIsChannelReset((CAN_MODULE)mod, (CAN_CHANNEL)chn);

}

/* ------------------------------------------------------------ */
/***	CAN::updateChannel
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

void
CAN::updateChannel(CHANNEL chn) {

    CANUpdateChannel((CAN_MODULE)mod, (CAN_CHANNEL)chn);

}

/* ------------------------------------------------------------ */
/*                  Event Management Functions                  */
/* ------------------------------------------------------------ */
/***	CAN::enableModuleEvent
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

void
CAN::enableModuleEvent(MODULE_EVENT evt, bool enable) {

    CANEnableModuleEvent((CAN_MODULE)mod, (CAN_MODULE_EVENT)evt, (::BOOL)enable);

}

/* ------------------------------------------------------------ */
/***	CAN::getModuleEvent
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

CAN::MODULE_EVENT
CAN::getModuleEvent() {

    return (CAN::MODULE_EVENT)CANGetModuleEvent((CAN_MODULE)mod);

}

/* ------------------------------------------------------------ */
/***	CAN::clearModuleEvent
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

void
CAN::clearModuleEvent(MODULE_EVENT evt) {

    CANClearModuleEvent((CAN_MODULE)mod, (CAN_MODULE_EVENT)evt);

}

/* ------------------------------------------------------------ */
/***	CAN::enableChannelEvent
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

void
CAN::enableChannelEvent(CHANNEL chn, CHANNEL_EVENT evt, bool enable) {

    CANEnableChannelEvent((CAN_MODULE)mod, (CAN_CHANNEL)chn,
                                (CAN_CHANNEL_EVENT)evt, (::BOOL)enable);

}

/* ------------------------------------------------------------ */
/***	CAN::getPendingEventCode
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

CAN::EVENT_CODE
CAN::getPendingEventCode() {

    return (CAN::EVENT_CODE)CANGetPendingEventCode((CAN_MODULE)mod);

}

/* ------------------------------------------------------------ */
/***	CAN::getAllChannelEventStatus
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

CAN::CHANNEL_MASK
CAN::getAllChannelEventStatus() {

    return (CAN::CHANNEL_MASK)CANGetAllChannelEventStatus((CAN_MODULE)mod);

}

/* ------------------------------------------------------------ */
/***	CAN::getAllChannelOverflowStatus
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

CAN::CHANNEL_MASK
CAN::getAllChannelOverflowStatus() {

    return (CAN::CHANNEL_MASK)CANGetAllChannelOverflowStatus((CAN_MODULE)mod);

}

/* ------------------------------------------------------------ */
/***	CAN::getChannelEvent
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

CAN::CHANNEL_EVENT
CAN::getChannelEvent(CHANNEL chn) {

    return (CAN::CHANNEL_EVENT)CANGetChannelEvent((CAN_MODULE)mod, (CAN_CHANNEL)chn);

}

/* ------------------------------------------------------------ */
/***	CAN::clearChannelEvent
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

void
CAN::clearChannelEvent(CHANNEL chn, CHANNEL_EVENT evt) {

    CANClearChannelEvent((CAN_MODULE)mod, (CAN_CHANNEL)chn, (CAN_CHANNEL_EVENT)evt);

}

/* ------------------------------------------------------------ */
/*                  Message Transmit Functions                  */
/* ------------------------------------------------------------ */
/***	CAN::configureChannelForTx
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

void
CAN::configureChannelForTx(CHANNEL chn, uint32_t size,
                        TX_RTR rtren, TXCHANNEL_PRIORITY pr) {

    CANConfigureChannelForTx((CAN_MODULE)mod, (CAN_CHANNEL)chn, size,
                                (CAN_TX_RTR)rtren, (CAN_TXCHANNEL_PRIORITY)pr);

}

/* ------------------------------------------------------------ */
/***	CAN::abortPendingTx
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

void
CAN::abortPendingTx(CHANNEL chn) {

    CANAbortPendingTx((CAN_MODULE)mod, (CAN_CHANNEL)chn);

}

/* ------------------------------------------------------------ */
/***	CAN::flushChannel
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

void
CAN::flushTxChannel(CHANNEL chn) {

    CANFlushTxChannel((CAN_MODULE)mod, (CAN_CHANNEL)chn);

}

/* ------------------------------------------------------------ */
/***	CAN::getTxChannelCondition
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

CAN::TX_CHANNEL_CONDITION
CAN::getTxChannelCondition(CHANNEL chn) {

    return (CAN::TX_CHANNEL_CONDITION)CANGetTxChannelCondition((CAN_MODULE)mod, (CAN_CHANNEL)chn);

}

/* ------------------------------------------------------------ */
/***	CAN::getMessageBuffer
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

CAN::TxMessageBuffer *
CAN::getTxMessageBuffer(CHANNEL chn) {
    
    return (CAN::TxMessageBuffer *)CANGetTxMessageBuffer((CAN_MODULE)mod, (CAN_CHANNEL)chn);

}

/* ------------------------------------------------------------ */
/***	CAN::isTxAborted
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

bool
CAN::isTxAborted(CHANNEL chn) {

    return (bool)CANIsTxAborted((CAN_MODULE)mod, (CAN_CHANNEL)chn);

}

/* ------------------------------------------------------------ */
/*                  Message Receive Functions                   */
/* ------------------------------------------------------------ */
/***	CAN::configureChannelForRx
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

void
CAN::configureChannelForRx(CHANNEL chn, uint32_t size, RX_DATA_MODE dataOnly) {

    CANConfigureChannelForRx((CAN_MODULE)mod, (CAN_CHANNEL)chn, size, (CAN_RX_DATA_MODE)dataOnly);

}

/* ------------------------------------------------------------ */
/***	CAN::getRxMessage
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

CAN::RxMessageBuffer *
CAN::getRxMessage(CHANNEL chn) {

    return (CAN::RxMessageBuffer *)CANGetRxMessage((CAN_MODULE)mod, (CAN_CHANNEL)chn);

}

/* ------------------------------------------------------------ */
/*              Message Filter Functions                        */
/* ------------------------------------------------------------ */
/***	CAN::configureFilterMask
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

void
CAN::configureFilterMask(FILTER_MASK mask, uint32_t maskbits, ID_TYPE id,
                                FILTER_MASK_TYPE mide) {

    CANConfigureFilterMask((CAN_MODULE)mod, (CAN_FILTER_MASK)mask, maskbits,
                                (CAN_ID_TYPE)id, (CAN_FILTER_MASK_TYPE)mide);

}

/* ------------------------------------------------------------ */
/***	CAN::configureFilter
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

void
CAN::configureFilter(FILTER filter, uint32_t id, ID_TYPE type) {

    CANConfigureFilter((CAN_MODULE)mod, (CAN_FILTER)filter, id, (CAN_ID_TYPE)type);

}

/* ------------------------------------------------------------ */
/***	CAN::enableFilter
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

void
CAN::enableFilter(FILTER filter, bool enable) {

    CANEnableFilter((CAN_MODULE)mod, (CAN_FILTER)filter, (::BOOL)enable);

}

/* ------------------------------------------------------------ */
/***	CAN::getLatestFilterHit
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

CAN::FILTER
CAN::getLatestFilterHit() {

    return (CAN::FILTER)CANGetLatestFilterHit((CAN_MODULE)mod);

}

/* ------------------------------------------------------------ */
/***	CAN::linkFilterToChannel
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

void
CAN::linkFilterToChannel(FILTER filter, FILTER_MASK mask, CHANNEL chn) {

    CANLinkFilterToChannel((CAN_MODULE)mod, (CAN_FILTER)filter,
                                (CAN_FILTER_MASK)mask, (CAN_CHANNEL)chn);

}

/* ------------------------------------------------------------ */
/***	CAN::isFilterDisabled
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

bool
CAN::isFilterDisabled(FILTER filter) {

    return (bool)CANIsFilterDisabled((CAN_MODULE)mod, (CAN_FILTER)filter);

}

/* ------------------------------------------------------------ */
/*                  Error State Tracking                        */
/* ------------------------------------------------------------ */
/***	CAN::getRxErrorCount
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

uint32_t
CAN::getRxErrorCount() {

    return CANGetRxErrorCount((CAN_MODULE)mod);

}

/* ------------------------------------------------------------ */
/***	CAN::getTxErrorCount
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

uint32_t
CAN::getTxErrorCount() {

    return CANGetTxErrorCount((CAN_MODULE)mod);

}

/* ------------------------------------------------------------ */
/***	CAN::getErrorState
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

CAN::ERROR_STATE
CAN::getErrorState() {

    return (CAN::ERROR_STATE)CANGetErrorState((CAN_MODULE)mod);

}

/* ------------------------------------------------------------ */
/*                  Information Functions                       */
/* ------------------------------------------------------------ */
/***	CAN::totalModules
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

uint32_t
CAN::totalModules() {

        return CANTotalModules();

}

/* ------------------------------------------------------------ */
/***	CAN::totalChannels
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

uint32_t
CAN::totalChannels() {

    return CANTotalChannels();

}

/* ------------------------------------------------------------ */
/***	CAN::totalFilters
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

uint32_t
CAN::totalFilters() {

    return CANTotalFilters();

}

/* ------------------------------------------------------------ */
/***	CAN::totalMasks
**
**	Parameters:
**
**	Return Value:
**
**	Errors:
**
**	Description:
**
*/

uint32_t
CAN::totalMasks() {

    return CANTotalMasks();

}

/* ------------------------------------------------------------ */

/************************************************************************/

