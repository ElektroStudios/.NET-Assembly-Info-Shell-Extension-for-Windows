
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.ObjectModel
Imports System.IO
Imports System.IO.Compression
Imports System.Reflection.PortableExecutable
Imports System.Text

Imports AssemblyInspection

#End Region

#Region " BundleFileType "

''' <summary>
''' Specifies the type of a file embedded in a .NET single-file bundle.
''' </summary>
Public Enum BundleFileType As Byte

    ''' <summary>Any other file.</summary>
    Unknown = 0

    ''' <summary>A managed assembly.</summary>
    Assembly = 1

    ''' <summary>A native binary.</summary>
    NativeBinary = 2

    ''' <summary>The deps.json file.</summary>
    DepsJson = 3

    ''' <summary>The runtimeconfig.json file.</summary>
    RuntimeConfigJson = 4

    ''' <summary>A symbols (PDB) file.</summary>
    Symbols = 5

End Enum

#End Region

#Region " BundleEntry "

''' <summary>
''' Represents a file embedded in a .NET single-file bundle.
''' </summary>
<DebuggerDisplay("{RelativePath,nq} ({Type})")>
Public NotInheritable Class BundleEntry

    ''' <summary>Gets the path of the file, relative to the application directory.</summary>
    Public ReadOnly Property RelativePath As String

    ''' <summary>Gets the type of the file.</summary>
    Public ReadOnly Property Type As BundleFileType

    ''' <summary>Gets the offset of the file data inside the bundle.</summary>
    Public ReadOnly Property Offset As Long

    ''' <summary>Gets the uncompressed size of the file.</summary>
    Public ReadOnly Property Size As Long

    ''' <summary>Gets the compressed size of the file, or zero if the file is not compressed.</summary>
    Public ReadOnly Property CompressedSize As Long

    ''' <summary>Gets a value indicating whether the file is compressed (Deflate).</summary>
    Public ReadOnly Property IsCompressed As Boolean
        Get
            Return Me.CompressedSize <> 0
        End Get
    End Property

    ''' <summary>
    ''' Initializes a new instance of the <see cref="BundleEntry"/> class.
    ''' </summary>
    ''' <param name="relativePath">The relative path of the file.</param>
    ''' <param name="type">The type of the file.</param>
    ''' <param name="offset">The offset of the file data inside the bundle.</param>
    ''' <param name="size">The uncompressed size of the file.</param>
    ''' <param name="compressedSize">The compressed size of the file, or zero.</param>
    Friend Sub New(relativePath As String, type As BundleFileType, offset As Long, size As Long, compressedSize As Long)
        Me.RelativePath = relativePath
        Me.Type = type
        Me.Offset = offset
        Me.Size = size
        Me.CompressedSize = compressedSize
    End Sub

End Class

#End Region

#Region " SingleFileBundle "

''' <summary>
''' Reads the manifest of a .NET single-file bundle (an apphost executable with the application files appended).
''' </summary>
''' <remarks>
''' The apphost contains, in a writable data section, an 8-byte bundle header offset followed by a
''' well-known 32-byte signature. A zero offset means that the apphost is not a bundle.
''' <para></para>
''' Bundle header: <c>UInt32 MajorVersion, UInt32 MinorVersion, Int32 FileCount, String BundleId</c>;
''' then, for version 2 or newer: <c>Int64 DepsJsonOffset, Int64 DepsJsonSize, Int64 RuntimeConfigOffset, Int64 RuntimeConfigSize, UInt64 Flags</c>.
''' <para></para>
''' Each file entry: <c>Int64 Offset, Int64 Size</c>, <c>Int64 CompressedSize</c> (version 6 or newer), <c>Byte Type, String RelativePath</c>.
''' Strings use the <see cref="BinaryWriter"/> format (7-bit encoded length prefix and UTF-8 bytes).
''' </remarks>
Public NotInheritable Class SingleFileBundle

#Region " Constants / Fields "

    ''' <summary>The bundle signature that follows the bundle header offset inside the apphost.</summary>
    Friend Shared ReadOnly Signature As Byte() = {
        &H8B, &H12, &H2, &HB9, &H6A, &H61, &H20, &H38, &H72, &H7B, &H93, &H2, &H14, &HD7, &HA0, &H32,
        &H13, &HF5, &HB9, &HE6, &HEF, &HAE, &H33, &H18, &HEE, &H3B, &H2D, &HCE, &H24, &HB3, &H6A, &HAE
    }

    ''' <summary>The maximum number of entries accepted, to reject corrupted headers early.</summary>
    Private Const MaxFileCount As Integer = 1000000

    ''' <summary>The maximum size of a data section that is scanned for the bundle signature.</summary>
    Private Const MaxScannedSectionSize As Integer = 4 * 1024 * 1024

