using System.Collections.Generic;
using System.Linq;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.IL;

namespace RediSharp.Decompilation
{
    public static class SyntaxTreeExtensions
    {
        #region Defaults

        private static readonly IList<string> _EmptyStringArray = new string[0];

        #endregion

        public static MethodDeclaration FirstMethodOrDefault(this SyntaxTree tree) =>
            tree.Children.FirstOrDefault(child => child is MethodDeclaration) as MethodDeclaration;

        public static IList<string> Warnings(this SyntaxTree tree) =>
            tree.FirstMethodOrDefault()?.Warnings() ?? _EmptyStringArray;

        public static IList<string> Warnings(this MethodDeclaration method) =>
            (method.Annotations.FirstOrDefault(annot => annot is ILFunction) as ILFunction)?.Warnings ??
            _EmptyStringArray;
    }
}