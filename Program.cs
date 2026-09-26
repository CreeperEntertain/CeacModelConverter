using CeacModelConverter.Conversion;

namespace CeacModelConverter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("CEAC Model Converter");
            Console.WriteLine("Java Geometry -> OBJ\n");

            string inputPath;

            if (args.Length > 0)
                inputPath = args[0];
            else
            {
                Console.WriteLine("Drag a Java geometry class into this console and press Enter.");
                Console.Write("File: ");

                inputPath = Console.ReadLine() ?? String.Empty;
            }

            inputPath = CleanDraggedPath(inputPath);

            if (string.IsNullOrWhiteSpace(inputPath))
            {
                Console.WriteLine("No file supplied.");
                return;
            }
            if (!File.Exists(inputPath))
            {
                Console.WriteLine($"File not found: {inputPath}");
            }

            string extension = Path.GetExtension(inputPath);

            if (!extension.Equals(".java", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("The input file must be a .java file.");
                return;
            }

            try
            {
                var converter = new JavaToObjConverter();

                string outputPath = converter.ConvertFile(inputPath);

                Console.WriteLine();
                Console.WriteLine($"Created: {outputPath}");
            }
            catch (Exception exception)
            {
                Console.WriteLine();
                Console.WriteLine("Conversion failed:");
                Console.WriteLine(exception);
            }

            if (args.Length == 0)
            {
                Console.WriteLine();
                Console.WriteLine("Press Enter to close.");
                Console.ReadLine();
            }
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
