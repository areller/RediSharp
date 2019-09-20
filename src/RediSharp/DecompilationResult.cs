using System;
using System.Linq;
using ICSharpCode.Decompiler.CSharp.Syntax;
using LiveDelegate.ILSpy;
using LiveDelegate.ILSpy.Extensions;

namespace RediSharp
{
    class DecompilationResult
    {
        public AstNode Body { get; }

        public string CursorVariableName { get; }

        public string ArgumentsVariableName { get; }

        public string KeysVariableName { get; }

        public DecompilationResult(
            AstNode body,
            string cursorVariableName,
            string argumentsVariableName,
            string keysVariableName)
        {
            Body = body;
            CursorVariableName = cursorVariableName;
            ArgumentsVariableName = argumentsVariableName;
            KeysVariableName = keysVariableName;
        }
        
        #region Factory

        public static DecompilationResult CreateFromDelegate<TCursor, TRes>(IDelegateReader reader,
            Function<TCursor, TRes> function)
        {
            var method = reader.Read(function)?.FirstMethodOrDefault();
            if (method is null)
            {
                throw new Exception("Unable to extract method from delegate");
            }

            var parameters = method.Parameters.ToArray();
            if (parameters.Length != 3)
            {
                throw new Exception($"Expected method with 3 arguments, got {parameters.Length}");
            }

            var cursorName = parameters[0].Name;
            var argsName = parameters[1].Name;
            var keysName = parameters[2].Name;

            return new DecompilationResult(method.Body, cursorName, argsName, keysName);
        }
        
        #endregion
    }
}