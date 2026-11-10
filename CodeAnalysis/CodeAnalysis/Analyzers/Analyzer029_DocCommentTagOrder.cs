using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Requires the top-level tags of a documentation comment to end with
    ///     <c>&lt;seealso /&gt;</c> directly above <c>&lt;remarks&gt;</c> and <c>&lt;remarks&gt;</c> last.
    /// </summary>
    /// <remarks>
    ///     The library reads a documentation comment from the required description down to the
    ///     optional detail: <c>&lt;remarks&gt;</c> closes the comment, and <c>&lt;seealso /&gt;</c>
    ///     links sit directly above it. A comment without <c>&lt;remarks&gt;</c> ends with
    ///     <c>&lt;seealso /&gt;</c>. A tag placed below either of the two breaks that reading
    ///     order, so the analyzer reports the misplaced element.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer029_DocCommentTagOrder : DiagnosticAnalyzer
    {
        /// <summary>
        ///     The prefix shared by the two rules of this analyzer.
        /// </summary>
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "029";

        /// <summary>
        ///     The title of <c>DESIGN029_1</c>.
        /// </summary>
        private static readonly LocalizableString Title001 = "Remarks is not the last tag";

        /// <summary>
        ///     The message of <c>DESIGN029_1</c>.
        /// </summary>
        private static readonly LocalizableString MessageFormat001 = "Move <remarks> to the bottom of the documentation comment";

        /// <summary>
        ///     Flags a documentation comment whose <c>&lt;remarks&gt;</c> element is not the last top-level element.
        /// </summary>
        private static readonly DiagnosticDescriptor Rule001 = new(DIAGNOSTIC_ID + "_1", Title001, MessageFormat001, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        /// <summary>
        ///     The title of <c>DESIGN029_2</c>.
        /// </summary>
        private static readonly LocalizableString Title002 = "Seealso is not directly above remarks";

        /// <summary>
        ///     The message of <c>DESIGN029_2</c>.
        /// </summary>
        private static readonly LocalizableString MessageFormat002 = "Move <seealso> below the other tags, directly above <remarks>";

        /// <summary>
        ///     Flags a documentation comment whose <c>&lt;seealso&gt;</c> elements are not grouped
        ///     at the bottom, directly above <c>&lt;remarks&gt;</c>.
        /// </summary>
        private static readonly DiagnosticDescriptor Rule002 = new(DIAGNOSTIC_ID + "_2", Title002, MessageFormat002, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        /// <summary>
        ///     Gets the two rules reported by this analyzer.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule001, Rule002];

        /// <summary>
        ///     Registers the syntax tree callback.
        /// </summary>
        /// <param name="context">The analysis context of the compilation.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxTreeAction(AnalyzeDocumentationComment);
        }

        /// <summary>
        ///     Checks the top-level tag order of every documentation comment of the syntax tree.
        /// </summary>
        /// <param name="context">The syntax context of the tree.</param>
        /// <remarks>
        ///     The check runs on the trivia structure, so it does not depend on the semantic model
        ///     and applies to every documentation comment of the file regardless of the kind of
        ///     declaration that carries it.
        /// </remarks>
        private static void AnalyzeDocumentationComment(SyntaxTreeAnalysisContext context)
        {
            foreach (var trivia in context.Tree.GetRoot().DescendantTrivia())
            {
                if (trivia.GetStructure() is not DocumentationCommentTriviaSyntax documentation)
                    continue;

                var names = new List<string>();
                var elements = new List<XmlNodeSyntax>();
                foreach (var node in documentation.Content)
                {
                    if (node is XmlElementSyntax element)
                    {
                        names.Add(element.StartTag.Name.LocalName.ValueText);
                        elements.Add(element);
                    }
                    else if (node is XmlEmptyElementSyntax empty)
                    {
                        names.Add(empty.Name.LocalName.ValueText);
                        elements.Add(empty);
                    }
                }

                if (names.Count == 0)
                    continue;

                var remarksIndex = names.IndexOf("remarks");
                if (remarksIndex >= 0 && remarksIndex != names.Count - 1)
                    context.ReportDiagnostic(Diagnostic.Create(Rule001, elements[remarksIndex].GetLocation()));

                var seealsoIndex = names.IndexOf("seealso");
                if (seealsoIndex < 0)
                    continue;

                for (var index = seealsoIndex + 1; index < names.Count; index++)
                {
                    if (names[index] == "seealso")
                        continue;

                    if (index == names.Count - 1 && names[index] == "remarks")
                        break;

                    context.ReportDiagnostic(Diagnostic.Create(Rule002, elements[seealsoIndex].GetLocation()));
                    break;
                }
            }
        }
    }
}