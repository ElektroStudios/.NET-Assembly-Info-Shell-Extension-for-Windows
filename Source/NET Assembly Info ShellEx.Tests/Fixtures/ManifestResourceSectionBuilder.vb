
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Reflection.Metadata
Imports System.Reflection.PortableExecutable

#End Region

#Region " ManifestResourceSectionBuilder "

Namespace Fixtures

    ''' <summary>
    ''' Emits a Win32 resource section (.rsrc) with a single resource: type, ID and language directories,
    ''' one data entry, and the resource data.
    ''' </summary>
    Friend NotInheritable Class ManifestResourceSectionBuilder : Inherits ResourceSectionBuilder

#Region " Private Fields "

        ''' <summary>The resource type (24 = RT_MANIFEST).</summary>
        Private ReadOnly resourceType As Integer

        ''' <summary>The resource integer ID.</summary>
        Private ReadOnly resourceId As Integer

        ''' <summary>The resource data.</summary>
        Private ReadOnly data As Byte()

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ManifestResourceSectionBuilder"/> class.
        ''' </summary>
        ''' <param name="resourceType">The resource type (24 = RT_MANIFEST).</param>
        ''' <param name="resourceId">The resource integer ID.</param>
        ''' <param name="data">The resource data.</param>
        Friend Sub New(resourceType As Integer, resourceId As Integer, data As Byte())
            Me.resourceType = resourceType
            Me.resourceId = resourceId
            Me.data = data
        End Sub

#End Region

#Region " ResourceSectionBuilder Implementation "

        ''' <summary>
        ''' Serializes the resource section.
        ''' </summary>
        ''' <param name="builder">The builder.</param>
        ''' <param name="location">The location of the section.</param>
        Protected Overrides Sub Serialize(builder As BlobBuilder, location As SectionLocation)

            Const SubdirectoryFlag As UInteger = &H80000000UI

            ' Level 1 (offset 0): resource type -> level 2 at offset 24.
            ManifestResourceSectionBuilder.WriteDirectory(builder)
            builder.WriteUInt32(CUInt(Me.resourceType))
            builder.WriteUInt32(SubdirectoryFlag Or 24UI)

            ' Level 2 (offset 24): resource ID -> level 3 at offset 48.
            ManifestResourceSectionBuilder.WriteDirectory(builder)
            builder.WriteUInt32(CUInt(Me.resourceId))
            builder.WriteUInt32(SubdirectoryFlag Or 48UI)

            ' Level 3 (offset 48): language (en-US) -> data entry at offset 72.
            ManifestResourceSectionBuilder.WriteDirectory(builder)
            builder.WriteUInt32(1033UI)
            builder.WriteUInt32(72UI)

            ' Data entry (offset 72): RVA, size, code page, reserved -> data at offset 88.
            builder.WriteUInt32(CUInt(location.RelativeVirtualAddress + 88))
            builder.WriteUInt32(CUInt(Me.data.Length))
            builder.WriteUInt32(0UI)
            builder.WriteUInt32(0UI)

            builder.WriteBytes(Me.data)
        End Sub

#End Region

#Region " Private Methods "

        ''' <summary>Writes an IMAGE_RESOURCE_DIRECTORY with a single integer-ID entry.</summary>
        Private Shared Sub WriteDirectory(builder As BlobBuilder)
            builder.WriteUInt32(0UI) ' Characteristics
            builder.WriteUInt32(0UI) ' TimeDateStamp
            builder.WriteUInt16(0US) ' MajorVersion
            builder.WriteUInt16(0US) ' MinorVersion
            builder.WriteUInt16(0US) ' NumberOfNamedEntries
            builder.WriteUInt16(1US) ' NumberOfIdEntries
        End Sub

#End Region

    End Class

End Namespace

#End Region
