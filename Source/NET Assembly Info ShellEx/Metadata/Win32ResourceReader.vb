
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Immutable
Imports System.Reflection.PortableExecutable

#End Region

#Region " Win32ResourceReader "

Namespace AssemblyInspection

    ''' <summary>
    ''' Reads Win32 resources from the resource section (.rsrc) of a PE image.
    ''' </summary>
    ''' <remarks>
    ''' The resource directory is a fixed three-level tree: type, name (or ID) and language.
    ''' Each level is read with plain loops; no recursion is involved.
    ''' </remarks>
    Friend Module Win32ResourceReader

#Region " Constants "

        ''' <summary>The <c>RT_MANIFEST</c> resource type.</summary>
        Friend Const ManifestResourceType As Integer = 24

        ''' <summary>The size of an <c>IMAGE_RESOURCE_DIRECTORY</c> structure.</summary>
        Private Const DirectorySize As Integer = 16

        ''' <summary>The size of an <c>IMAGE_RESOURCE_DIRECTORY_ENTRY</c> structure.</summary>
        Private Const DirectoryEntrySize As Integer = 8

        ''' <summary>The size of an <c>IMAGE_RESOURCE_DATA_ENTRY</c> structure.</summary>
        Private Const DataEntrySize As Integer = 16

        ''' <summary>The high bit that flags a named entry, or an entry that points to a subdirectory.</summary>
        Private Const HighBit As UInteger = &H80000000UI

#End Region

