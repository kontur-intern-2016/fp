﻿using TagCloud.Interfaces;
using TagCloud.IntermediateClasses;
using TagCloud.Layouter;
using TagCloud.Result;

namespace TagCloud
{
    public class LinearSizeScheme : ISizeScheme
    {
        public Result<Size> GetSize(FrequentedWord word)
        {
            var width = word.Word.Length * word.Frequency * 10 + 100;
            var height = word.Frequency * 10 + 50;
            return new Size(width, height);
        }
    }
}