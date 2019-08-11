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
            {
                return schema.Reference.Id.Pascalize();
            }

            switch (schema.Type)
            {
                case "string":
                {
                    switch (schema.Format)
                    {
                        case "uri":
                            return nameof(Uri);
                        case "date":
                            return nameof(DateTime);
                        case "date-time":
                            return nameof(DateTimeOffset);
                        case "uuid":
                            return nameof(Guid);
                        default:
                            return "string";
                    }
                }
                case "boolean":
                {
                    return "bool";
                }
                case "integer":
                case "number":
                {
                    return "int";
                }
                case "array":
                {
                    return $"List<{DetermineClrType(schema.Items)}>";
                }
                case "object":
                {
                    if (schema.AdditionalProperties != null)
                    {
                        return $"Dictionary<string,{DetermineClrType(schema.AdditionalProperties)}>"; 
                    }
                    return "object";
                }
                default:
                    throw new InvalidOperationException(schema.Type);
            }
        }

        protected void WriteCsFile( string baseName, object context)
        {
            Action<TextWriter, object> compiledTemplate;
            using (var s = File.OpenText(Path.Combine(_templatesPath, $"{TemplateName}.mustache")))
            {
                compiledTemplate = Handlebars.Compile(s);
            }

            var outputFileName = Path.Combine(_outputPath, $"{baseName}.Designer.cs");
            if (File.Exists(outputFileName)) File.Delete(outputFileName);
            using (var o = new StreamWriter(File.OpenWrite(outputFileName)))
            {
                Console.WriteLine($"Writing file {outputFileName}");
                compiledTemplate.Invoke(o, context);
            }
        }
    }
}