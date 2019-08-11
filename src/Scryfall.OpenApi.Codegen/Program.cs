using System;
using System.IO;
using HandlebarsDotNet;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace Scryfall.OpenApi.Codegen
{
    public class Program
    {
        public static void Main()
        {
            SetWorkingDirectoryToProjectRoot();
            Handlebars.Configuration.TextEncoder = new PassthroughEncoder();
            var openApiYaml = Path.Combine("..", "scryfall-openapi", "openapi.yml");
            OpenApiDocument document;
            Console.WriteLine("Loading OpenApi YAML");
            using (var stream = File.OpenRead(openApiYaml))
            {
                var reader = new OpenApiStreamReader();
                document = reader.Read(stream, out _);
            }

            var templatesPath = Path.Combine("src", "Scryfall.OpenApi.Codegen");
            var outputPath = Path.Combine("src", "Scryfall.Api");
            new CodeRenderer(document, templatesPath, outputPath).Render();
        }

        private static void SetWorkingDirectoryToProjectRoot()
        {
            var startDir = Directory.GetCurrentDirectory();
            var dir = new DirectoryInfo(startDir);
            while (dir != null && dir.GetFiles("*.sln").Length != 1)
                dir = dir.Parent;
            if (dir == null) throw new InvalidOperationException("Couldn't find project root");
            if (dir.FullName != startDir) Directory.SetCurrentDirectory(dir.FullName);
        }
    }

    public class PassthroughEncoder : ITextEncoder
    {
        public string Encode(string value)
        {
            return value;
        }
    }
}
