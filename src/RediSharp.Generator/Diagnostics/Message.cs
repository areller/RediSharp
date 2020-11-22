using Microsoft.CodeAnalysis;

namespace RediSharp.Generator.Diagnostics
{
    abstract class Message
    {
        public string Id { get; }
        public string Text { get; }
        public DiagnosticSeverity Severity { get; }

        protected Message(string id, string text, DiagnosticSeverity severity)
        {
            Id = id;
            Text = text;
            Severity = severity;
        }
    }
}