#Region " Types "

        ''' <summary>
        ''' An <c>IMAGE_RESOURCE_DIRECTORY_ENTRY</c>.
        ''' </summary>
        Private Structure DirectoryEntry

            ''' <summary>A value indicating whether the entry is identified by a string name instead of an integer ID.</summary>
            Friend IsNamed As Boolean

            ''' <summary>The integer ID (only meaningful when <see cref="IsNamed"/> is <see langword="False"/>).</summary>
            Friend Id As Integer

            ''' <summary>A value indicating whether <see cref="Offset"/> points to a subdirectory instead of a data entry.</summary>
            Friend IsDirectory As Boolean

            ''' <summary>The offset, relative to the start of the resource section, of the subdirectory or data entry.</summary>
            Friend Offset As Integer

        End Structure

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Reads the first resource of the specified type (lowest integer ID, first language).
        ''' </summary>
        ''' <param name="peReader">
        ''' The PE reader.
        ''' </param>
        '''
        ''' <param name="resourceType">
        ''' The integer resource type (e.g. <see cref="ManifestResourceType"/>).
        ''' </param>
        '''
        ''' <param name="resourceId">
        ''' When this method returns, contains the integer ID of the resource that was read, or zero.
        ''' </param>
        ''' <returns>
        ''' The resource data, or <see langword="Nothing"/> if the image has no resource of that type.
        ''' </returns>
        ''' <exception cref="BadImageFormatException">
        ''' The resource section is corrupted.
        ''' </exception>
        Friend Function ReadFirstResource(peReader As PEReader, resourceType As Integer, ByRef resourceId As Integer) As Byte()

            If peReader Is Nothing Then
                Throw New ArgumentNullException(NameOf(peReader))
            End If

            resourceId = 0
            Dim peHeader As PEHeader = peReader.PEHeaders.PEHeader
            If (peHeader Is Nothing) OrElse (peHeader.ResourceTableDirectory.RelativeVirtualAddress = 0) OrElse (peHeader.ResourceTableDirectory.Size = 0) Then
                Return Nothing
            End If

            Dim section As ImmutableArray(Of Byte) = peReader.GetSectionData(peHeader.ResourceTableDirectory.RelativeVirtualAddress).GetContent()
            If section.IsDefaultOrEmpty Then
                Throw New BadImageFormatException("The resource directory points outside of the image sections.")
            End If
            Dim data As Byte() = section.ToArray()

            ' Level 1: resource type.
            Dim typeEntry As DirectoryEntry
            If Not Win32ResourceReader.TryFindEntry(data, 0, resourceType, typeEntry) Then
                Return Nothing
            End If

            ' Level 2: resource name/ID (the lowest integer ID).
            Dim nameEntry As DirectoryEntry
            If Not Win32ResourceReader.TryFindEntry(data, typeEntry.Offset, Nothing, nameEntry) Then
                Return Nothing
            End If

            ' Level 3: language (the first one).
            Dim languageEntry As DirectoryEntry
            If Not Win32ResourceReader.TryFindEntry(data, nameEntry.Offset, Nothing, languageEntry) OrElse languageEntry.IsDirectory Then
                Throw New BadImageFormatException("The resource directory has an unexpected structure.")
            End If

            ' IMAGE_RESOURCE_DATA_ENTRY: OffsetToData (RVA), Size, CodePage, Reserved.
            Win32ResourceReader.EnsureRange(data, languageEntry.Offset, Win32ResourceReader.DataEntrySize)
            Dim dataRva As Integer = BitConverter.ToInt32(data, languageEntry.Offset)
            Dim dataSize As Integer = BitConverter.ToInt32(data, languageEntry.Offset + 4)
            If dataSize < 0 Then
                Throw New BadImageFormatException("The resource data size is invalid.")
            End If

            Dim content As ImmutableArray(Of Byte) = peReader.GetSectionData(dataRva).GetContent()
            If content.Length < dataSize Then
                Throw New BadImageFormatException("The resource data points outside of the image sections.")
            End If

            resourceId = nameEntry.Id
            Return content.Take(dataSize).ToArray()
        End Function

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Finds an entry of a resource directory.
        ''' </summary>
        ''' <param name="data">
        ''' The resource section data.
        ''' </param>
        '''
        ''' <param name="directoryOffset">
        ''' The offset of the <c>IMAGE_RESOURCE_DIRECTORY</c>.
        ''' </param>
        '''
        ''' <param name="id">
        ''' The integer ID to find, or <see langword="Nothing"/> to find the integer entry with the lowest ID
        ''' (or the first named entry, if the directory has no integer entries).
        ''' </param>
        '''
        ''' <param name="entry">
        ''' When this method returns, contains the entry that was found.
        ''' </param>
        ''' <returns>
        ''' <see langword="True"/> if an entry was found; otherwise, <see langword="False"/>.
        ''' </returns>
        Private Function TryFindEntry(data As Byte(), directoryOffset As Integer, id As Integer?, ByRef entry As DirectoryEntry) As Boolean

            Win32ResourceReader.EnsureRange(data, directoryOffset, Win32ResourceReader.DirectorySize)
            Dim namedCount As Integer = BitConverter.ToUInt16(data, directoryOffset + 12)
            Dim idCount As Integer = BitConverter.ToUInt16(data, directoryOffset + 14)
            Dim entriesOffset As Integer = directoryOffset + Win32ResourceReader.DirectorySize

            Dim found As Boolean = False
            For i As Integer = 0 To namedCount + idCount - 1
                Dim offset As Integer = entriesOffset + (i * Win32ResourceReader.DirectoryEntrySize)
                Win32ResourceReader.EnsureRange(data, offset, Win32ResourceReader.DirectoryEntrySize)

                Dim nameField As UInteger = BitConverter.ToUInt32(data, offset)
                Dim offsetField As UInteger = BitConverter.ToUInt32(data, offset + 4)
                Dim candidate As New DirectoryEntry With {
                    .IsNamed = (nameField And Win32ResourceReader.HighBit) <> 0,
                    .Id = CInt(nameField And &HFFFFUI),
                    .IsDirectory = (offsetField And Win32ResourceReader.HighBit) <> 0,
                    .Offset = CInt(offsetField And Not Win32ResourceReader.HighBit)
                }

                If id.HasValue Then
                    If Not candidate.IsNamed AndAlso candidate.Id = id.Value Then
                        entry = candidate
                        Return True
                    End If
                ElseIf Not found OrElse (entry.IsNamed AndAlso Not candidate.IsNamed) OrElse
                       (Not entry.IsNamed AndAlso Not candidate.IsNamed AndAlso candidate.Id < entry.Id) Then
                    entry = candidate
                    found = True
                End If
            Next i

            Return found
        End Function

        ''' <summary>
        ''' Throws a <see cref="BadImageFormatException"/> if the specified range is outside of the data.
        ''' </summary>
        ''' <param name="data">
        ''' The data.
        ''' </param>
        '''
        ''' <param name="offset">
        ''' The start of the range.
        ''' </param>
        '''
        ''' <param name="length">
        ''' The length of the range.
        ''' </param>
        Private Sub EnsureRange(data As Byte(), offset As Integer, length As Integer)

            If (offset < 0) OrElse (CLng(offset) + length > data.Length) Then
                Throw New BadImageFormatException("The resource directory is truncated or corrupted.")
            End If
        End Sub

#End Region

    End Module

End Namespace

#End Region
