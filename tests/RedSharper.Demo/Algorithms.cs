using System;
using System.Collections.Generic;

namespace RedSharper.Demo
{
    static class Algorithms
    {
        public static void Traverse<T>(this T root, Func<T, IEnumerable<T>> childrenCall, Func<T, bool> call)
        {
            var stack = new Stack<T>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                var top = stack.Pop();
                var cont = call?.Invoke(top);
                if (!(cont ?? false)) continue;

                var children = childrenCall?.Invoke(top);
                if (children != null)
                {
                    foreach (var child in children)
                    {
                        stack.Push(child);
                    }
                }
            }
        }
    }
}