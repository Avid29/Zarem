# Define globals declarations
.global entry

.data
board:  .space 36           # The 6x6 board as bytes

.text
entry:
    # Call read logic
    jal     read_board
    
    # Solve the board
    li      a0,     0
    jal     solve_sudoku
    
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
    li      a2,     0
    li      a7,     3
    ecall
    
    # Read input
    la      a0,     buffer
    li      a1,     37
    li      a2,     0
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
    li      t5,     6                   # col limit

print_loop:
    beq     t1,     t2,     print_done
    
    # Print the current digit
    lbu     a0,     0(t0)
    li      a7,     1
    ecall
    
    # Print a space for visibility
    la      a0,     space
    li      a2,     0
    li      a7,     3
    ecall

    addi    t1,     t1,     1           # Increment total
    addi    t0,     t0,     1           # Increment pointer

    # Check if we hit the end of the row (index % 6 == 0)
    # Use the index after increment to simply this check
    rem     t6,     t1,     t5
    bne     t6,     zero,     print_loop   
    
    # If we are here, t6 == 6. Print a newline
    la      a0,     newline
    li      a1,     0
    li      a7,     3
    ecall
    
    j       print_loop

print_done:
    ret
    
solve_sudoku:
    addi    sp, sp, -12
    sw      ra, 8(sp)
    sw      s0, 4(sp)
    sw      s1, 0(sp)
    
    # Save current index in s0
    move    s0, a0            

    # Base Case: If index == 36, we are done
    li      t0, 36
    beq     s0, t0, solved_true

    # Check if current cell is already filled
    la      t1, board
    add     t1, t1, s0
    lbu     t2, 0(t1)
    bne     t2, zero, skip_to_next

    # Try values 1 to 6
    li      s1, 1             # Use s1 to track our digit (requires saving s1!)
    addi    sp, sp, -4
    sw      s1, 0(sp)         # Pushing s1 to stack to demonstrate building

try_digit:
    move    a0, s0            # Param: index
    lw      a1, 0(sp)         # Param: digit
    jal     is_valid
    beq     a0, zero, backtrack

    # If valid, place it
    la      t1, board
    add     t1, t1, s0
    lw      t2, 0(sp)
    sb      t2, 0(t1)

    # Recurse
    addi    a0, s0, 1
    jal     solve_sudoku
    bne     a0, zero, solved_cleanup

    # If recursion failed, reset
    la      t1, board
    add     t1, t1, s0
    sb      zero, 0(t1)

backtrack:
    lw      t0, 0(sp)
    addi    t0, t0, 1
    sw      t0, 0(sp)         # Increment digit on stack
    li      t1, 7
    blt     t0, t1, try_digit

    # Failed all 1-6
    addi    sp, sp, 4         # Pop s1
    li      a0, 0
    j       solver_ret

skip_to_next:
    addi    a0, s0, 1
    jal     solve_sudoku
    j       solver_ret

solved_true:
    li      a0, 1
    j       solver_ret

solved_cleanup:
    addi    sp, sp, 4         # Pop s1
    li      a0, 1

solver_ret:
    lw      ra, 8(sp)
    lw      s0, 4(sp)
    lw      s1, 0(sp)
    addi    sp, sp, 12
    ret
    
is_valid:
    addi    sp, sp, -8        # Smallest possible frame for ra/s0
    sw      ra, 4(sp)
    sw      s0, 0(sp)         # Use s0 to keep 'index' safe across calls
    
    move    s0, a0            # Save index in s0
    
    # a1 (value) is used immediately, so we don't need to save it 
    # UNLESS check_row/col/box overwrites it (which they shouldn't if they follow ABI).

    jal     check_row
    beq     a0, zero, is_valid_end  # Short-circuit failure
    
    move    a0, s0            # Restore index for next call
    jal     check_col
    beq     a0, zero, is_valid_end
    
    move    a0, s0
    jal     check_box

is_valid_end:
    lw      ra, 4(sp)
    lw      s0, 0(sp)
    addi    sp, sp, 8
    ret
    
check_row:
    # a0 = index, a1 = value
    li      t0, 6
    div     t1, a0, t0        # t1 = row index (0-5)
    mul     t1, t1, t0        # t1 = start of row (row * 6)
    
    la      t2, board
    add     t2, t2, t1        # t2 = address of board[row][0]
    
    li      t3, 0             # loop counter
row_loop:
    lbu     t4, 0(t2)
    beq     t4, a1, fail      # Found duplicate!
    addi    t2, t2, 1
    addi    t3, t3, 1
    blt     t3, t0, row_loop
    li      a0, 1
    ret
    
check_col:
    # a0 = index, a1 = value
    li      t0, 6
    rem     t1, a0, t0        # t1 = column index (0-5)
    
    la      t2, board
    add     t2, t2, t1        # t2 = address of board[0][col]
    
    li      t3, 0             # counter
col_loop:
    lbu     t4, 0(t2)
    beq     t4, a1, fail
    addi    t2, t2, 6         # Move down one row (skip 6 bytes)
    addi    t3, t3, 1
    blt     t3, t0, col_loop
    li      a0, 1
    ret
    
check_box:
    # a0 = index, a1 = value
    li      t0, 6
    div     t1, a0, t0        # row
    rem     t2, a0, t0        # col
    
    # row_start = (row / 2) * 2
    li      t3, 2
    div     t1, t1, t3
    mul     t1, t1, t3
    
    # col_start = (col / 3) * 3
    li      t3, 3
    div     t2, t2, t3
    mul     t2, t2, t3
    
    # Iterate through 2 rows, 3 columns
    li      t3, 0             # i = 0 to 1
outer_box:
    li      t4, 0             # j = 0 to 2
inner_box:
    # index = (row_start + i) * 6 + (col_start + j)
    add     t5, t1, t3        # current_row
    mul     t5, t5, t0        # * 6
    add     t6, t2, t4        # current_col
    add     t5, t5, t6        # final index
    
    la      a2, board
    add     a2, a2, t5
    lbu     a3, 0(a2)
    beq     a3, a1, fail
    
    addi    t4, t4, 1
    li      a4, 3
    blt     t4, a4, inner_box
    addi    t3, t3, 1
    li      a4, 2
    blt     t3, a4, outer_box
    
    li      a0, 1
    ret

fail:
    li      a0, 0
    ret
