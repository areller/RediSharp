using Microsoft.CodeAnalysis;

namespace RediSharp.Generator.Diagnostics.Messages
{
    class ArrowExpressionsNotSupported : Message
    {
        public ArrowExpressionsNotSupported()
            : base(
                  "RS1002",
                  "Arrow expressions are not supported currently.",
                  DiagnosticSeverity.Error)
        {
        }
    }
}