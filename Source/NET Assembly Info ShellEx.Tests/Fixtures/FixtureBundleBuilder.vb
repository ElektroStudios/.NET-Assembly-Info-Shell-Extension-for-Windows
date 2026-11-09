
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Immutable
Imports System.IO
Imports System.IO.Compression
Imports System.Reflection.Metadata
Imports System.Reflection.PortableExecutable
Imports System.Text

#End Region

#Region " FixtureBundleEntry "

Namespace Fixtures

    ''' <summary>
    ''' A file to embed in a synthetic single-file bundle.
    ''' </summary>
    Friend NotInheritable Class FixtureBundleEntry

        ''' <summary>Gets the relative path of the file.</summary>
        Friend ReadOnly Property RelativePath As String

        ''' <summary>Gets the type of the file.</summary>
        Friend ReadOnly Property Type As BundleFileType

        ''' <summary>Gets the content of the file.</summary>
        Friend ReadOnly Property Content As Byte()

        ''' <summary>Gets a value indicating whether the file is stored compressed (Deflate).</summary>
        Friend ReadOnly Property Compress As Boolean

        ''' <summary>Initializes a new instance of the <see cref="FixtureBundleEntry"/> class.</summary>
        Friend Sub New(relativePath As String, type As BundleFileType, content As Byte(), Optional compress As Boolean = False)
            Me.RelativePath = relativePath
            Me.Type = type
            Me.Content = content
            Me.Compress = compress
        End Sub

        ''' <summary>Creates a text file entry (UTF-8).</summary>
        Friend Shared Function FromText(relativePath As String, type As BundleFileType, text As String) As FixtureBundleEntry
            Return New FixtureBundleEntry(relativePath, type, Encoding.UTF8.GetBytes(text))
        End Function

    End Class

#End Region

#Region " FixtureBundleBuilder "

    ''' <summary>
    ''' Emits synthetic .NET single-file bundles: a native host PE image (with the bundle placeholder in a
    ''' writable .data section, like the real apphost) followed by the embedded files and the bundle manifest.
    ''' </summary>
    Friend NotInheritable Class FixtureBundleBuilder

#Region " Native Host "

        ''' <summary>
        ''' A minimal native PE image with a .text section and a writable .data section.
        ''' </summary>
        Private NotInheritable Class NativeHostPEBuilder : Inherits PEBuilder

            ''' <summary>The content of the .data section.</summary>
            Private ReadOnly dataSection As Byte()

            ''' <summary>Initializes a new instance of the <see cref="NativeHostPEBuilder"/> class.</summary>
            Friend Sub New(machine As Machine, dataSection As Byte())
                MyBase.New(New PEHeaderBuilder(machine:=machine, imageCharacteristics:=Characteristics.ExecutableImage Or Characteristics.LargeAddressAware),
                           Function(content As IEnumerable(Of Blob)) New BlobContentId(Guid.Empty, 1UI))
                Me.dataSection = dataSection
            End Sub

            ''' <summary>Creates the sections.</summary>
            Protected Overrides Function CreateSections() As ImmutableArray(Of Section)
                Return ImmutableArray.Create(
                    New Section(".text", SectionCharacteristics.ContainsCode Or SectionCharacteristics.MemExecute Or SectionCharacteristics.MemRead),
                    New Section(".data", SectionCharacteristics.ContainsInitializedData Or SectionCharacteristics.MemRead Or SectionCharacteristics.MemWrite))
            End Function

            ''' <summary>Serializes a section.</summary>
            Protected Overrides Function SerializeSection(name As String, location As SectionLocation) As BlobBuilder
                Dim builder As New BlobBuilder()
                If name = ".text" Then
                    builder.WriteBytes(&HCC, 32) ' int3
                Else
                    builder.WriteBytes(Me.dataSection)
                End If
                Return builder
            End Function

            ''' <summary>Gets the data directories (none).</summary>
            Protected Overrides Function GetDirectories() As PEDirectoriesBuilder
                Return New PEDirectoriesBuilder()
            End Function

        End Class

#End Region

#Region " Constructors "

        ''' <summary>Prevents a default instance of the <see cref="FixtureBundleBuilder"/> class from being created.</summary>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Emits a single-file bundle to a temporary ".exe" file.
        ''' </summary>
        ''' <param name="entries">The files to embed.</param>
        ''' <param name="majorVersion">The bundle format major version (1, 2 or 6).</param>
        ''' <param name="fileName">The file name without extension, or <see langword="Nothing"/> for a random name.</param>
        ''' <returns>
        ''' The temporary fixture file.
        ''' </returns>
        Friend Shared Function Create(entries As IEnumerable(Of FixtureBundleEntry), Optional majorVersion As Integer = 6, Optional fileName As String = Nothing) As FixtureFile
            Return FixtureFile.CreateNamed(FixtureBundleBuilder.Build(entries, majorVersion), If(fileName, $"Bundle_{Guid.NewGuid():N}"), ".exe")
        End Function

        ''' <summary>
        ''' Emits a single-file bundle.
        ''' </summary>
        ''' <param name="entries">The files to embed; <see langword="Nothing"/> emits a plain apphost (header offset zero).</param>
        ''' <param name="majorVersion">The bundle format major version (1, 2 or 6).</param>
        ''' <returns>
        ''' The file bytes.
        ''' </returns>
        Friend Shared Function Build(entries As IEnumerable(Of FixtureBundleEntry), Optional majorVersion As Integer = 6) As Byte()

            ' Placeholder: 8-byte header offset (zero) + 32-byte signature, surrounded by padding.
            Dim dataSection As New List(Of Byte)
            dataSection.AddRange(New Byte(63) {})
            Dim placeholderIndex As Integer = dataSection.Count
            dataSection.AddRange(New Byte(7) {})
            dataSection.AddRange(SingleFileBundle.Signature)
            dataSection.AddRange(New Byte(63) {})

            Dim peBlob As New BlobBuilder()
            Dim host As New NativeHostPEBuilder(Machine.Amd64, dataSection.ToArray())
            host.Serialize(peBlob)
            Dim image As Byte() = peBlob.ToArray()

            If entries Is Nothing Then
                Return image ' A framework-dependent apphost that is not a bundle.
            End If

            Using output As New MemoryStream()
                output.Write(image, 0, image.Length)

                ' Embedded files.
                Dim records As New List(Of Tuple(Of Long, Long, Long, FixtureBundleEntry))
                For Each entry As FixtureBundleEntry In entries
                    Dim offset As Long = output.Length
                    Dim compressedSize As Long = 0
                    If entry.Compress Then
                        Dim compressed As Byte() = FixtureBundleBuilder.Deflate(entry.Content)
                        output.Write(compressed, 0, compressed.Length)
                        compressedSize = compressed.Length
                    Else
                        output.Write(entry.Content, 0, entry.Content.Length)
                    End If
                    records.Add(Tuple.Create(offset, CLng(entry.Content.Length), compressedSize, entry))
                Next entry

                ' Bundle header and manifest.
                Dim headerOffset As Long = output.Length
                Using writer As New BinaryWriter(output, Encoding.UTF8, leaveOpen:=True)
                    writer.Write(CUInt(majorVersion))
                    writer.Write(0UI)
                    writer.Write(records.Count)
                    writer.Write($"BundleId_{Guid.NewGuid():N}")
                    If majorVersion >= 2 Then
                        writer.Write(0L) : writer.Write(0L) : writer.Write(0L) : writer.Write(0L) : writer.Write(0UL)
                    End If
                    For Each record As Tuple(Of Long, Long, Long, FixtureBundleEntry) In records
                        writer.Write(record.Item1)
                        writer.Write(record.Item2)
                        If majorVersion >= 6 Then
                            writer.Write(record.Item3)
                        End If
                        writer.Write(CByte(record.Item4.Type))
                        writer.Write(record.Item4.RelativePath)
                    Next record
                End Using

                Dim bundle As Byte() = output.ToArray()
                FixtureBundleBuilder.PatchHeaderOffset(bundle, headerOffset)
                Return bundle
            End Using
        End Function

        ''' <summary>
        ''' Overwrites the bundle header offset stored before the bundle signature.
        ''' </summary>
        ''' <param name="bundle">The bundle bytes.</param>
        ''' <param name="headerOffset">The new header offset.</param>
        Friend Shared Sub PatchHeaderOffset(bundle As Byte(), headerOffset As Long)
            Dim signatureIndex As Integer = SingleFileBundle.IndexOf(bundle, SingleFileBundle.Signature)
            Array.Copy(BitConverter.GetBytes(headerOffset), 0, bundle, signatureIndex - 8, 8)
        End Sub

#End Region

#Region " Private Methods "

        ''' <summary>Compresses data with raw Deflate, like the .NET bundler.</summary>
        Private Shared Function Deflate(data As Byte()) As Byte()
            Using output As New MemoryStream()
                Using deflateStream As New DeflateStream(output, CompressionLevel.Optimal, leaveOpen:=True)
                    deflateStream.Write(data, 0, data.Length)
                End Using
                Return output.ToArray()
            End Using
        End Function

#End Region

    End Class

End Namespace

#End Region
