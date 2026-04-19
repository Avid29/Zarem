
.globl entry

.text
    entry:
    
    # Prompt the user for their name
    la      $a0,    name_prompt
    la      $a2,    0
    xori    $v0,    $zero,  3
    syscall
    
    # Read user input (36 char limit)
    la      $a0,    name_str
    li      $a1,    36
    li      $a2,    2
    li      $v0,    4
    syscall
    
    # Say "Hello <name>"
    la      $a0,    hello_str
    li      $a2,    2
    xori    $v0,    $zero, 3
    syscall
    
    # Print newline
    la      $a0,    newline_str
    li      $a2,    0
    li      $v0,    3
    syscall
    
    # Exit gracefully
    xori    $v0,    $zero, 9
    syscall

.data
name_prompt:    .asciiz "What is your name? (36 chars)\n"
hello_str:      .utf16 "Hello "
name_str:       .space 36
newline_str:    .asciiz "\n"