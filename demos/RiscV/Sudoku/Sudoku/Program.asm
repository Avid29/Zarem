# Define globals declarations
.global entry

.data
board:  .space 36           # The 6x6 board as bytes

.text
entry:
    # Call read logic
    jal     read_board
    
    # TODO: Solve the board
    
    # Print the result
    jal     print_board
    
    # Exit gracefully
    li      a7,     9
    ecall
    
.data
prompt:     .asciiz "Enter 6x6 board (use 0 for empty):\n"
space:      .asciiz " " 
newline:    .asciiz "\n" 
buffer:     .space  37   # Buffer for string input

.text
read_board:
    # Print the prompt
    la      a0,     prompt
    li      a1,     0
    li      a7,     3
    ecall
    
    # Read input
    la      a0,     buffer
    li      a1,     37
    li      a7,     4
    ecall
    
    # Setup loop
    la      t0,     buffer              # Source
    la      t1,     board               # Destination
    li      t2,     0                   # Count = 0
    li      t4,     36                  # Max = 36

convert_loop:
    beq     t2,     t4,     read_done   # Stop once we have 36 bytes
    lbu     t3,     0(t0)               # Load ASCII char
    
    # Simple ASCII to Int: '0' is 48, '6' is 54
    # Note: This logic assumes the user provides digits.
    # You could add a check here to ensure t3 is between 48-54.
    addi    t3,     t3,     -48         
    sb      t3,     0(t1)             # Store raw byte (0-6)

    addi    t0,     t0,     1
    addi    t1,     t1,     1
    addi    t2,     t2,     1
    j       convert_loop
    
read_done:
    ret
    
# Prints the board from 'board' back to console
print_board:
    la      t0,     board
    li      t1,     0                   # index
    li      t2,     36                  # limit

print_loop:
    beq     t1,     t2,     print_done
    
    # Print the current digit
    lbu     a0,     0(t0)
    li      a7,     1
    ecall
    
    # Print a space for visibility
    la      a0,     space
    li      a1,     0
    li      a7,     1
    ecall

    addi    t1,     t1,     1           # Increment total
    addi    t0,     t0,     1           # Increment pointer
    addi    t5,     t5,     1           # Increment column counter

    # Check if we hit the end of the row (column counter == 6)
    blt     t5,     t6,     print_loop   
    
    # If we are here, t5 == 6. Print newline and reset t5.
    la      a0,     newline
    li      a1,     0
    li      a7,     3
    ecall
    
    li      t5,     0                   # Reset column counter
    j       print_loop

print_done:
    ret