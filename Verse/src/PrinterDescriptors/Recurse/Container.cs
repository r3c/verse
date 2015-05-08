using System;
using System.Collections.Generic;

namespace Verse.PrinterDescriptors.Recurse
{
    internal class Container<TEntity, TContext, TNative>
    {
        public Dictionary<string, Follow<TEntity, TContext, TNative>> fields = new Dictionary<string, Follow<TEntity, TContext, TNative>>();

        public Follow<TEntity, TContext, TNative> items = null;

        public Func<TEntity, TNative> value = null;
    }
}