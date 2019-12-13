using System.Collections.Generic;
using System.Linq;

namespace TagsCloudTextProcessing.Formatters
{
    public class TrimFormatter : IWordsFormatter
    {
        public IEnumerable<string> Format(IEnumerable<string> wordsInput)
        {
            return wordsInput.Select(w => w.Trim());
        }
    }
}