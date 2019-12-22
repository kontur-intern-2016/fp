using System.Collections.Generic;
using System.Linq;
using TagsCloud.CloudConstruction;
using TagsCloud.ErrorHandler;
using TagsCloud.Visualization.SizeDefiner;
using TagsCloud.Visualization.Tag;

namespace TagsCloud.Visualization
{
    public class Layouter : ILayouter
    {
        private readonly ICloudLayouter _cloud;
        private readonly ISizeDefiner _sizeDefiner;

        public Layouter(ICloudLayouter cloud, ISizeDefiner sizeDefiner)
        {
            this._cloud = cloud;
            this._sizeDefiner = sizeDefiner;
        }

        public IEnumerable<Result<Tag.Tag>> GetTags(Dictionary<string, int> wordFrequency)
        {
            var maxFrequency = wordFrequency.Values.Max();
            var minFrequency = wordFrequency.Values.Min();
            foreach (var item in wordFrequency)
            {
                var tagSize = _sizeDefiner.GetTagSize(item.Key, item.Value, minFrequency, maxFrequency);
                var locationRectangle = _cloud.PutNextRectangle(tagSize.RectangleSize);
                if (!locationRectangle.IsSuccess)
                {
                    yield return Result.Fail<Tag.Tag>(locationRectangle.Error);
                    yield break;
                }
                yield return new Tag.Tag(locationRectangle.Value, item.Key, tagSize.FontSize, item.Value);
            }
        }
    }
}