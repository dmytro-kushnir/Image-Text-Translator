using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerVisionSample.helpers
{
    class Utils
    {
        public static IEnumerable<string> SplitBy(string str, int chunkLength)
        {
            if (String.IsNullOrEmpty(str)) yield return "";

            for (int i = 0; i < str.Length; i += chunkLength)
            {
                if (chunkLength + i > str.Length)
                    chunkLength = str.Length - i;

                yield return str.Substring(i, chunkLength);
            }
         }
    }
}
