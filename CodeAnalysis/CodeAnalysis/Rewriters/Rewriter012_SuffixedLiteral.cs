using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Adds the literal suffix of the declared type to the numeric constants of a document and replaces
    ///     every constant conversion helper with the suffixed literal of its target type.
    /// </summary>
    /// <remarks>
    ///     The suffix of a constant is stated by its declaration, so the constant part of the rewriter works
    ///     without a semantic model and can be applied by the formatting pipeline. The helper part needs the
    ///     semantic model that resolves the invoked method, so it is only available when the model is supplied.
    ///     An invocation that is replaced has a literal argument, so replacing it does not need the rewritten
    ///     children.
    /// </remarks>
    internal sealed class Rewriter012_SuffixedLiteral : CSharpSyntaxRewriter
    {
        /// <summary>
        ///     The semantic model that resolves the invoked conversion helpers of the original tree,
        ///     or <see langword="null" /> when only the constant declarations are rewritten.
        /// </summary>
        private readonly SemanticModel? _semanticModel;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Rewriter012_SuffixedLiteral" /> class
        ///     that only rewrites constant declarations.
        /// </summary>
        public Rewriter012_SuffixedLiteral()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Rewriter012_SuffixedLiteral" /> class.
        /// </summary>
        /// <param name="semanticModel">The semantic model of the document to rewrite.</param>
        public Rewriter012_SuffixedLiteral(SemanticModel semanticModel) => _semanticModel = semanticModel;

        /// <summary>
        ///     Rewrites a constant conversion helper call into a suffixed literal.
        /// </summary>
        /// <param name="node">The invocation to rewrite.</param>
        /// <returns>The suffixed literal, or the invocation visited by the base implementation.</returns>
        public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node) => _semanticModel != null && LiteralFacts.TryGetReplacement(node, _semanticModel, out var replacement) ? replacement.WithTriviaFrom(node) : base.VisitInvocationExpression(node);

        /// <summary>
        ///     Adds the literal suffix of the declared type to the constants of a field declaration.
        /// </summary>
        /// <param name="node">The field declaration to rewrite.</param>
        /// <returns>The field declaration whose constant initializers carry their suffix.</returns>
        public override SyntaxNode? VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            if (base.VisitFieldDeclaration(node) is not FieldDeclarationSyntax declaration)
                return null;

            var suffix = GetConstantSuffix(node.Modifiers, node.Declaration.Type);
            return suffix == null ? declaration : declaration.WithDeclaration(RewriteConstants(declaration.Declaration, suffix));
        }

        /// <summary>
        ///     Adds the literal suffix of the declared type to the constants of a local declaration.
        /// </summary>
        /// <param name="node">The local declaration to rewrite.</param>
        /// <returns>The local declaration whose constant initializers carry their suffix.</returns>
        public override SyntaxNode? VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            if (base.VisitLocalDeclarationStatement(node) is not LocalDeclarationStatementSyntax statement)
                return null;

            var suffix = GetConstantSuffix(node.Modifiers, node.Declaration.Type);
            return suffix == null ? statement : statement.WithDeclaration(RewriteConstants(statement.Declaration, suffix));
        }

        /// <summary>
        ///     Gets the literal suffix of the declared type of a constant declaration.
        /// </summary>
        /// <param name="modifiers">The modifiers of the declaration.</param>
        /// <param name="type">The declared type.</param>
        /// <returns>
        ///     The suffix of the declared type, or <see langword="null" /> when the declaration is not constant or has no
        ///     suffix.
        /// </returns>
        private string? GetConstantSuffix(SyntaxTokenList modifiers, TypeSyntax type)
        {
            if (!modifiers.Any(SyntaxKind.ConstKeyword))
                return null;

            if (type is PredefinedTypeSyntax predefined)
                return LiteralFacts.GetLiteralSuffix(predefined.Keyword.ValueText);

            var resolvedType = _semanticModel?.GetTypeInfo(type).Type;
            return resolvedType == null ? null : LiteralFacts.GetLiteralSuffix(resolvedType.SpecialType);
        }

        /// <summary>
        ///     Appends the suffix to the initializer of every constant declarator.
        /// </summary>
        /// <param name="declaration">The declaration whose initializers are rewritten.</param>
        /// <param name="suffix">The suffix of the declared type.</param>
        /// <returns>The declaration with the suffixed initializers.</returns>
        private static VariableDeclarationSyntax RewriteConstants(VariableDeclarationSyntax declaration, string suffix)
        {
            var variables = declaration.Variables;
            var changed = false;
            for (var i = 0; i < variables.Count; i++)
            {
                var variable = variables[i];
                if (variable.Initializer == null || !LiteralFacts.TryAppendLiteralSuffix(variable.Initializer.Value, suffix, out var value))
                    continue;

                variables = variables.Replace(variable, variable.WithInitializer(variable.Initializer.WithValue(value)));
                changed = true;
            }

            return changed ? declaration.WithVariables(variables) : declaration;
        }
    }
}