#End Region

#Region " Properties "

    ''' <summary>Gets the major version of the bundle format (1 = .NET Core 3.x, 2 = .NET 5, 6 = .NET 6 or newer).</summary>
    Public ReadOnly Property MajorVersion As Integer

    ''' <summary>Gets the minor version of the bundle format.</summary>
    Public ReadOnly Property MinorVersion As Integer

    ''' <summary>Gets the unique identifier of the bundle.</summary>
    Public ReadOnly Property BundleId As String

    ''' <summary>Gets the offset of the bundle header inside the file.</summary>
    Public ReadOnly Property HeaderOffset As Long

    ''' <summary>Gets the files embedded in the bundle.</summary>
    Public ReadOnly Property Entries As ReadOnlyCollection(Of BundleEntry)

    ''' <summary>Gets a value indicating whether any embedded file is compressed.</summary>
    Public ReadOnly Property IsCompressed As Boolean
        Get
            Return Me.Entries.Any(Function(entry As BundleEntry) entry.IsCompressed)
        End Get
    End Property

#End Region

#Region " Constructors "

    ''' <summary>Initializes a new instance of the <see cref="SingleFileBundle"/> class.</summary>
    Private Sub New(majorVersion As Integer, minorVersion As Integer, bundleId As String, headerOffset As Long, entries As List(Of BundleEntry))
        Me.MajorVersion = majorVersion
        Me.MinorVersion = minorVersion
        Me.BundleId = bundleId
        Me.HeaderOffset = headerOffset
        Me.Entries = entries.AsReadOnly()
    End Sub

#End Region

#Region " Public Methods "

    ''' <summary>
    ''' Reads the bundle manifest of the specified PE image, if it is a single-file bundle.
    ''' </summary>
    ''' <param name="stream">
    ''' A seekable stream positioned anywhere; the whole stream must contain the file.
    ''' </param>
    ''' <returns>
    ''' The bundle, or <see langword="Nothing"/> if the image is not a single-file bundle.
    ''' </returns>
    ''' <exception cref="BadImageFormatException">
    ''' The stream is not a PE image, or it is a bundle with a corrupted manifest.
    ''' </exception>
    Public Shared Function TryRead(stream As Stream) As SingleFileBundle

        If stream Is Nothing Then
            Throw New ArgumentNullException(NameOf(stream))
        End If
        If Not stream.CanSeek Then
            Throw New ArgumentException("The stream must be seekable.", NameOf(stream))
        End If

        stream.Position = 0
        Dim headers As New PEHeaders(stream)

        Dim headerOffset As Long = SingleFileBundle.FindHeaderOffset(stream, headers)
        If headerOffset = 0 Then
            Return Nothing ' Not an apphost, or an apphost without bundle (framework-dependent, not single-file).
        End If
        If (headerOffset < 0) OrElse (headerOffset >= stream.Length) Then
            Throw New BadImageFormatException("The single-file bundle header offset is out of range.")
        End If

        Try
            Return SingleFileBundle.ReadManifest(stream, headerOffset)

        Catch ex As EndOfStreamException
            Throw New BadImageFormatException("The single-file bundle manifest is truncated.", ex)

        End Try
    End Function

    ''' <summary>
    ''' Reads (and decompresses, if needed) the content of an embedded file.
    ''' </summary>
    ''' <param name="stream">
    ''' The stream of the bundle file.
    ''' </param>
    '''
    ''' <param name="entry">
    ''' The embedded file.
    ''' </param>
    ''' <returns>
    ''' The file content.
    ''' </returns>
    ''' <exception cref="BadImageFormatException">
    ''' The embedded file is truncated or its compressed data is corrupted.
    ''' </exception>
    Public Function ReadEntry(stream As Stream, entry As BundleEntry) As Byte()

        If stream Is Nothing Then
            Throw New ArgumentNullException(NameOf(stream))
        End If
        If entry Is Nothing Then
            Throw New ArgumentNullException(NameOf(entry))
        End If

        Dim storedSize As Long = If(entry.IsCompressed, entry.CompressedSize, entry.Size)
        If (entry.Offset < 0) OrElse (storedSize < 0) OrElse (entry.Offset + storedSize > stream.Length) OrElse (entry.Size > Integer.MaxValue) Then
            Throw New BadImageFormatException($"The embedded file '{entry.RelativePath}' is out of the bundle range.")
        End If

        Dim stored As Byte() = New Byte(CInt(storedSize) - 1) {}
        stream.Position = entry.Offset
        SingleFileBundle.ReadExactly(stream, stored)

        If Not entry.IsCompressed Then
            Return stored
        End If

        Try
            Using compressed As New MemoryStream(stored, writable:=False),
                  deflate As New DeflateStream(compressed, CompressionMode.Decompress),
                  output As New MemoryStream(CInt(entry.Size))
                deflate.CopyTo(output)
                If output.Length <> entry.Size Then
                    Throw New BadImageFormatException($"The embedded file '{entry.RelativePath}' has an unexpected decompressed size.")
                End If
                Return output.ToArray()
            End Using

        Catch ex As InvalidDataException
            Throw New BadImageFormatException($"The compressed data of the embedded file '{entry.RelativePath}' is corrupted.", ex)

        End Try
    End Function

    ''' <summary>
    ''' Determines the main (application) assembly of the bundle.
    ''' </summary>
    ''' <param name="stream">
    ''' The stream of the bundle file.
    ''' </param>
    '''
    ''' <param name="bundleFileName">
    ''' The file name of the bundle (e.g. "MyApp.exe").
    ''' </param>
    ''' <returns>
    ''' The entry of the main assembly.
    ''' </returns>
    ''' <remarks>
    ''' Strategies, by priority:
    ''' <list type="number">
    '''   <item><description>The assembly named like the bundle ("MyApp.exe" -> "MyApp.dll"), as the host does.</description></item>
    '''   <item><description>The "project" library declared in the embedded deps.json (useful when the bundle was renamed).</description></item>
    '''   <item><description>The only assembly with a managed entry point.</description></item>
    ''' </list>
    ''' </remarks>
    ''' <exception cref="BadImageFormatException">
    ''' The main assembly could not be determined.
    ''' </exception>
    Public Function ResolveMainAssembly(stream As Stream, bundleFileName As String) As BundleEntry

        If String.IsNullOrEmpty(bundleFileName) Then
            Throw New ArgumentNullException(NameOf(bundleFileName))
        End If

        Dim assemblies As List(Of BundleEntry) = Me.Entries.Where(Function(entry As BundleEntry) entry.Type = BundleFileType.Assembly).ToList()

        ' 1. Same name as the bundle.
        Dim expectedName As String = $"{Path.GetFileNameWithoutExtension(bundleFileName)}.dll"
        Dim match As BundleEntry = assemblies.FirstOrDefault(
            Function(entry As BundleEntry) String.Equals(entry.RelativePath, expectedName, StringComparison.OrdinalIgnoreCase))
        If match IsNot Nothing Then
            Return match
        End If

        ' 2. The "project" library of deps.json.
        Dim depsJson As BundleEntry = Me.Entries.FirstOrDefault(Function(entry As BundleEntry) entry.Type = BundleFileType.DepsJson)
        If depsJson IsNot Nothing Then
            Dim projectName As String = SingleFileBundle.GetDepsJsonProjectName(TextFileDecoder.Decode(Me.ReadEntry(stream, depsJson)))
            If projectName.Length <> 0 Then
                match = assemblies.FirstOrDefault(
                    Function(entry As BundleEntry) String.Equals(entry.RelativePath, $"{projectName}.dll", StringComparison.OrdinalIgnoreCase))
                If match IsNot Nothing Then
                    Return match
                End If
            End If
        End If

        ' 3. The only assembly with a managed entry point.
        Dim executables As New List(Of BundleEntry)
        For Each entry As BundleEntry In assemblies
            Using image As New MemoryStream(Me.ReadEntry(stream, entry), writable:=False)
                Dim headers As New PEHeaders(image)
                If (headers.CorHeader IsNot Nothing) AndAlso Not headers.IsDll AndAlso (headers.CorHeader.EntryPointTokenOrRelativeVirtualAddress <> 0) Then
                    executables.Add(entry)
                End If
            End Using
        Next entry

        If executables.Count = 1 Then
            Return executables(0)
        End If

        Throw New BadImageFormatException("The main assembly of the single-file bundle could not be determined.")
    End Function

#End Region

#Region " Friend Methods "

    ''' <summary>
    ''' Gets the name of the "project" library declared in a deps.json document.
    ''' </summary>
    ''' <param name="depsJson">
    ''' The deps.json text.
    ''' </param>
    ''' <returns>
    ''' The project name (e.g. "MyApp" for a library named "MyApp/1.0.0"),
    ''' or <see cref="String.Empty"/> if it cannot be determined.
    ''' </returns>
    Friend Shared Function GetDepsJsonProjectName(depsJson As String) As String

        Dim root As XElement
        Try
            root = JsonDocumentReader.Parse(depsJson)

        Catch ex As Xml.XmlException
            Return String.Empty ' Not valid JSON: the other strategies still apply.

        End Try

        Dim libraries As XElement = JsonDocumentReader.GetMember(root, "libraries")
        If libraries Is Nothing Then
            Return String.Empty
        End If

        For Each library As XElement In libraries.Elements()
            If String.Equals(JsonDocumentReader.GetMemberValue(library, "type"), "project", StringComparison.Ordinal) Then
                Dim name As String = JsonDocumentReader.GetMemberName(library)
                Dim slashIndex As Integer = name.IndexOf("/"c)
                Return If(slashIndex = -1, name, name.Substring(0, slashIndex))
            End If
        Next library

        Return String.Empty
    End Function

#End Region

#Region " Private Methods "

    ''' <summary>
    ''' Searches the bundle signature in the writable, initialized data sections of the image (where the apphost
    ''' keeps it), and returns the header offset that precedes it.
    ''' </summary>
    ''' <param name="stream">
    ''' The stream of the file.
    ''' </param>
    '''
    ''' <param name="headers">
    ''' The PE headers.
    ''' </param>
    ''' <returns>
    ''' The bundle header offset, or zero if the signature was not found (or the apphost is not a bundle).
    ''' </returns>
    Private Shared Function FindHeaderOffset(stream As Stream, headers As PEHeaders) As Long

        Const DataSectionFlags As SectionCharacteristics = SectionCharacteristics.ContainsInitializedData Or SectionCharacteristics.MemWrite

        For Each section As SectionHeader In headers.SectionHeaders
            If (section.SectionCharacteristics And DataSectionFlags) <> DataSectionFlags Then
                Continue For
            End If
            If (section.SizeOfRawData <= 0) OrElse (section.SizeOfRawData > SingleFileBundle.MaxScannedSectionSize) OrElse
               (CLng(section.PointerToRawData) + section.SizeOfRawData > stream.Length) Then
                Continue For
            End If

            Dim data As Byte() = New Byte(section.SizeOfRawData - 1) {}
            stream.Position = section.PointerToRawData
            SingleFileBundle.ReadExactly(stream, data)

            Dim index As Integer = SingleFileBundle.IndexOf(data, SingleFileBundle.Signature)
            If index >= 8 Then
                Return BitConverter.ToInt64(data, index - 8)
            End If
        Next section

        Return 0
    End Function

    ''' <summary>
    ''' Reads the bundle manifest (header and file entries).
    ''' </summary>
    ''' <param name="stream">
    ''' The stream of the file.
    ''' </param>
    '''
    ''' <param name="headerOffset">
    ''' The bundle header offset.
    ''' </param>
    ''' <returns>
    ''' The bundle.
    ''' </returns>
    Private Shared Function ReadManifest(stream As Stream, headerOffset As Long) As SingleFileBundle

        stream.Position = headerOffset
        Using reader As New BinaryReader(stream, Encoding.UTF8, leaveOpen:=True)
            Dim majorVersion As Integer = CInt(reader.ReadUInt32())
            Dim minorVersion As Integer = CInt(reader.ReadUInt32())
            Dim fileCount As Integer = reader.ReadInt32()
            Dim bundleId As String = reader.ReadString()

            If (majorVersion < 1) OrElse (majorVersion > 100) OrElse (fileCount < 0) OrElse (fileCount > SingleFileBundle.MaxFileCount) Then
                Throw New BadImageFormatException("The single-file bundle header is corrupted.")
            End If

            If majorVersion >= 2 Then
                reader.ReadInt64() ' deps.json offset
                reader.ReadInt64() ' deps.json size
                reader.ReadInt64() ' runtimeconfig.json offset
                reader.ReadInt64() ' runtimeconfig.json size
                reader.ReadUInt64() ' flags
            End If

            Dim entries As New List(Of BundleEntry)(fileCount)
            For i As Integer = 0 To fileCount - 1
                Dim offset As Long = reader.ReadInt64()
                Dim size As Long = reader.ReadInt64()
                Dim compressedSize As Long = If(majorVersion >= 6, reader.ReadInt64(), 0L)
                Dim type As BundleFileType = CType(reader.ReadByte(), BundleFileType)
                Dim relativePath As String = reader.ReadString()
                entries.Add(New BundleEntry(relativePath, type, offset, size, compressedSize))
            Next i

            Return New SingleFileBundle(majorVersion, minorVersion, bundleId, headerOffset, entries)
        End Using
    End Function

    ''' <summary>
    ''' Finds the first occurrence of a byte sequence.
    ''' </summary>
    ''' <param name="data">The data to search.</param>
    ''' <param name="pattern">The byte sequence to find.</param>
    ''' <returns>
    ''' The index of the first occurrence, or -1.
    ''' </returns>
    Friend Shared Function IndexOf(data As Byte(), pattern As Byte()) As Integer

        Dim first As Byte = pattern(0)
        Dim lastStart As Integer = data.Length - pattern.Length
        For i As Integer = 0 To lastStart
            If data(i) <> first Then
                Continue For
            End If
            Dim matched As Boolean = True
            For j As Integer = 1 To pattern.Length - 1
                If data(i + j) <> pattern(j) Then
                    matched = False
                    Exit For
                End If
            Next j
            If matched Then
                Return i
            End If
        Next i
        Return -1
    End Function

    ''' <summary>
    ''' Fills the buffer from the stream, or throws if the stream ends before.
    ''' </summary>
    ''' <param name="stream">The stream.</param>
    ''' <param name="buffer">The buffer to fill.</param>
    Private Shared Sub ReadExactly(stream As Stream, buffer As Byte())

        Dim total As Integer = 0
        While total < buffer.Length
            Dim read As Integer = stream.Read(buffer, total, buffer.Length - total)
            If read = 0 Then
                Throw New BadImageFormatException("Unexpected end of file.")
            End If
            total += read
        End While
    End Sub

#End Region

End Class

#End Region
