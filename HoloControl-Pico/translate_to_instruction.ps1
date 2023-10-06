function Get-InstructionBits {
    [CmdletBinding()]
    param([Char]$Instruction)
    Write-Output ([int]$Instruction -band 31)
}
"abcdefghijklmnopqrstuvwxyz".ToCharArray() | ForEach-Object {Get-InstructionBits($_)}