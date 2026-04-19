# This project exists to ensure the proper handling of
# utf16 and right-to-left lettering systems

# פרויקט הזו קיים להבטיח תמיכה לשימוש אותיות עברית

.globl כניסה

כניסה:
    # הדפס שלום עולם
    la    $a0,    שלום_עולם_מחרוזת
    li    $a2,    2
    li    $v0,    3
    syscall
    
    # יוצא את תהליך
    xori    $v0,    $zero,  9
    syscall

.data
שלום_עולם_מחרוזת:   .utf16 "שלום עולם\n"