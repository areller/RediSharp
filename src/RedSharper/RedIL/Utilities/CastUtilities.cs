namespace RedSharper.RedIL.Utilities
{
    static class CastUtilities
    {
        public static T CastNode<S, T>(S obj)
            where T : RedILNode
            where S : RedILNode
        {
            var casted = obj as T;
            if (casted == null)
            {
                throw new RedILException($"Unable to cast node '{typeof(S)}' to '{typeof(T)}'");
            }

            return casted;
        }

        public static T CastRedILNode<T>(RedILNode obj)
            where T : RedILNode
            => CastNode<RedILNode, T>(obj);
    }
}