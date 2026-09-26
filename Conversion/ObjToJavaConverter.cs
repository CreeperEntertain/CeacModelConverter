using CeacModelConverter.Java;
using CeacModelConverter.Parsing;

namespace CeacModelConverter.Conversion
{
    internal sealed class ObjToJavaConverter
    {
        private readonly ObjGeometryParser parser;
        private readonly JavaGeometryWriter writer;

        public ObjToJavaConverter()
        {
            parser = new ObjGeometryParser();
            writer = new JavaGeometryWriter();
        }

        public string ConvertFile(string inputPath)
        {
            string source = File.ReadAllText(inputPath);
            string fallbackName = Path.GetFileNameWithoutExtension(inputPath);
            var geometry = parser.Parse(source, fallbackName);
            string outputPath = Path.ChangeExtension(inputPath, ".java");

            writer.Write(geometry, outputPath);

            return outputPath;
        }
    }
}
