# Avishai Dernis 2026

# Prints the number 42 using a syscall

entry:
	xori		$a0,	$zero,	42
	xori		$v0,	$zero,	1
	syscall
