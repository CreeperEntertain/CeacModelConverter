using CeacModelConverter.Conversion;

namespace CeacModelConverter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("CEAC Model Converter");
            Console.WriteLine("Java Geometry <-> OBJ\n");

            if (args.Length > 0)
            {
                foreach (string input in args)
                    ConvertFile(input);
                return;
            }

            Console.WriteLine("Drag a Java geometry class or OBJ file into this console and press Enter.");
            Console.Write("File: ");

            string inputPath = Console.ReadLine() ?? String.Empty;
            ConvertFile(inputPath);

            Console.WriteLine();
            Console.WriteLine("Press Enter to close.");
            Console.ReadLine();
        }

        private static void ConvertFile(string inputPath)
        {
            inputPath = CleanDraggedPath(inputPath);

            if (string.IsNullOrWhiteSpace(inputPath))
            {
                Console.WriteLine("No file supplied.");
                return;
            }
            if (!File.Exists(inputPath))
            {
                Console.WriteLine($"File not found: {inputPath}");
                return;
            }

            string extension = Path.GetExtension(inputPath);

            try
            {
                string outputPath;

                if (extension.Equals(".java", StringComparison.OrdinalIgnoreCase))
                {
                    var converter = new JavaToObjConverter();
                    outputPath = converter.ConvertFile(inputPath);
                }
                else if (extension.Equals(".obj", StringComparison.OrdinalIgnoreCase))
                {
                    var converter = new ObjToJavaConverter();
                    outputPath = converter.ConvertFile(inputPath);
                }
                else
                {
                    Console.WriteLine($"Unsupported file type: {Path.GetFileName(inputPath)}");
                    return;
                }

                Console.WriteLine($"Created: {outputPath}");
            }
            catch (Exception exception)
            {
                Console.WriteLine();
                Console.WriteLine($"Conversion failed for: {inputPath}");
                Console.WriteLine(exception);
            }

            Console.WriteLine();
        }

        private static string CleanDraggedPath(string input)
        {
            string path = input.Trim();

            bool changed;

            do
            {
                changed = false;
                if (path.Length >= 2)
                    if (
                        (path.StartsWith("\"") && path.EndsWith("\"")) ||
                        (path.StartsWith("'") && path.EndsWith("'")) ||
                        (path.StartsWith("(") && path.EndsWith(")"))
                    )
                    {
                        path = path.Substring(1, path.Length - 1).Trim();
                        changed = true;
                    }
            }
            while (changed);

            return path;
        }
    }
}
