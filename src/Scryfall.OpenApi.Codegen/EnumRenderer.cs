using System.Collections.Generic;
using System.Linq;
using Humanizer;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Scryfall.OpenApi.Codegen
{
    public class EnumRenderer : RendererBase, ICodeRenderer
    {
        public EnumRenderer(OpenApiDocument document, string templatesPath, string outputPath) : base(document, templatesPath, outputPath)
        {
        }

        protected override string TemplateName { get; } = "Enum";

        private IEnumerable<KeyValuePair<string, OpenApiSchema>> Enums =>
            Document.Components.Schemas.Where(pair => pair.Value.Type == "string" && pair.Value.Enum.Any());

        public void Render()
        {
            foreach (var (name, schema) in Enums)
            {
                var typeName = name.Pascalize();
                WriteCsFile(typeName, new
                {
                    typeName,
                    elements = from member in schema.Enum
                        let entry = (OpenApiString) member
                        select new
                        {
                            rawName = entry.Value,
                            name = entry.Value.Pascalize()
                        }
                });
            }
        }
    }
}