using Microsoft.CodeAnalysis;

namespace RediSharp.Generator.Diagnostics
{
    static class GeneratorExecutionContextExtensions
    {
        public static void Report(this GeneratorExecutionContext context, Message message, Location location)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    message.Id,
                    "RediSharp",
                    message.Text,
                    message.Severity,
                    message.Severity,
                    true,
                    0,
                    false,
                    message.Text,
                    message.Text,
                    null,
                    location));
        }

        public static void Report<T>(this GeneratorExecutionContext context, Location location)
            where T : Message, new()
        {
            context.Report(new T(), location);
        }
    }
}