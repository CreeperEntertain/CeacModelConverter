using CeacModelConverter.Parsing;
using CeacModelConverter.WavefrontObj;

namespace CeacModelConverter.Conversion
{
    internal sealed class JavaToObjConverter
    {
        private readonly JavaGeometryParser parser;
        private readonly ObjWriter writer;

        public JavaToObjConverter()
        {
            parser = new JavaGeometryParser();
            writer = new ObjWriter();
        }

        public string ConvertFile(string inputPath)
        {
            string source = File.ReadAllText(inputPath);
            string fallbackName = Path.GetFileNameWithoutExtension(inputPath);
            var geometry = parser.Parse(source, fallbackName);
            string outputPath = Path.ChangeExtension(inputPath, ".obj");

            writer.Write(geometry, outputPath);

            return outputPath;
        }
    }
}
