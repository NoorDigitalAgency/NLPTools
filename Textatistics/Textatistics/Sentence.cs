using System;

namespace Textatistics
{
    [Serializable]
    public class Sentence
    {
        public int Offset { get; set; }

        public Token[] Tokens { get; set; }
    }
}
