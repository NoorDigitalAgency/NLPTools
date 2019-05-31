using System;

namespace Textatistics
{
    [Serializable]
    public class Token
    {
        public string Value { get; set; }

        public string Lemma { get; set; }

        public string Id { get; set; }

        public int NeTag { get; set; }

        public int NeTypeTag { get; set; }

        public int PosTag { get; set; }

        public bool IsCapitalized { get; set; }

        public bool IsSpace { get; set; }

        public int Offset { get; set; }

        public int TokenType { get; set; }
    }
}