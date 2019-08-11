using System.Linq;
using Humanizer;
using Microsoft.OpenApi.Models;

namespace Scryfall.OpenApi.Codegen
{
    public class ApiInterfaceRenderer : RendererBase, ICodeRenderer
    {
        public ApiInterfaceRenderer(OpenApiDocument document, string templatesPath, string outputPath) : base(document, templatesPath, outputPath)
        {
        }

        protected override string TemplateName => "ApiClientInterface";

        public void Render()
        {
            WriteCsFile("IApiClient", new
            {
                operations = from pathEntry in Document.Paths
                    from operationEntry in pathEntry.Value.Operations
                    select new
                    {
                        summary = operationEntry.Value.Summary,
                        description = operationEntry.Value.Description,
                        returnType = operationEntry.Value.Responses.First().Value.Content["application/json"].Schema.Reference.Id.Pascalize(),
                        name = operationEntry.Value.OperationId.Pascalize(),
                        parameters = string.Join(", ", operationEntry.Value.Parameters.Select(p => $"{DetermineClrType(p.Schema)} {p.Name.Camelize()}") )
                    }
            });
        }
    }
}