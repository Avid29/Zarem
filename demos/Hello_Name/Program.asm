.text
    entry:
    
    # Prompt the user for their name
    xori    $a0,    $zero,  name_prompt
    xori    $v0,    $zero,  4
    syscall
    
    # Read user input (36 char limit)
    xori    $a0,    $zero, name_str
    xori    $a1,    $zero, 36
    xori    $v0,    $zero, 8
    syscall
    
    # Say "Hello <name>"
    xori    $a0,    $zero, hello_str
    xori    $v0,    $zero, 4
    syscall
    
    # Print newline
    xori    $a0,    $zero, newline_str
    xori    $v0,    $zero, 4
    syscall
    
    # Exit gracefully
    xori    $v0,    $zero, 10
    syscall

.data
name_prompt:
    .asciiz "What is your name? (36 chars)\n"
hello_str:
    .ascii "Hello "
name_str:
    .space 36
newline_str:
    .asciiz "\n"