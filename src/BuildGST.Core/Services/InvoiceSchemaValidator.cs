using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using BuildGST.Abstractions.Interfaces;

namespace BuildGST.Core.Services;

/// <summary>
/// Validates invoice JSON against the embedded GST invoice XML schema.
/// </summary>
public sealed class InvoiceSchemaValidator : IInvoiceSchemaValidator
{
    private const string SchemaResourceName = "BuildGST.Core.Resources.GstInvoiceSchema.xsd";
    private static readonly Lazy<XmlSchemaSet> SchemaSet = new Lazy<XmlSchemaSet>(LoadSchemaSet, true);

    /// <inheritdoc />
    public bool Validate(string json)
    {
        return GetValidationErrors(json).Count == 0;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<string> GetValidationErrors(string json)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(json))
        {
            errors.Add("Invoice JSON is required.");
            return errors;
        }

        try
        {
            var xmlDocument = ConvertJsonToXml(json);
            xmlDocument.Validate(
                SchemaSet.Value,
                (sender, eventArgs) => errors.Add(eventArgs.Message),
                true);
        }
        catch (JsonException exception)
        {
            errors.Add($"Invalid JSON: {exception.Message}");
        }
        catch (XmlException exception)
        {
            errors.Add($"Invalid XML generated from JSON: {exception.Message}");
        }
        catch (InvalidOperationException exception)
        {
            errors.Add(exception.Message);
        }

        return errors;
    }

    private static XDocument ConvertJsonToXml(string json)
    {
        using (var document = JsonDocument.Parse(json))
        {
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException("Invoice JSON root must be an object.");
            }

            var root = new XElement("invoice");
            foreach (var property in document.RootElement.EnumerateObject())
            {
                root.Add(ConvertProperty(property.Name, property.Value));
            }

            return new XDocument(root);
        }
    }

    private static XElement ConvertProperty(string name, JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Array)
        {
            var arrayElement = new XElement(name);
            foreach (var item in value.EnumerateArray())
            {
                arrayElement.Add(ConvertArrayItem(name, item));
            }

            return arrayElement;
        }

        if (value.ValueKind == JsonValueKind.Object)
        {
            var element = new XElement(name);
            foreach (var property in value.EnumerateObject())
            {
                element.Add(ConvertProperty(property.Name, property.Value));
            }

            return element;
        }

        return new XElement(name, ConvertScalar(value));
    }

    private static XElement ConvertArrayItem(string parentName, JsonElement item)
    {
        var itemName = parentName == "itemList" ? "item" : "value";
        if (item.ValueKind == JsonValueKind.Object)
        {
            var element = new XElement(itemName);
            foreach (var property in item.EnumerateObject())
            {
                element.Add(ConvertProperty(property.Name, property.Value));
            }

            return element;
        }

        return new XElement(itemName, ConvertScalar(item));
    }

    private static string ConvertScalar(JsonElement value)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.String:
                return value.GetString() ?? string.Empty;
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
                return value.GetRawText();
            case JsonValueKind.Null:
                return string.Empty;
            default:
                throw new InvalidOperationException($"Unsupported JSON value kind '{value.ValueKind}' for XML conversion.");
        }
    }

    private static XmlSchemaSet LoadSchemaSet()
    {
        var assembly = typeof(InvoiceSchemaValidator).GetTypeInfo().Assembly;
        using (var stream = assembly.GetManifestResourceStream(SchemaResourceName))
        {
            if (stream == null)
            {
                throw new InvalidOperationException($"Embedded schema resource '{SchemaResourceName}' was not found.");
            }

            using (var reader = XmlReader.Create(stream))
            {
                var schemaSet = new XmlSchemaSet();
                schemaSet.Add(null, reader);
                schemaSet.Compile();
                return schemaSet;
            }
        }
    }
}
