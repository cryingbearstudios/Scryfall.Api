using System.Linq;
using Humanizer;
using Microsoft.OpenApi.Models;

namespace Scryfall.OpenApi.Codegen
{
    public class ApiClientRenderer : RendererBase, ICodeRenderer
    {
        public ApiClientRenderer(OpenApiDocument document, string templatesPath, string outputPath) : base(document, templatesPath, outputPath)
        {
        }

        protected override string TemplateName => "ApiClient";

        public void Render()
        {
            WriteCsFile("ApiClient", new
            {
                baseUrl = Document.Servers[0].Url,
                operations = Document.Paths
                    .SelectMany(pathEntry => pathEntry.Value.Operations,
                        (pathEntry, operationEntry) => CreateOperationModel(pathEntry.Key, operationEntry.Key, operationEntry.Value))
            });
        }

        private static object CreateOperationModel(string path, OperationType operationType, OpenApiOperation operation)
        {
            var parameters = operation.Parameters.Select(p =>
            {
                var parameterType = DetermineClrType(p.Schema);
                var parameterName = p.Name.Camelize();
                return new
                {
                    Key = p.Name,
                    summary = p.Description,
                    Value = parameterName,
                    Declaration = $"{parameterType} {parameterName}"
                };
            }).ToList();
            return new
            {
                summary = operation.Summary,
                description = operation.Description,
                returnType = OperationReturnType(operation),
                name = operation.OperationId.Pascalize(),
                parametersString = string.Join(", ", parameters.Select(p => p.Declaration)),
                operationType, 
                path,
                parameters,
            };
        }

        private static string OperationReturnType(OpenApiOperation operation)
        {
            var (_, firstResponse) = operation.Responses.First();
            var jsonContent = firstResponse.Content["application/json"];
            return DetermineClrType(jsonContent.Schema);
        }
    }
}