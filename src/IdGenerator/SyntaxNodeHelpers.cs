using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace IxSoftware.Generators
{
    internal static class SyntaxNodeHelpers
    {
        public static bool IsPartial(this TypeDeclarationSyntax node)
        {
            return node.Modifiers.Any(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword);
        }

        public static bool IsStatic(this MemberDeclarationSyntax node)
        {
            return node.Modifiers.Any(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StaticKeyword);
        }

        public static bool IsReadOnly(this MemberDeclarationSyntax node)
        {
            return node.Modifiers.Any(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ReadOnlyKeyword);
        }

        public static bool IsPublic(this MemberDeclarationSyntax node)
        {
            return node.Modifiers.Any(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword);
        }

        public static bool IsPrivate(this MemberDeclarationSyntax node)
        {
            return node.Modifiers.Any(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword) || !IsPublic(node);
        }

        public static bool IsOverride(this MemberDeclarationSyntax node)
        {
            return node.Modifiers.Any(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OverrideKeyword);
        }
    }
}
