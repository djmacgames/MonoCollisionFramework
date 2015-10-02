using System;
using System.IO;

namespace MonoCollisionPipeline
{
    public class Noise
    {
        private float[] amounts = null;
        private int i = 0;

        public Noise(Type type, string resourceName)
        {
            StreamReader reader = null;

            try
            {
                String text = (reader = new StreamReader(type.Assembly.GetManifestResourceStream(resourceName))).ReadToEnd();
                String[] tokens = text.Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                amounts = new float[tokens.Length];
                for (int j = 0; j != tokens.Length; j++)
                {
                    amounts[j] = float.Parse(tokens[j]);
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        public void Reset()
        {
            i = 0;
        }

        public float Next()
        {
            float amount = amounts[i];

            i = (i + 1) % amounts.Length;

            return amount;
        }
    }
}
