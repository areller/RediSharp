using Microsoft.CodeAnalysis;

namespace RediSharp.Generator.Diagnostics.Messages
{
    class RedisProceduresMustBePartial : Message
    {
        public RedisProceduresMustBePartial()
            : base(
                  "RS1001",
                  "Redis procedures must be a partial class.",
                  DiagnosticSeverity.Error)
        {
        }
    }
}