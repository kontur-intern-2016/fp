﻿using System.IO;
using TagCloud.Infrastructure;
using TagCloud.WordsProcessing;

namespace TagCloud.App
{
    public class AppSettings
    {
        public PictureConfig PictureConfig { get; set; }
        public FileInfo InputFileInfo { get; set; }
        public string OutputFilePath{ get; set; }
        public WordClassSettings WordClassSettings { get; set; }
        public string WordPainterAlgorithmName { get; set; }
    }
}
