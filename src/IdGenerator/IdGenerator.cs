using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Runtime.CompilerServices
{
    public sealed class IsExternalInit { }
}

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
    public sealed class IdGenerator : ISourceGenerator
    {
        private class StructSyntaxReceiver : ISyntaxReceiver
        {
            public List<StructInfo> Nodes { get; } = new List<StructInfo>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if(!syntaxNode.IsKind(SyntaxKind.StructDeclaration))
                {
                    return;
                }
                var s = (StructDeclarationSyntax)syntaxNode;
                    
                // Struct must be partial.
                if(!s.IsPartial())
                {
                    return;
                }

                // Struct must have IEquatable<T> where T is the type of the struct.
                if(s.BaseList is null || !s.BaseList.HasEquatable(s.Identifier))
                {
                    return;
                }

                var isComparable = s.BaseList.HasComparable(s.Identifier) ? IsComparable.Comparable : IsComparable.NonComparable;
                    
                var fields = s.Members.OfType<FieldDeclarationSyntax>().Where(f => !f.IsStatic()).ToArray();

                // Struct must have a single non-static readonly field.
                if (fields.Length != 1)
                {
                    return;
                }

                var field = fields[0];

                // Field must be read only.
                if(!field.IsReadOnly())
                {
                    return;
                }

                // Find out if struct has a ToString() method already defined.
                var hasToString = s.HasToStringMethod();

                var validationMethod = s.HasValidateMethod() ? Validation.ValidateBool : Validation.NoValidation;

                var f = fields[0];
                Nodes.Add(new StructInfo(s, s.Identifier, f.Declaration.Variables[0].Identifier, f.Declaration.Type, isComparable, validationMethod, hasToString));
            }
        }

        
        public void Execute(GeneratorExecutionContext context)
        {
            var syntaxReceiver = (StructSyntaxReceiver)(context.SyntaxReceiver ?? throw new InvalidOperationException("Missing syntax receiver!"));
            if(syntaxReceiver is null)
            {
                return;
            }

            bool isNullableEnabled = context.Compilation.Options.NullableContextOptions != NullableContextOptions.Disable;

            var result = new StringBuilder(512);

            foreach (var s in syntaxReceiver.Nodes)
            {
                var tree = s.Identifier.SyntaxTree;
                if(tree is null)
                {
                    continue;
                }

                var semanticModel = context.Compilation.GetSemanticModel(tree);
                
                var symbol = semanticModel.GetDeclaredSymbol(s.Struct);
                string? typeName = CodeBuilder.MakeTypeName(s.ValueType, semanticModel);
                var typeSymbol = semanticModel.GetTypeInfo(s.ValueType).Type;

                bool isReferenceType;
                if (typeSymbol is not null)
                {
                    isReferenceType = typeSymbol.IsReferenceType;
                }
                else
                {
                    isReferenceType = false;
                }

                if(typeName is null || symbol is null)
                {
                    continue;
                }

                result.Clear();

                result.AddPreamble(symbol, s);

                result.AddConstructor(typeName, s);

                result.AddEquals(typeName, isNullableEnabled, isReferenceType, s);

                if(!s.HasToString)
                {
                    result.AddToString(typeName, s);
                }

                if(s.Comparable == IsComparable.Comparable)
                {
                    result.AddComparison(s);
                }

                result.AddEndOfFile();

                var filename = @$"id-{s.Identifier}.cs";
                context.AddSource(filename, SourceText.From(result.ToString(), Encoding.UTF8));
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new StructSyntaxReceiver());
        }
    }
}
