using Microsoft.ML.Data;

namespace Textatistics
{
    public class LanguageSentence
    {
        [LoadColumn(0)]
        public string Label { get; set; }

        [LoadColumn(1)]
        public string Sentence { get; set; }
    }
}