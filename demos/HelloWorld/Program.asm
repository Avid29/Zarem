# Avishai Dernis 2026

# Prints "Hello World" using a syscall

.text
entry:
    
    # Print "Hello World"
	xori 	$a0,	$zero,  hello_world
	xori	$v0,	$zero,	4
	syscall

    # Shutdown
    xori    $v0,    $zero,  10
    syscall

.data
hello_world:
	.asciiz "Hello World!"
