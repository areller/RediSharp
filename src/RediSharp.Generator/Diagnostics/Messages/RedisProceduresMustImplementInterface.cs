using Microsoft.CodeAnalysis;

namespace RediSharp.Generator.Diagnostics.Messages
{
    class RedisProceduresMustImplementInterface : Message
    {
        public RedisProceduresMustImplementInterface()
            : base(
                  "RS1001",
                  "Redis procedures must implement the IRedisProcedure<TRes> interface.",
                  DiagnosticSeverity.Error)
        {
        }
    }
}