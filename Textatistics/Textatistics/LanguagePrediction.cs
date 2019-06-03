using Microsoft.ML.Data;

namespace Textatistics
{
    public class LanguagePrediction
    {
        [ColumnName("PredictedLabel")]
        public string Label;

        public float[] Score { get; set; }
    }
}