using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IxSoftware.Generators
{
    [Generator]
    public sealed class IdGenerator : ISourceGenerator
    {
        private class StructInfo
        {
            public StructInfo(StructDeclarationSyntax @struct, SyntaxToken identifier, SyntaxToken value, TypeSyntax valueType, bool hasToString)
            {
                this.Struct = @struct;
                Identifier = identifier;
                Value = value;
                ValueType = valueType;
                HasToString = hasToString;
            }

            public StructDeclarationSyntax Struct { get; }

            public SyntaxToken Identifier { get; }

            public SyntaxToken Value { get; }

            public TypeSyntax ValueType { get; }

            public bool HasToString { get; }
        }

        private class StructSyntaxReceiver : ISyntaxReceiver
        {
            public List<StructInfo> Nodes { get; } = new List<StructInfo>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if(syntaxNode.IsKind(SyntaxKind.StructDeclaration))
                {
                    var s = (StructDeclarationSyntax)syntaxNode;
                    
                    // Struct must be partial
                    if(!s.IsPartial())
                    {
                        return;
                    }

                    // Struct must have IEquatable<T> where T is the type of the struct
                    if(s.BaseList is null || !s.BaseList.Types.Select(x => x.Type).OfType<GenericNameSyntax>().Any(x => x.Identifier.ValueText == nameof(IEquatable<int>) && x.TypeArgumentList.Arguments.Any(x => x.ToString() == s.Identifier.ToString())))
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

                    if(!field.IsReadOnly())
                    {
                        return;
                    }

                    // Find out if struct has a ToString() method already defined.
                    var toStringMethod = s.Members.OfType<MethodDeclarationSyntax>().Where(m => m.IsOverride() && m.Identifier.Text == "ToString" && !m.ParameterList.Parameters.Any()).SingleOrDefault();

                    var f = fields[0];
                    Nodes.Add(new StructInfo(s, s.Identifier, f.Declaration.Variables[0].Identifier, f.Declaration.Type, toStringMethod != null));
                }
            }
        }

        private string MakeTypeName(TypeSyntax type, SemanticModel semanticModel)
        {
            if (type is TupleTypeSyntax tupleType)
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
                    if(i < tupleType.Elements.Count - 1)
                    {
                        builder.Append(", ");
                    }
                }
                builder.Append(')');
                return builder.ToString();
            }
            else
            {
                var typeSymbol = semanticModel.GetSymbolInfo(type);
                return $"global::{typeSymbol.Symbol.ContainingNamespace}.{typeSymbol.Symbol.Name}";
            }
        }

        public void Execute(SourceGeneratorContext context)
        {
            var syntaxReceiver = (StructSyntaxReceiver)context.SyntaxReceiver;

            foreach(var s in syntaxReceiver.Nodes)
            {
                var semanticModel = context.Compilation.GetSemanticModel(s.Identifier.SyntaxTree);
                string name = s.Identifier.ToString();
                
                var symbol = semanticModel.GetDeclaredSymbol(s.Struct);

                string typeName = MakeTypeName(s.ValueType, semanticModel);
                var result = new StringBuilder(512);
                string text = @$"
namespace {symbol.ContainingNamespace}
{{  
    public readonly partial struct {s.Identifier} : System.IEquatable<{s.Identifier}>
    {{

        private {s.Identifier}({typeName} value)
        {{
            this.{s.Value} = value;
        }}

        public bool Equals({s.Identifier} other) => this.{s.Value}.Equals(other.{s.Value});

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
                result.Append(text);

                if(!s.HasToString)
                {
                    result.AppendLine(@$"
        public override string ToString() => ((System.FormattableString)$""{{{s.Value}}}"").ToString(System.Globalization.CultureInfo.InvariantCulture);");
                }

                result.AppendLine(@"
    }
}");

                context.AddSource(@$"id-{s.Identifier}.cs", SourceText.From(result.ToString(), Encoding.UTF8));
            }
        }

        public void Initialize(InitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new StructSyntaxReceiver());
        }
    }
}
