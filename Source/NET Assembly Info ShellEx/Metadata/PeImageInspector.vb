
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Reflection
Imports System.Reflection.PortableExecutable

#End Region

#Region " PeImageInspector "

Namespace AssemblyInspection

    ''' <summary>
    ''' Provides methods to interpret the portable executable (PE) headers of a managed image.
    ''' </summary>
    Friend Module PeImageInspector

#Region " Private Fields "

        ''' <summary>
        ''' The machine types that are recognized by this module.
        ''' </summary>
        Private ReadOnly KnownMachines As Machine() = {
            Machine.I386, Machine.Amd64, Machine.Arm64, Machine.Arm, Machine.ArmThumb2, Machine.IA64
        }

        ''' <summary>
        ''' The values that ReadyToRun (crossgen) XORs into the machine type of non-Windows images,
        ''' and the name of their target operating system.
        ''' </summary>
        Private ReadOnly ReadyToRunOsMachineOverrides As New Dictionary(Of Integer, String) From {
            {&H7B79, "Linux"},
            {&H4644, "macOS"},
            {&HADC4, "FreeBSD"},
            {&H1993, "NetBSD"},
            {&H1992, "SunOS"}
        }

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Resolves the real machine type of an image,
        ''' decoding the operating system override applied by ReadyToRun to non-Windows images.
        ''' </summary>
        ''' <param name="rawMachine">
        ''' The machine type, as stored in the COFF header.
        ''' </param>
        '''
        ''' <param name="targetOs">
        ''' When this method returns, contains the name of the target operating system
        ''' if the machine type had an override applied; otherwise, <see cref="String.Empty"/>.
        ''' </param>
        ''' <returns>
        ''' The resolved machine type.
        ''' </returns>
        Friend Function ResolveMachine(rawMachine As Machine, ByRef targetOs As String) As Machine

            targetOs = String.Empty
            If PeImageInspector.KnownMachines.Contains(rawMachine) Then
                Return rawMachine
            End If

            For Each pair As KeyValuePair(Of Integer, String) In PeImageInspector.ReadyToRunOsMachineOverrides
                Dim candidate As Machine = CType(CInt(rawMachine) Xor pair.Key, Machine)
                If PeImageInspector.KnownMachines.Contains(candidate) Then
                    targetOs = pair.Value
                    Return candidate
                End If
            Next pair

            Return rawMachine
        End Function

        ''' <summary>
        ''' Gets the <see cref="ProcessorArchitecture"/> of an image, as used in assembly display names and in the GAC.
        ''' </summary>
        ''' <param name="machine">
        ''' The machine type, as stored in the COFF header.
        ''' </param>
        '''
        ''' <param name="flags">
        ''' The CLI header flags.
        ''' </param>
        ''' <returns>
        ''' The resulting <see cref="ProcessorArchitecture"/>.
        ''' <para></para>
        ''' <see cref="ProcessorArchitecture.None"/> is returned for architectures
        ''' that have no <see cref="ProcessorArchitecture"/> value (e.g. ARM64).
        ''' </returns>
        Friend Function GetProcessorArchitecture(machine As Machine, flags As CorFlags) As ProcessorArchitecture

            Dim targetOs As String = Nothing
            Select Case PeImageInspector.ResolveMachine(machine, targetOs)

                Case Machine.I386
                    Dim isIlOnly As Boolean = (flags And CorFlags.ILOnly) = CorFlags.ILOnly
                    Dim requires32Bit As Boolean = (flags And CorFlags.Requires32Bit) = CorFlags.Requires32Bit
                    ' "Any CPU (prefer 32-bit)" images are still MSIL; only a hard 32-bit requirement makes them x86.
                    Dim prefers32Bit As Boolean = (flags And CorFlags.Prefers32Bit) = CorFlags.Prefers32Bit
                    Return If(isIlOnly AndAlso (Not requires32Bit OrElse prefers32Bit),
                              ProcessorArchitecture.MSIL,
                              ProcessorArchitecture.X86)

                Case Machine.Amd64
                    Return ProcessorArchitecture.Amd64

                Case Machine.IA64
                    Return ProcessorArchitecture.IA64

                Case Machine.Arm, Machine.ArmThumb2
                    Return ProcessorArchitecture.Arm

                Case Else
                    Return ProcessorArchitecture.None

            End Select
        End Function

        ''' <summary>
        ''' Gets a friendly name of the target platform of an image (e.g. "Any CPU", "x86 (32-bit)", "ARM64 (64-bit)").
        ''' </summary>
        ''' <param name="machine">
        ''' The machine type, as stored in the COFF header.
        ''' </param>
        '''
        ''' <param name="flags">
        ''' The CLI header flags.
        ''' </param>
        ''' <returns>
        ''' The friendly platform name.
        ''' </returns>
        Friend Function GetPlatformName(machine As Machine, flags As CorFlags) As String

            Dim targetOs As String = Nothing
            Dim resolvedMachine As Machine = PeImageInspector.ResolveMachine(machine, targetOs)
            Dim platformName As String

            Select Case resolvedMachine

                Case Machine.I386
                    Dim isIlOnly As Boolean = (flags And CorFlags.ILOnly) = CorFlags.ILOnly
                    Dim requires32Bit As Boolean = (flags And CorFlags.Requires32Bit) = CorFlags.Requires32Bit
                    Dim prefers32Bit As Boolean = (flags And CorFlags.Prefers32Bit) = CorFlags.Prefers32Bit

                    If Not isIlOnly Then
                        platformName = "x86 (32-bit)"
                    ElseIf prefers32Bit Then
                        platformName = "Any CPU (prefer 32-bit)"
                    ElseIf requires32Bit Then
                        platformName = "x86 (32-bit)"
                    Else
                        platformName = "Any CPU"
                    End If

                Case Machine.Amd64
                    platformName = "x64 (64-bit)"

                Case Machine.Arm64
                    platformName = "ARM64 (64-bit)"

                Case Machine.Arm, Machine.ArmThumb2
                    platformName = "ARM (32-bit)"

                Case Machine.IA64
                    platformName = "Itanium (64-bit)"

                Case Else
                    platformName = $"Unknown (0x{CUShort(machine):X4})"

            End Select

            Return If(targetOs.Length = 0, platformName, $"{platformName} - {targetOs}")
        End Function

        ''' <summary>
        ''' Gets the output type of an image, using the Visual Studio project terminology
        ''' (e.g. "Class Library (DLL)", "Console Application (EXE)", "Windows Application (EXE)").
        ''' </summary>
        ''' <param name="headers">
        ''' The PE headers.
        ''' </param>
        ''' <returns>
        ''' The output type.
        ''' </returns>
        ''' <exception cref="ArgumentNullException">
        ''' <paramref name="headers"/> is <see langword="Nothing"/>.
        ''' </exception>
        '''
        ''' <exception cref="BadImageFormatException">
        ''' The image has no PE optional header.
        ''' </exception>
        Friend Function GetOutputType(headers As PEHeaders) As String

            If headers Is Nothing Then
                Throw New ArgumentNullException(NameOf(headers))
            End If
            If headers.PEHeader Is Nothing Then
                Throw New BadImageFormatException("The image has no PE optional header.")
            End If

            ' NOTE: the subsystem of a DLL is usually "Windows CUI", so 'IsDll' must be checked first.
            If headers.IsDll Then
                Return "Class Library (DLL)"
            End If

            Select Case headers.PEHeader.Subsystem
                Case Subsystem.WindowsCui
                    Return "Console Application (EXE)"

                Case Subsystem.WindowsGui
                    Return "Windows Application (EXE)"

                Case Else
                    Return $"Executable ({headers.PEHeader.Subsystem})"
            End Select
        End Function

#End Region

    End Module

End Namespace

#End Region
