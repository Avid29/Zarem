.text
.globl entry         # Declare the entry point
.globl square_func   # Reference external symbol

entry:
    
    # Read int
    li      $v0,    5
    syscall

    # Take input squred
    move    $a0,    $v0
    jal     square_func
    
    # Print the input squared
    move    $a0,    $v0
    li      $v0,    1
    syscall
    
    # Exit gracefully
    li      $v0,    10
    syscall