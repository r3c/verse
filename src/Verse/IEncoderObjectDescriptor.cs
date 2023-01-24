using System;

namespace Verse;

public interface IEncoderObjectDescriptor<TNative, out TObject>
{
    /// <summary>
    /// Declare new named field on current object entity and reuse existing
    /// descriptor to define how it should be encoded. This method can be
    /// used to describe recursive schemas.
    /// </summary>
    /// <typeparam name="TField">Field type</typeparam>
    /// <param name="name">Field name</param>
    /// <param name="getter">Field getter from current entity</param>
    /// <param name="descriptor">Existing encoder descriptor</param>
    /// <returns>Field encoder descriptor</returns>
    IEncoderDescriptor<TNative, TField> HasField<TField>(string name, Func<TObject, TField> getter,
        IEncoderDescriptor<TNative, TField> descriptor);

    /// <summary>
    /// Declare new named field on current object entity. Resulting
    /// descriptor defines how it should be encoded.
    /// </summary>
    /// <typeparam name="TField">Field type</typeparam>
    /// <param name="name">Field name</param>
    /// <param name="getter">Field getter from current entity</param>
    /// <returns>Field encoder descriptor</returns>
    IEncoderDescriptor<TNative, TField> HasField<TField>(string name, Func<TObject, TField> getter);

    /// <summary>
    /// Declare new named field on current object entity without using a
    /// dedicated instance for this field. This method can be used to
    /// flatten complex hierarchies when mapping them.
    /// </summary>
    /// <param name="name">Field name</param>
    /// <returns>Current entity encoder descriptor</returns>
    IEncoderDescriptor<TNative, TObject> HasField(string name);
}