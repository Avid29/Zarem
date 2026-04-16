# Adam Dernis 2024

# This test contains a branch using a literal and no labels.
# Its expected behavior is to loop
# 5 times before falling through to completion.

# Features:
# ------------------
# Label usage:	No
# Macros:		No
# Jumps:		No
# Branches:		Yes
# Memory:		No
# Syscalls:		No
#

ori		$s0,	$zero,	5
addi	$s0,	$s0,	-1
bgez	$s0,	-8
