using System;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoCollisionPipeline
{
    public interface ICodeHandler
    {
        bool HandleCommand(string name, Arguments args, ContentImporterContext context);
    }
}
