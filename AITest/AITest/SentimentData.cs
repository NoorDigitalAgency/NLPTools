using Microsoft.ML.Data;

namespace AITest
{
    public class SentimentData
    {
        [LoadColumn(0)]
        public string SentimentText;

        [LoadColumn(1)]
        public string SentimentTags;

        [LoadColumn(2)]
        public string SentimentLemmas;

        [LoadColumn(3), ColumnName("Label")]
        public bool Sentiment;
    }
}