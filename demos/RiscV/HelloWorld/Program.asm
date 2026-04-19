# Avishai Dernis 2026

# Prints "Hello World" using a syscall

.globl entry

.text
entry:
    
    # Print "Hello World"
	xori    a0,     zero,   hello_world
	xori    a1,     zero,   0
	xori    a7,     zero,   3
	ecall

    # Shutdown
    xori    a7,     zero,  9
    ecall

.data
hello_world:    .asciiz "Hello World!\n"
