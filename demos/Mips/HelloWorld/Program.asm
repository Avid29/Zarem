# Avishai Dernis 2026

# Prints "Hello World" using a syscall

.globl entry

.text
entry:
    
    # Print "Hello World"
	la      $a0,    hello_world
	li      $a2,    0
	xori	$v0,	$zero,	3
	syscall

    # Shutdown
    xori    $v0,    $zero,  9
    syscall

.data
hello_world:    .asciiz "Hello World!\n"
