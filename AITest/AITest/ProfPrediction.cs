using Microsoft.ML.Data;

namespace AITest
{
    public class ProfPrediction: ProfData
    {
        [ColumnName("PredictedLabel")]
        public string Prediction { get; set; }

        public float Probability { get; set; }

        public float[] Score { get; set; }
    }
}