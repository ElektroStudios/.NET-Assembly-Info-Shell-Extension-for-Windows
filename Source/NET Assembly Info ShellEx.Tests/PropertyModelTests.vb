
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Reflection.Metadata

Imports AssemblyInspection

Imports Microsoft.VisualStudio.TestTools.UnitTesting

#End Region

#Region " PropertyModelTests "

''' <summary>
''' Tests <see cref="PropertyRow"/>, <see cref="PropertyGroup"/> and <see cref="AttributeValueTypeProvider"/>.
''' </summary>
<TestClass>
Public NotInheritable Class PropertyModelTests

#Region " PropertyRow "

    <TestMethod>
    Public Sub PropertyRow_ValidArguments_AreExposed()
        Dim row As New PropertyRow("Name", "Value")

        Assert.AreEqual("Name", row.Name)
        Assert.AreEqual("Value", row.Value)
    End Sub

    <TestMethod>
    Public Sub PropertyRow_EmptyValue_IsAllowed()
        Dim row As New PropertyRow("Name", String.Empty)

        Assert.AreEqual(String.Empty, row.Value)
    End Sub

    <TestMethod>
    Public Sub PropertyRow_InvalidArguments_ThrowArgumentNullException()
        Assert.ThrowsExactly(Of ArgumentNullException)(
            Sub()
                Dim unused As New PropertyRow(Nothing, "Value")
            End Sub)
        Assert.ThrowsExactly(Of ArgumentNullException)(
            Sub()
                Dim unused As New PropertyRow(String.Empty, "Value")
            End Sub)
        Assert.ThrowsExactly(Of ArgumentNullException)(
            Sub()
                Dim unused As New PropertyRow("Name", Nothing)
            End Sub)
    End Sub

#End Region

#Region " PropertyGroup "

    <TestMethod>
    Public Sub PropertyGroup_GetValue_ReturnsTheRowValue()
        Dim group As New PropertyGroup("Group", {New PropertyRow("A", "1"), New PropertyRow("B", "2")})

        Assert.AreEqual("2", group.GetValue("B"))
        Assert.IsTrue(group.ContainsRow("A"))
        Assert.IsFalse(group.ContainsRow("a"))
    End Sub

    <TestMethod>
    Public Sub PropertyGroup_GetValue_MissingRow_ThrowsKeyNotFoundException()
        Dim group As New PropertyGroup("Group", {New PropertyRow("A", "1")})

        Assert.ThrowsExactly(Of KeyNotFoundException)(Sub() group.GetValue("Z"))
    End Sub

    <TestMethod>
    Public Sub PropertyGroup_RowsAreACopy()
        Dim rows As New List(Of PropertyRow) From {New PropertyRow("A", "1")}
        Dim group As New PropertyGroup("Group", rows)

        rows.Add(New PropertyRow("B", "2"))

        Assert.AreEqual(1, group.Rows.Count)
    End Sub

    <TestMethod>
    Public Sub PropertyGroup_InvalidArguments_ThrowArgumentNullException()
        Assert.ThrowsExactly(Of ArgumentNullException)(
            Sub()
                Dim unused As New PropertyGroup(Nothing, Enumerable.Empty(Of PropertyRow)())
            End Sub)
        Assert.ThrowsExactly(Of ArgumentNullException)(
            Sub()
                Dim unused As New PropertyGroup("Group", Nothing)
            End Sub)
    End Sub

#End Region

#Region " AttributeValueTypeProvider "

    <TestMethod>
    <DataRow("System.Runtime.InteropServices.DllImportSearchPath")>
    <DataRow("System.Diagnostics.DebuggableAttribute+DebuggingModes")>
    <DataRow("System.Security.Permissions.SecurityAction, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")>
    Public Sub AttributeValueTypeProvider_KnownEnum_IsInt32(typeName As String)
        Dim provider As New AttributeValueTypeProvider()

        Assert.AreEqual(PrimitiveTypeCode.Int32, provider.GetUnderlyingEnumType(typeName))
    End Sub

    <TestMethod>
    Public Sub AttributeValueTypeProvider_UnknownEnum_ThrowsNotSupportedException()
        Dim provider As New AttributeValueTypeProvider()

        Assert.ThrowsExactly(Of NotSupportedException)(Sub() provider.GetUnderlyingEnumType("Contoso.UnknownByteEnum"))
    End Sub

    <TestMethod>
    Public Sub AttributeValueTypeProvider_SystemType_IsRecognized()
        Dim provider As New AttributeValueTypeProvider()

        Assert.IsTrue(provider.IsSystemType(provider.GetSystemType()))
        Assert.IsFalse(provider.IsSystemType("System.String"))
        Assert.AreEqual("System.Int32[]", provider.GetSZArrayType("System.Int32"))
        Assert.AreEqual("Int32", provider.GetPrimitiveType(PrimitiveTypeCode.Int32))
    End Sub

#End Region

End Class

#End Region
