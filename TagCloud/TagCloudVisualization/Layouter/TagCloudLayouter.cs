﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TagCloud.Settings;
using TagCloud.TagCloudVisualization.Analyzer;
using TagCloud.TagCloudVisualization.Extensions;

namespace TagCloud.TagCloudVisualization.Layouter
{
    public class TagCloudLayouter : ITagCloudLayouter
    {
        private readonly FontSettings fontSettings;
        private readonly SettingsLoader settingsLoader;
        private Rectangle AvailableCloudArea => new Rectangle(-settingsLoader.ImageSettings.Width/2, 
                                                                -settingsLoader.ImageSettings.Height/2, 
                                                                settingsLoader.ImageSettings.Width, 
                                                                settingsLoader.ImageSettings.Height);
        private readonly ICloudLayouter layouter;
        private int maxFrequency;
        private int minFrequency;

        public TagCloudLayouter(FontSettings fontSettings,
                    ICloudLayouter layouter, SettingsLoader settingsLoader)
        {
            this.fontSettings = fontSettings;
            this.layouter = layouter;
            this.settingsLoader = settingsLoader;
        }

        public Result<IEnumerable<Tag>> GetCloudTags(Dictionary<string, int> weightedWords)
        {
            layouter.Clear();
            if (weightedWords.Count == 0) 
                return Result.Ok(Enumerable.Empty<Tag>());

            minFrequency = weightedWords.Values.Min();
            maxFrequency = weightedWords.Values.Max();

            return Result.Of(() => weightedWords.Select(GenerateTag));
        }

        
        private Tag GenerateTag(KeyValuePair<string, int> weightedWord)
        {
            var fontSize = GetFontSize(weightedWord.Value);
            var font = new Font(fontSettings.FontFamily, fontSize);
            var frameSize = TextRenderer.MeasureText(weightedWord.Key, font);
            var frame = layouter.PutNextRectangle(frameSize);
            if (!AvailableCloudArea.Contains(frame))
                throw new Exception("TagCloud doesn't fit in given picture sizes. Try to reduce the font sizes");
            return new Tag(weightedWord.Key, font, frame);
        }

        private int GetFontSize(int currentWeight)
        {
            var medianFrequency = maxFrequency - minFrequency;
            var medianFontSize = fontSettings.MaxFontSize - fontSettings.MinFontSize;
            var fontScaler = maxFrequency == minFrequency ? 1 : (double) currentWeight / medianFrequency;
            return (int) Math.Round(fontSettings.MinFontSize + fontScaler * medianFontSize);
        }
    }
}