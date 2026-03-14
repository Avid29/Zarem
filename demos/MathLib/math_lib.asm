.text
.globl square_func    # Export symbol

square_func:
    mult $a0, $a0     # Multiply arg by itself
    mflo $v0          # Move result to return register
    jr $ra
    nop