using NStagger;

namespace Textatistics
{
    public class Ad
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }

        public string Text { get; set; }

        public TaggedToken[][] TaggedData { get; set; }
    }
}