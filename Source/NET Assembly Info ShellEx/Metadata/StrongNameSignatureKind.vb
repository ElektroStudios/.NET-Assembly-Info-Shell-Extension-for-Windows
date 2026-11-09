
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " StrongNameSignatureKind "

''' <summary>
''' Specifies the strong-name signature state of an assembly.
''' </summary>
Public Enum StrongNameSignatureKind As Integer

    ''' <summary>
    ''' The assembly has no public key, so it is not strong-named.
    ''' </summary>
    NotSigned = 0

    ''' <summary>
    ''' The assembly declares a public key, but the CLI header is not flagged as strong-name signed
    ''' (typically a delay-signed assembly).
    ''' </summary>
    DelaySigned = 1

    ''' <summary>
    ''' The assembly declares a public key and the CLI header is flagged as strong-name signed.
    ''' </summary>
    Signed = 2

End Enum

#End Region
