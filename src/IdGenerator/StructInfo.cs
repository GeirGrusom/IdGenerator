using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace IxSoftware.Generators
{
    internal record StructInfo(StructDeclarationSyntax Struct, SyntaxToken Identifier, SyntaxToken Value, TypeSyntax ValueType, IsComparable Comparable, Validation Validation, bool HasToString);
}
