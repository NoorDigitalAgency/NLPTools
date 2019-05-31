using System;
using System.Collections.Generic;

namespace Textatistics
{
    [Serializable]
    public class Ad
    {
        public string Category { get; set; }

        public List<Sentence> Sentences { get; set; }
    }
}