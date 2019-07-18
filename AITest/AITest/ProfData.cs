using Microsoft.ML.Data;

namespace AITest
{
    public class ProfData
    {
        [LoadColumn(0)]
        public string Text { get; set; }

        [LoadColumn(1)]
        public string Title { get; set; }

        [LoadColumn(2)]
        public string Group { get; set; }
    }
}