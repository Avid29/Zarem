# Avishai Dernis 2026

# The classic fizz buzz

.globl entry

.text
entry:

    # Begin loop at zero
    addi    $s0,    $zero, 1

loop:
    
    # Check Fizz
    xori    $t1,    $zero,  3
    div     $s0,    $t1
    mfhi    $t6             # Cache x % 3 in $t6
    
    # Branch past fizz if x % 3 != 0
    bne     $t6,    $zero,  skip_fizz
    nop
    
    # Print fizz
    la      $a0,    fizz_str
    xori    $v0,    $zero,  4
    syscall
    
skip_fizz:

    # Check Buzz
    xori    $t1,    $zero,  5
    div     $s0,    $t1
    mfhi    $t7             # Cache x % 5 in $t7
    
    # Branch past fizz if x % 5 != 0
    bne     $t7,    $zero,  skip_buzz
    nop
    
    # Print buzz
    la      $a0,    buzz_str
    xori    $v0,    $zero,  4
    syscall
    
skip_buzz:

    # Branch past if either fizz or buzz
    beq     $t6,    $zero,  newline
    nop
    beq     $t7,    $zero,  newline
    nop
    
    # Neither Fizz nor Buzz
    # Print the number
    xor     $a0,    $zero,  $s0
    xori    $v0,    $zero,  1
    syscall
    
newline:

    # Explicitly print new line if either fizz or buzz
    la      $a0,    newline_str
    xori    $v0,    $zero,  4
    syscall
    
loop_check:
    
    # Loop again if $s0 <= 100
    # Increment in the delay slot
    slti    $t0,    $s0,    100
    bgtz    $t0,    loop
    addi    $s0,    $s0,    1
    
loop_end:

    # Shutdown
    xori    $v0,    $zero,  10
    syscall
    
    
.data

    # Define resource strings
    
fizz_str:
	.asciiz "Fizz"
buzz_str:
	.asciiz "Buzz"
newline_str:
    .asciiz "\n"