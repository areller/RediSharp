using System.Text;
using ICSharpCode.Decompiler.CSharp.Syntax;
using RedSharper.RedIL;

namespace RedSharper.Lua
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