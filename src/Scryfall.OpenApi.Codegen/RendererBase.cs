using System;
using System.IO;
using HandlebarsDotNet;
using Humanizer;
using Microsoft.OpenApi.Models;

namespace Scryfall.OpenApi.Codegen
{
    public abstract class RendererBase
    {
        protected OpenApiDocument Document { get; }
        private readonly string _templatesPath;
        private readonly string _outputPath;

        protected RendererBase(OpenApiDocument document, string templatesPath, string outputPath)
        {
            Document = document;
            _templatesPath = templatesPath;
            _outputPath = outputPath;
        }

        protected abstract string TemplateName { get; }

        protected static string DetermineClrType(OpenApiSchema schema)
        {
            if (schema.Reference != null)
                return schema.Reference.Id.Pascalize();

            switch (schema.Type)
            {
                case "string":
                    return schema.Format switch
                    {
                        "uri" => nameof(Uri),
                        "date" => nameof(DateTime),
                        "date-time" => nameof(DateTimeOffset),
                        "uuid" => nameof(Guid),
                        _ => "string",
                    };
                case "boolean":
                    return "bool";
                case "integer":
                    return "int";
                case "number":
                    return schema.Format switch
                    {
                        "double" => "double",
                        _ => "int",
                    };
                case "array":
                    return $"List<{DetermineClrType(schema.Items)}>";
                case "object":
                {
                    if (schema.AdditionalProperties != null)
                        return $"Dictionary<string,{DetermineClrType(schema.AdditionalProperties)}>";
                    return "object";
                }
                default:
                    throw new InvalidOperationException(schema.Type);
            }
        }

        protected void WriteCsFile(string baseName, object context)
        {
            Action<TextWriter, object> compiledTemplate;
            using (var s = File.OpenText(Path.Combine(_templatesPath, $"{TemplateName}.mustache")))
            {
                compiledTemplate = Handlebars.Compile(s);
            }

            var outputFileName = Path.Combine(_outputPath, $"{baseName}.Designer.cs");
            if (File.Exists(outputFileName)) File.Delete(outputFileName);
            using var o = new StreamWriter(File.OpenWrite(outputFileName));
            Console.WriteLine($"Writing file {outputFileName}");
            compiledTemplate.Invoke(o, context);
        }
    }
}