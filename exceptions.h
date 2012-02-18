#include <setjmp.h>

#include "WProgram.h"
#include "wiring_private.h"

#define TRY if(!(_excep_code = setjmp(_excep_buf)))
#define CATCH else 
#define EXCEPTION_NUM_EXCEPTIONS 14

// declared static in case exception condition would prevent
// auto variable being created
static enum {
   EXCEP_IRQ = 0,         // interrupt
   EXCEP_AdEL = 4,         // address error exception (load or ifetch)
   EXCEP_AdES,            // address error exception (store)
   EXCEP_IBE,            // bus error (ifetch)
   EXCEP_DBE,            // bus error (load/store)
   EXCEP_Sys,            // syscall
   EXCEP_Bp,            // breakpoint
   EXCEP_RI,            // reserved instruction
   EXCEP_CpU,            // coprocessor unusable
   EXCEP_Overflow,         // arithmetic overflow
   EXCEP_Trap,            // trap (possible divide by zero)
   EXCEP_IS1 = 16,         // implementation specfic 1
   EXCEP_CEU,            // CorExtend Unuseable
   EXCEP_C2E            // coprocessor 2
} _excep_codes;
static jmp_buf _excep_buf;
static unsigned int _excep_code; // exception code corresponds to _excep_codes
static unsigned int _excep_addr; // exception address
static unsigned int _excep_stat; // status register


#ifdef __cplusplus
extern "C" {
#endif
void _general_exception_handler(unsigned cause, unsigned status) {
  _excep_code = (cause & 0x0000007C) >> 2;
  _excep_stat = status;
  _excep_addr = __builtin_mfc0(_CP0_EPC, _CP0_EPC_SELECT);
  if ((cause & 0x80000000) != 0)
    _excep_addr += 4;
  longjmp(_excep_buf, _excep_code);
}
#ifdef __cplusplus
}
#endif
