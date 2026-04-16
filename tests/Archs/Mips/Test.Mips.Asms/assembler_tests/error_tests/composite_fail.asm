# Adam Dernis 2024

# Features:
# ------------------
# Label usage:	Entry only
# Macros:		No
# Jumps:		Yes, with links
# Branches:		No
# Memory:		Yes
# Syscalls:		Yes
#

main:
	lui		$v0,	0,			0		# Extra argument
	ori		$a0,	$zero,		0
	xkcd	$t0,	$a0,		0		# No such instruction
	syscall

	lui		$at,	10($t0)				# Wrong argument
	sw	    $v0,	176($at)
	lui		$at,	4096
	sw		$v1,	180($at)
	lw		$a0,	0($sp)
	lw		$a1,	4(sp)				# Not a register
	lw		$a2,	8($sp)		

	jal		1048576
	
	sll		$zero,		$zero,		0		# Warning: No operation
	sll		$t0,		$s0,		32		# Warning: Truncation
	add		,		    $v0,		$zero   # Empty Argument
	ori		$v0,		$hero,		17		# Not a valid register
	syscall

	addi	$t0, $t1, ''					# Empty character literal
