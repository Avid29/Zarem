.data
prompt_num:
    .asciiz "Enter a number: "
prompt_op:
    .asciiz "Enter Operator (+, -, *, / or 'q' to quit): "
result_msg:
    .asciiz "Result: "
error_msg:
    .asciiz "Error: Invalid Operator"
exit_msg:
    .asciiz "Exiting calculator. Goodbye!\n"
newline:
    .asciiz "\n"
    
op_buffer:
    .space 2
    
.text
.globl entry

entry:
loop:
    # ---- Get First Number ----
    la      $a0, prompt_num
    jal     read_float
    nop
    mov.s   $f20, $f0      # Save to $f20 (saved register)
    
    # ---- Get Operator ----
    la $a0, prompt_op
    jal read_char
    nop
    move $s0, $v0        # Save operator to $s0
    
    # Check for quit condition
    li  $t0, 'q'
    beq $s0, $t0, exit_prog
    
    # ---- Get Second Number ----
    la $a0, prompt_num
    jal read_float
    nop
    mov.s $f21, $f0      # Save to $f21
    
    # ---- Perform Calculation ----
    move $a0, $s0        # Pass operator
    # Arguments for floats are passed in $f12, $f14
    mov.s $f12, $f20
    mov.s $f14, $f21
    jal calculate        # Result comes back in $f0
    nop
    
    # ---- Print Result ----
    mov.s $f12, $f0
    jal print_result
    nop

    j loop               # Repeat indefinitely
    nop
    
exit_prog:
    li $v0, 4
    la $a0, exit_msg
    syscall
    li $v0, 10
    syscall
    
# ---------------------------------------------------------
# Subroutine: read_float
# $a0 = address of prompt
# Returns: $f0 = float value
# ---------------------------------------------------------
read_float:
    li $v0, 4
    syscall
    li $v0, 6
    syscall
    jr $ra
    nop
    
# ---------------------------------------------------------
# Subroutine: read_char
# $a0 = address of prompt
# Returns: $v0 = character
# ---------------------------------------------------------
read_char:
    li $v0, 4
    syscall
    li $v0, 8
    la $a0, op_buffer
    li $a1, 2
    syscall
    lb $v0, 0($a0)
    jr $ra
    nop
    
# ---------------------------------------------------------
# Subroutine: calculate
# $a0 = operator, $f12 = num1, $f14 = num2
# Returns: $f0 = result
# ---------------------------------------------------------
calculate:
    li      $t0,    '+'
    beq     $a0,    $t0,    add_op
    li      $t0,    '-' 
    beq     $a0,    $t0,    sub_op
    li      $t0,    '*' 
    beq     $a0,    $t0,    mul_op
    li      $t0,    '/' 
    beq     $a0,    $t0,    div_op
    nop
    mtc1    $zero,  $f0     # Return 0 if invalid
    jr      $ra
    
    
add_op:
    add.s   $f0, $f12, $f14
    jr      $ra
    nop
sub_op:
    sub.s   $f0, $f12, $f14
    jr      $ra
    nop
mul_op:
    mul.s   $f0, $f12, $f14
    jr      $ra
    nop
div_op:
    div.s   $f0, $f12, $f14
    jr      $ra
    nop
    
# ---------------------------------------------------------
# Subroutine: print_result
# $f12 = value to print
# ---------------------------------------------------------
print_result:
    # Save $ra if we were calling other functions, but we aren't.
    li $v0, 4
    la $a0, result_msg
    syscall
    
    li $v0, 2            # Print float in $f12
    syscall
    
    li $v0, 4
    la $a0, newline
    syscall
    jr $ra
    nop