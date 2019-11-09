using System.Collections.Generic;
using Microsoft.OpenApi.Models;

namespace Scryfall.OpenApi.Codegen
{
    public class CodeRenderer : ICodeRenderer
    {
        private readonly IEnumerable<ICodeRenderer> _inner;

        public CodeRenderer(OpenApiDocument document, string templatesPath, string outputPath)
        {
            _inner = new ICodeRenderer[]
            {
                new EnumRenderer(document, templatesPath, outputPath),
                new ModelRenderer(document, templatesPath, outputPath),
                new ApiInterfaceRenderer(document, templatesPath, outputPath),
                new ApiClientRenderer(document, templatesPath, outputPath),
            };
        }

        public void Render()
        {
            foreach (var codeRenderer in _inner)
            {
                codeRenderer.Render();
            }
        }
    }
}