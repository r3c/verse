using System;

namespace Verse.Test.Schemas;

public static class SetterHelper
{
    public static Func<TEntity, TField, TEntity> Mutation<TEntity, TField>(Action<TEntity, TField> action)
    {
        return (entity, field) =>
        {
            action(entity, field);

            return entity;
        };
    }
}