using CeacModelConverter.Model;
using System.Globalization;
using System.Text;

namespace CeacModelConverter.Java
{
    internal class JavaGeometryWriter
    {
        public void Write(
            GeometryModel geometry,
            string outputPath
        )
        {
            string contents = BuildSource(geometry);

            File.WriteAllText(outputPath, contents, new UTF8Encoding(false));
        }

        private static string BuildSource(GeometryModel geometry)
        {
            var output = new StringBuilder();

            // Imports
            output.AppendLine("package net.centertain.ceac.material.shapes.models;");
            output.AppendLine();
            output.AppendLine("import net.centertain.ceac.material.utility.ModelHelper;");
            output.AppendLine("import net.minecraft.client.renderer.block.model.ItemOverrides;");
            output.AppendLine("import net.minecraft.client.renderer.texture.TextureAtlasSprite;");
            output.AppendLine("import net.minecraft.client.resources.model.*;");
            output.AppendLine("import net.minecraft.resources.ResourceLocation;");
            output.AppendLine("import net.minecraftforge.client.model.geometry.IGeometryBakingContext;");
            output.AppendLine("import net.minecraftforge.client.model.geometry.IUnbakedGeometry;");
            output.AppendLine();
            output.AppendLine("import java.util.function.Function;");
            output.AppendLine();

            // Class head
            output.AppendLine($"public class {geometry.Name} implements IUnbakedGeometry<{geometry.Name}> {{");
            output.AppendLine("    public static BakedModel COLLISION_SHAPE;");
            output.AppendLine();

            // Builder start
            output.AppendLine("    @Override");
            output.AppendLine("    public BakedModel bake(");
            output.AppendLine("            IGeometryBakingContext context,");
            output.AppendLine("            ModelBaker baker,");
            output.AppendLine("            Function<Material, TextureAtlasSprite> sprites,");
            output.AppendLine("            ModelState modelState,");
            output.AppendLine("            ItemOverrides overrides,");
            output.AppendLine("            ResourceLocation modelLocation");
            output.AppendLine("    ) {");
            output.AppendLine("        TextureAtlasSprite sprite = sprites.apply(context.getMaterial(\"texture\"));");
            output.AppendLine();
            output.AppendLine("        SimpleBakedModel.Builder builder = new SimpleBakedModel.Builder(");
            output.AppendLine("                context.useAmbientOcclusion(),");
            output.AppendLine("                context.useBlockLight(),");
            output.AppendLine("                context.isGui3d(),");
            output.AppendLine("                context.getTransforms(),");
            output.AppendLine("                overrides");
            output.AppendLine("        );");
            output.AppendLine();
            output.AppendLine("        builder.particle(sprite);");
            output.AppendLine();

            // Face generation
            foreach (GeometryFace face in geometry.Faces)
            {
                if (!string.IsNullOrWhiteSpace(face.Comment))
                    output.AppendLine($"        // {face.Comment}");

                string helper = face.Type == FaceType.Quad
                    ? "quad"
                    : "triangle";

                output.AppendLine($"        builder.addUnculledFace(ModelHelper.{helper}(");
                output.AppendLine("                sprite,");

                for (int i = 0; i < face.Vertices.Count; i++)
                {
                    GeometryVertex vertex = face.Vertices[i];
                    string suffix = i == face.Vertices.Count - 1
                        ? ""
                        : ",";

                    output.AppendLine($"                ModelHelper.vertex({Format(vertex.X)}, {Format(vertex.Y)}, {Format(vertex.Z)}, {Format(vertex.U)}, {Format(vertex.V)}){suffix}");
                }

                output.AppendLine("        ));");
                output.AppendLine();
            }

            // Builder end
            output.AppendLine("        COLLISION_SHAPE = builder.build(context.getRenderType(modelLocation));");
            output.AppendLine("        return COLLISION_SHAPE;");
            output.AppendLine("    }");
            output.AppendLine("}");

            return output.ToString();
        }

        private static string Format(double value)
            => value.ToString("0.###############", CultureInfo.InvariantCulture) + "f";
    }
}
