using System;

namespace MonoCollisionPipeline
{
    public class Variant
    {
        private object value;

        public Variant(string s, Arguments args = null)
        {
            if (s == "true")
            {
                value = true;
            }
            else if (s == "false")
            {
                value = false;
            }
            else if (s[0] == '$')
            {
                int i;

                if (args == null)
                {
                    throw new Exception("variable does not exist");
                }
                if (s.Length == 1)
                {
                    throw new Exception("variable format");
                }
                if (!int.TryParse(s.Substring(1), out i))
                {
                    throw new Exception("variable format");
                }
                if (i < 0 || i >= args.Count)
                {
                    throw new Exception("variable does not exist");
                }
                value = args[i].value;
            }
            else
            {
                int i;
                float f;

                if (int.TryParse(s, out i))
                {
                    value = i;
                }
                else if (float.TryParse(s, out f))
                {
                    value = f;
                }
                else
                {
                    value = s;
                }
            }
        }

        public bool BoolValue
        {
            get
            {
                if (value is bool)
                {
                    return (bool)value;
                }
                throw new Exception("can not convert");
            }
        }

        public int IntValue
        {
            get
            {
                if (value is int)
                {
                    return (int)value;
                }
                throw new Exception("can not convert");
            }
        }

        public float FloatValue
        {
            get
            {
                if (value is int)
                {
                    return (int)value;
                }
                else if (value is float)
                {
                    return (float)value;
                }
                throw new Exception("can not convert");
            }
        }

        public string StrValue
        {
            get
            {
                if (value is string)
                {
                    return (string)value;
                }
                throw new Exception("can not convert");
            }
        }
    }
}
