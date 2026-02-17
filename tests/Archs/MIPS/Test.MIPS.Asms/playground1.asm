.globl label1
.globl label2

label1:
	ori		$s0,	$zero,	10
	ori		$s1,	$zero,	'a'
label1_main:
	add		$t0,	$s0,	$s1
	j		label2
    
label1_end:
.data
label3:
