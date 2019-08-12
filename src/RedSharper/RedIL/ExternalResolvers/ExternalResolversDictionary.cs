using System.Collections.Generic;
using System.Linq;
using RedSharper.RedIL.Attributes;
using RedSharper.RedIL.ExternalResolvers.Enums;

namespace RedSharper.RedIL.ExternalResolvers
{
    class ExternalResolversDictionary
    {
        class Entry
        {
            public string FullName { get; set; }

            public string ReflectionName { get; set; }

            public string Name { get; set; }

            public EntryType Type { get; set; }

            public RedILResolver Resolver { get; set; }
        }

        private List<Entry> _entries;
        
        public ExternalResolversDictionary()
        {
            _entries = new List<Entry>();

            _entries.Add(new Entry()
            {
                FullName = "System.Nullable",
                ReflectionName = null,
                Name = "HasValue",
                Type = EntryType.Member,
                Resolver = new NullableHasValueResolver()
            });

            _entries.Add(new Entry()
            {
                FullName = "System.Nullable",
                ReflectionName = null,
                Name = "Value",
                Type = EntryType.Member,
                Resolver = new NullableValueResolver()
            });
        }

        public RedILResolver FindResolver(string reflectionName, string fullName, string name, EntryType type)
        {
            var entry = _entries.FirstOrDefault(e =>
                (e.ReflectionName == reflectionName || e.FullName == fullName) && e.Name == name && e.Type == type);
            if (entry is null)
            {
                throw new RedILException($"Unable to resolver for method ReflectionName: '{reflectionName}', FullName: '{fullName}', Name: '{name}', Type: '{type}'");
            }

            return entry.Resolver;
        }
    }
}