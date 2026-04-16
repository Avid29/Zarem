# Avishai Dernis 2026

# Prints "Hello World" using a syscall

.globl entry

.text
entry:
	xori		$a0,	$zero,	hello_world
	xori		$v0,	$zero,	4
	syscall

.data
hello_world:
	.asciiz "Hello World!"
