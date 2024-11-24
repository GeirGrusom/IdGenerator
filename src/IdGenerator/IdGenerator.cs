using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Text;
using System.Threading;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Runtime.CompilerServices
{
    public sealed class IsExternalInit { }
}
#pragma warning restore IDE0130 // Namespace does not match folder structure

namespace IxSoftware.Generators
{
    internal enum IsComparable
    {
        Comparable,
        NonComparable
    }

    internal enum Validation
    {
        NoValidation,
        ValidateBool
    }

    [Generator]
    public sealed class IdGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var provider = context.SyntaxProvider.CreateSyntaxProvider(IsIdentityType, Transform);
            context.RegisterSourceOutput(provider, GenerateSource);
            
        }

        private void GenerateSource(SourceProductionContext context, StructInfo info)
        {
            var tree = info.Identifier.SyntaxTree;
            if (tree is null)
            {
                return;
            }

            var result = new StringBuilder();

            result.AddPreamble(info);
            result.AddConstructor(info);

            result.AddEquals(info);

            if (!info.HasToString)
            {
                result.AddToString(info);
            }

            if (info.Comparable == IsComparable.Comparable)
            {
                result.AddComparison(info);
            }

            result.AddEndOfFile();

            var filename = @$"id-{info.Identifier}.cs";

            context.AddSource(filename, result.ToString());
        }

        internal StructInfo Transform(GeneratorSyntaxContext context, CancellationToken cancellationToken)
        {
            var s = (StructDeclarationSyntax)context.Node;
            var fields = s.Members.OfType<FieldDeclarationSyntax>().Where(f => !f.IsStatic()).ToArray();

            var field = fields[0];

            var isComparable = s.BaseList?.HasComparable(s.Identifier) == true ? IsComparable.Comparable : IsComparable.NonComparable;

            // Find out if struct has a ToString() method already defined.
            var hasToString = s.HasToStringMethod();

            var validationMethod = s.HasValidateMethod() ? Validation.ValidateBool : Validation.NoValidation;

            var f = fields[0];
            var valueType = f.Declaration.Type;

            var fieldIdentifier = f.Declaration.Variables[0].Identifier;

            var symbol = context.SemanticModel.GetDeclaredSymbol(s)!;
            string? typeName = CodeBuilder.MakeTypeName(valueType, context.SemanticModel);
            var typeSymbol = context.SemanticModel.GetTypeInfo(valueType).Type;
            
            bool isReferenceType;
            if (typeSymbol is not null)
            {
                isReferenceType = typeSymbol.IsReferenceType;
            }
            else
            {
                isReferenceType = false;
            }

            bool nullable = context.SemanticModel.GetNullableContext(s.Span.Start)
                is not NullableContext.Disabled;

            return new StructInfo(s, typeName!, s.Identifier, fieldIdentifier, valueType, isComparable, validationMethod, hasToString, isReferenceType, symbol, typeSymbol!, nullable);
        }

        internal bool IsIdentityType(SyntaxNode node, CancellationToken cancellationToken)
        {
            if (!node.IsKind(SyntaxKind.StructDeclaration))
            {
                return false;
            }
            var s = (StructDeclarationSyntax)node;

            // Struct must be partial.
            if (!s.IsPartial())
            {
                return false;
            }

            // Struct must have IEquatable<T> where T is the type of the struct.
            if (s.BaseList is not { } b || !b.HasEquatable(s.Identifier))
            {
                return false;
            }

            var fields = s.Members.OfType<FieldDeclarationSyntax>().Where(f => !f.IsStatic() && f.IsReadOnly()).ToArray();

            // Struct must have a single non-static readonly field.
            if (fields.Length != 1)
            {
                return false;
            }

            return true;
        }
    }
}
