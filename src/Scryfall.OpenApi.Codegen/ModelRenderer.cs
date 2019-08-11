using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using Microsoft.OpenApi.Models;

namespace Scryfall.OpenApi.Codegen
{
    public class ModelRenderer : RendererBase, ICodeRenderer
    {
        public ModelRenderer(OpenApiDocument document, string templatesPath, string outputPath) : base(document, templatesPath, outputPath)
        {
        }

        private IEnumerable<KeyValuePair<string, OpenApiSchema>> Models =>
            Document.Components.Schemas.Where(pair => pair.Value.Type == "object");

        protected override string TemplateName => "Model";

        public void Render()
        {
            foreach (var (name, schema) in Models)
            {
                var modelName = name.Pascalize();

                var propertiesSchema = schema;
                string inheritance = null;
                if (schema.AllOf != null && schema.AllOf.Count > 0)
                    foreach (var baseSchema in schema.AllOf)
                        if (baseSchema.Reference != null)
                            inheritance = $" : {baseSchema.Reference.Id.Pascalize()}";
                        else
                            propertiesSchema = baseSchema;

                WriteCsFile(modelName, new {
                    modelName,
                    inheritance,
                    properties = from pair in propertiesSchema.Properties
                        select new
                        {
                            description = pair.Value.Description?.Split(new[]{"\r\n", "\r", "\n"}, StringSplitOptions.None).First(),
                            rawName = pair.Key,
                            nullable = pair.Value.Nullable.ToString().ToLower(),
                            returnType = DetermineClrType(pair.Value),
                            propertyName = pair.Key.Pascalize()
                        }
                    });
            }
        }
    }
}