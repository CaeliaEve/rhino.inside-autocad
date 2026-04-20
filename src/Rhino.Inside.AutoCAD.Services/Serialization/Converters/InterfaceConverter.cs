using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rhino.Inside.AutoCAD.Services;

/// <summary>
/// A JSON converter that enables deserialization of interface types by mapping them to concrete implementations.
/// </summary>
/// <typeparam name="TClass">
/// The concrete class type used for deserialization. Must be a reference type that implements <typeparamref name="TInterface"/>.
/// </typeparam>
/// <typeparam name="TInterface">
/// The interface type that this converter handles during JSON serialization and deserialization.
/// </typeparam>
/// <remarks>
/// This converter is useful when working with dependency injection patterns or configuration systems
/// where JSON data needs to be deserialized into interface-typed properties. The converter reads JSON
/// into the concrete <typeparamref name="TClass"/> type but returns it as <typeparamref name="TInterface"/>.
/// </remarks>
/// <seealso cref="JsonConverter{T}"/>
/// <seealso cref="InterfaceStructConverter{TStruct,TInterface}"/>
public class InterfaceConverter<TClass, TInterface> : JsonConverter<TInterface> where TClass : class, TInterface
{
    /// <summary>
    /// Reads and deserializes JSON data into the concrete <typeparamref name="TClass"/> type,
    /// returning it as <typeparamref name="TInterface"/>.
    /// </summary>
    /// <param name="reader">
    /// The <see cref="Utf8JsonReader"/> to read JSON data from.
    /// </param>
    /// <param name="typeToConvert">
    /// The target type for conversion, expected to be <typeparamref name="TInterface"/>.
    /// </param>
    /// <param name="options">
    /// The <see cref="JsonSerializerOptions"/> controlling deserialization behavior.
    /// </param>
    /// <returns>
    /// The deserialized instance as <typeparamref name="TInterface"/>, or <c>null</c> if the JSON value is null.
    /// </returns>
    public override TInterface? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<TClass>(ref reader, options);
    }

    /// <summary>
    /// Writes the <typeparamref name="TInterface"/> value to JSON.
    /// </summary>
    /// <param name="writer">
    /// The <see cref="Utf8JsonWriter"/> to write JSON data to.
    /// </param>
    /// <param name="value">
    /// The <typeparamref name="TInterface"/> value to serialize.
    /// </param>
    /// <param name="options">
    /// The <see cref="JsonSerializerOptions"/> controlling serialization behavior.
    /// </param>
    /// <remarks>
    /// This implementation is intentionally empty as serialization is not required for this converter's use case.
    /// </remarks>
    public override void Write(Utf8JsonWriter writer, TInterface value, JsonSerializerOptions options) { }
}