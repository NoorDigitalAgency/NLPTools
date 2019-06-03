using Microsoft.ML.Data;

namespace Textatistics
{
    public class LanguageSentence
    {
        [LoadColumn(0)]
        public string Language { get; set; }

        [LoadColumn(1)]
        public string Sentence { get; set; }
    }
}