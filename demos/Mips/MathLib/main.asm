.globl entry         
.globl square_func   # Reference external symbol

.text
entry:
    
    # Read int
    li      $v0,    2
    syscall

    # Take input squred
    move    $a0,    $v0
    jal     square_func
    nop
    
    # Print the input squared
    move    $a0,    $v0
    li      $v0,    1
    syscall
    
    # Exit gracefully
    li      $v0,    9
    syscall