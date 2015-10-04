using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

using TImport = Microsoft.Xna.Framework.Content.Pipeline.Graphics.NodeContent;

namespace MonoCollisionPipeline
{
    [ContentImporter(".txt", DisplayName = "Text Model Importer", DefaultProcessor = "ModelProcessor")]
    public class TextModelImporter : ContentImporter<TImport>
    {
        public override TImport Import(string filename, ContentImporterContext context)
        {
            CodeRunner runner = new CodeRunner(context);
            ModelCodeHandler handler = new ModelCodeHandler();

            runner.Run(filename, handler);

            return handler.Root;
        }
    }
}
