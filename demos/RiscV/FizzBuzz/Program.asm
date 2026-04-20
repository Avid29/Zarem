# Avishai Dernis 2026

# The classic fizz buzz

.def SYS_PRINT_INT 1
.def SYS_PRINT_STR 3

.globl entry

.text
entry:

    # Begin loop at one
    addi    s0,    zero,    1

loop:
    
    # Check Fizz
    xori    t1,     zero,   3
    rem     t5,     s0,     t1
    
    # Branch past fizz if x % 3 != 0
    bne     t5,     zero,    skip_fizz
    
    # Print fizz
    la      a0,     fizz_str
    xori    a2,     zero,   0
    xori    a7,     zero,   SYS_PRINT_STR
    ecall
    
skip_fizz:

    # Check Buzz
    xori    t1,     zero,   5
    rem     t6,     s0,     t1
    
    # Branch past fizz if x % 5 != 0
    bne     t6,     zero,    skip_buzz
    
    # Print buzz
    la      a0,       buzz_str
    xori    a2,       zero,     0
    xori    a7,       zero,     SYS_PRINT_STR
    ecall
    
skip_buzz:

    # Branch past if either fizz or buzz
    beq     t5,     zero,    newline
    beq     t6,     zero,    newline
    
    # Neither Fizz nor Buzz
    # Print the number
    move    a0,     s0
    xori    a7,     zero,    SYS_PRINT_INT
    ecall
    
newline:

    # Explicitly print new line if either fizz or buzz
    la      a0,     newline_str
    xori    a2,     zero,   0
    xori    a7,     zero,   SYS_PRINT_STR
    ecall
    
loop_check:
    
    # Increment and loop again if $s0 < 101
    slti    t0,     s0,      100
    addi    s0,     s0,      1
    bne     t0,     zero,    loop
    
loop_end:

    # Shutdown
    xori    a7,    zero,    9
    ecall
    
    
.data

    # Define resource strings
fizz_str: .asciiz "Fizz"
buzz_str: .asciiz "Buzz"
newline_str: .asciiz "\n"