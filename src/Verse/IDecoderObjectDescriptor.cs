using System;

namespace Verse;

public interface IDecoderObjectDescriptor<TNative, TObject>
{
    /// <summary>
    /// Declare new named field on current object entity and reuse existing
    /// descriptor to define how it should be decoded. This method can be
    /// used to describe recursive schemas.
    /// </summary>
    /// <typeparam name="TField">Field type</typeparam>
    /// <param name="name">Field name</param>
    /// <param name="setter">Field setter to current entity</param>
    /// <param name="descriptor">Existing decoder descriptor</param>
    /// <returns>Object decoder field descriptor</returns>
    IDecoderDescriptor<TNative, TField> HasField<TField>(string name, Func<TObject, TField, TObject> setter,
        IDecoderDescriptor<TNative, TField> descriptor);

    /// <summary>
    /// Declare new named field on current object entity. Resulting
    /// descriptor defines how it should be decoded.
    /// </summary>
    /// <typeparam name="TField">Field type</typeparam>
    /// <param name="name">Field name</param>
    /// <param name="setter">Field setter to current entity</param>
    /// <returns>Object decoder field descriptor</returns>
    IDecoderDescriptor<TNative, TField> HasField<TField>(string name, Func<TObject, TField, TObject> setter);

    /// <summary>
    /// Declare new named field on current object entity without creating a
    /// dedicated instance for this field. This method can be used to
    /// flatten complex hierarchies when mapping them.
    /// </summary>
    /// <param name="name">Field name</param>
    /// <returns>Current entity field descriptor</returns>
    IDecoderDescriptor<TNative, TObject> HasField(string name);
}