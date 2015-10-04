using System;

namespace MonoCollisionPipeline
{
    public class Arguments
    {
        private Variant[] args;

        public Arguments(Variant[] args)
        {
            this.args = args;
        }

        public int Count
        {
            get { return args.Length; }
        }

        public Variant this[int i]
        {
            get
            {
                if (i >= 0 && i < Count)
                {
                    return args[i];
                }
                throw new Exception("argument count");
            }
        }
    }
}
