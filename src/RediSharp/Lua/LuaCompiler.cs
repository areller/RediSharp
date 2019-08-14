using System.Text;
using ICSharpCode.Decompiler.CSharp.Syntax;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL;

namespace RediSharp.Lua
{
    class LuaCompiler
    {
        public LuaCompiler()
        {

        }

        public string Compile(RedILNode tree)
        {
            var instance = new CompilationInstance(tree);
            return instance.Compile();
        }
    }
}