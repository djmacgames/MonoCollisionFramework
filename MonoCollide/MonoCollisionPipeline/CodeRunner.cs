using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoCollisionPipeline
{
    public class CodeRunner
    {
        private class CodeException : Exception
        {
            public CodeException(string message)
                : base(message)
            {
            }
        }

        private ContentImporterContext context;
        private Arguments args;

        public CodeRunner(ContentImporterContext context, Arguments args = null)
        {
            this.context = context;
            this.args = args;
        }

        public void Run(string filename, ICodeHandler handler)
        {
            string[] lines = File.ReadAllLines(filename);
            int lineNumber = 1;

            try
            {
                foreach (string line in lines)
                {
                    string tline = line.Trim();

                    if (!tline.StartsWith("#") && tline.Length != 0)
                    {
                        string[] tokens = tline.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        Variant[] args = new Variant[tokens.Length - 1];

                        for (int i = 0; i != args.Length; i++)
                        {
                            args[i] = new Variant(tokens[i + 1], this.args);
                        }

                        Arguments cargs = new Arguments(args);

                        if (!handler.HandleCommand(tokens[0], cargs, context))
                        {
                            string cfilename = tokens[0] + ".txt";

                            if (File.Exists(cfilename))
                            {
                                CodeRunner runner = new CodeRunner(context, cargs);

                                runner.Run(cfilename, handler);
                            }
                            else
                            {
                                throw new Exception("undefined");
                            }
                        }
                    }
                    lineNumber++;
                }
            }
            catch (CodeException ex)
            {
                throw new CodeException(string.Format("{0}\n\t\tline {1} : source '{2}'", ex.Message, lineNumber, filename));
            }
            catch (Exception ex)
            {
                throw new CodeException(string.Format("\n\tERROR - {0}\n\t\tline {1} : source '{2}'", ex.Message, lineNumber, filename));
            }
        }
    }
}
