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
    [Generator]
    public sealed class IdGenerator : ISourceGenerator
    {
        private record StructInfo(StructDeclarationSyntax Struct, SyntaxToken Identifier, SyntaxToken Value, TypeSyntax ValueType, bool HasToString);

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
                var toStringMethod = s.Members.OfType<MethodDeclarationSyntax>().Where(m => m.IsOverride() && string.Equals(m.Identifier.Text, nameof(object.ToString), StringComparison.Ordinal) && !m.ParameterList.Parameters.Any()).SingleOrDefault();

                var f = fields[0];
                Nodes.Add(new StructInfo(s, s.Identifier, f.Declaration.Variables[0].Identifier, f.Declaration.Type, toStringMethod != null));
            }
        }

        private static string? MakeTypeName(TypeSyntax type, SemanticModel semanticModel)
        {
            if (type is TupleTypeSyntax tupleType)
            {
                return MakeTupleTypeName(semanticModel, tupleType);
            }
            else
            {
                var typeSymbol = semanticModel.GetSymbolInfo(type);
                if(typeSymbol.Symbol is null)
                {
                    return null;
                }
                return $"global::{typeSymbol.Symbol.ContainingNamespace}.{typeSymbol.Symbol.Name}";
            }
        }

        private static string MakeTupleTypeName(SemanticModel semanticModel, TupleTypeSyntax tupleType)
        {
            var builder = new StringBuilder(tupleType.Span.Length);
            builder.Append('(');
            for (int i = 0; i < tupleType.Elements.Count; i++)
            {
                var item = tupleType.Elements[i];
                builder.Append(MakeTypeName(item.Type, semanticModel));
                if (!item.Identifier.IsMissing)
                {
                    builder.Append(' ');
                    builder.Append(item.Identifier.ValueText);
                }
                if (i < tupleType.Elements.Count - 1)
                {
                    builder.Append(", ");
                }
            }
            builder.Append(')');
            return builder.ToString();
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var syntaxReceiver = (StructSyntaxReceiver)(context.SyntaxReceiver ?? throw new InvalidOperationException("Missing syntax receiver!"));
            if(syntaxReceiver is null)
            {
                return;
            }

            var result = new StringBuilder(512);

            foreach (var s in syntaxReceiver.Nodes)
            {
                var tree = s.Identifier.SyntaxTree;
                if(tree is null)
                {
                    continue;
                }

                var semanticModel = context.Compilation.GetSemanticModel(tree);
                string name = s.Identifier.ToString();
                
                var symbol = semanticModel.GetDeclaredSymbol(s.Struct);
                string? typeName = MakeTypeName(s.ValueType, semanticModel);
                if(typeName is null || symbol is null)
                {
                    continue;
                }

                result.Clear();

                var isString = string.Equals(typeName, "global::System.String", StringComparison.Ordinal);

                string text = @$"
namespace {symbol.ContainingNamespace}
{{  
    public readonly partial struct {s.Identifier} : global::System.IEquatable<{s.Identifier}>
    {{

        private {s.Identifier}({typeName} value)
        {{
            this.{s.Value} = value;
        }}
";
                result.Append(text);

                if(isString)
                {
                    result.AppendLine(@$"        public bool Equals({s.Identifier} other) => this.{s.Value}.Equals(other.{s.Value}, global::System.StringComparison.Ordinal);");
                }
                else
                {
                    result.AppendLine($@"        public bool Equals({s.Identifier} other) => this.{s.Value}.Equals(other.{s.Value});");
                }

                text = 
        @$"

        public override bool Equals(object obj)
        {{
            if(obj is {s.Identifier} other)
            {{
                return this.Equals(other);
            }}

            return false;
        }}
        
        public override int GetHashCode() => this.{s.Value}.GetHashCode();

        public static explicit operator {typeName}({name} value) => value.{s.Value};

        public static implicit operator {s.Identifier}({typeName} value) => new {s.Identifier}(value);

        public static bool operator ==({name} lhs, {name} rhs) => lhs.Equals(rhs);

        public static bool operator !=({name} lhs, {name} rhs) => !lhs.Equals(rhs);";
                result.AppendLine(text);

                if(!s.HasToString)
                {
                    result.AppendLine(@$"
        public override string ToString() => ((global::System.FormattableString)$""{{{s.Value}}}"").ToString(global::System.Globalization.CultureInfo.InvariantCulture);");
                }

                result.AppendLine(@"
    }
}");

                context.AddSource(@$"id-{s.Identifier}.cs", SourceText.From(result.ToString(), Encoding.UTF8));
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new StructSyntaxReceiver());
        }
    }
}
