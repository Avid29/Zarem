# Adam Dernis 2024

# This test contains a branch using a label.
# Its expected behavior is to loop
# 5 times before falling through to completion.

# Features:
# ------------------
# Label usage:	Yes
# Macros:		No
# Jumps:		No
# Branches:		Yes
# Memory:		No
# Syscalls:		No
#

	ori		$s0,	$zero,	5
loop:
	addi	$s0,	$s0,	-1
	bgez	$s0,	loop
