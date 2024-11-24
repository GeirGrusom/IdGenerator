using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace IxSoftware.Generators
{
    internal record StructInfo
    (
        StructDeclarationSyntax Struct,
        string TypeName,
        SyntaxToken Identifier,
        SyntaxToken Value,
        TypeSyntax ValueType,
        IsComparable Comparable,
        Validation Validation,
        bool HasToString,
        bool IsFieldReferenceType,
        INamedTypeSymbol DeclaredSymbol,
        ITypeSymbol TypeInfo,
        bool IsNullableEnabled
    );
}
