using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace IxSoftware.Generators
{
    internal static class SyntaxNodeHelpers
    {
        public static bool IsPartial(this TypeDeclarationSyntax node) => node.Modifiers.Any(PartialKeyword);

        public static bool IsStatic(this MemberDeclarationSyntax node) => node.Modifiers.Any(StaticKeyword);

        public static bool IsReadOnly(this MemberDeclarationSyntax node) => node.Modifiers.Any(ReadOnlyKeyword);

        public static bool IsPublic(this MemberDeclarationSyntax node) => node.Modifiers.Any(PublicKeyword);

        public static bool IsOverride(this MemberDeclarationSyntax node) => node.Modifiers.Any(OverrideKeyword);

        public static bool HasEquatable(this BaseListSyntax node, SyntaxToken equatableType)
        {
            return node.Types.Select(x => x.Type).OfType<GenericNameSyntax>().Any(x => string.Equals(x.Identifier.ValueText, nameof(IEquatable<int>), StringComparison.Ordinal) && x.TypeArgumentList.Arguments.Any(x => string.Equals(x.ToString(), equatableType.ToString(), StringComparison.Ordinal)));
        }
    }
}
