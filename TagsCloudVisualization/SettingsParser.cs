﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DocoptNet;
using ikvm.extensions;
using TagsCloudVisualization.Providers.Layouter.Interfaces;
using TagsCloudVisualization.Providers.Sizable;
using TagsCloudVisualization.Results;
using TagsCloudVisualization.Settings;

namespace TagsCloudVisualization
{
    internal class SettingsParser
    {
        private static readonly List<string> parametersToUse = new List<string>
        {
            "--input", "--max_words", "--exclude", "--sizer", "--brush", "--back", "--font", "--font_size",
            "--width", "--height", "--x", "--y", "--coef", "--type", "--output_path", "--output_ext"
        };

        public static Result<ApplicationSettings> GetSettings(IDictionary<string, ValueObject> parameters)
        {
            if (parameters.Any(param => param.Value == null))
            {
                return Result.Fail<ApplicationSettings>(
                    $"{parameters.First(param => param.Value == null).Key} is null");
            }

            if (parametersToUse.Any(param => !parameters.Keys.Contains(param)))
            {
                return Result.Fail<ApplicationSettings>(
                    $"{parametersToUse.First(param => !parameters.Keys.Contains(param))} parameter not find");
            }

            var readerSettings = GetReaderSettings(parameters["--input"].ToString(),
                parameters["--max_words"].ToString(),
                parameters["--exclude"].ToString());
            if (!readerSettings.IsSuccess)
            {
                return Result.Fail<ApplicationSettings>(readerSettings.Error);
            }

            var drawerSettings = GetDrawerSettings(parameters["--brush"].ToString(),
                parameters["--back"].ToString(),
                parameters["--font"].ToString(),
                parameters["--font_size"].ToString(),
                parameters["--height"].ToString(),
                parameters["--width"].ToString(),
                parameters["--sizer"].ToString());
            if (!drawerSettings.IsSuccess)
            {
                return Result.Fail<ApplicationSettings>(drawerSettings.Error);
            }

            var layouterSettings = GetLayouterSettings(parameters["--x"].ToString(),
                parameters["--y"].ToString(),
                parameters["--coef"].ToString(),
                parameters["--type"].ToString());
            if (!layouterSettings.IsSuccess)
            {
                return Result.Fail<ApplicationSettings>(layouterSettings.Error);
            }


            var imageExt = Result.Of(() => (ImageExt) Enum.Parse(typeof(ImageExt),
                parameters["--output_ext"].ToString()));
            if (!imageExt.IsSuccess)
            {
                return Result.Fail<ApplicationSettings>("cant parse imageExt");
            }

            var savePath = parameters["--output_path"].ToString();

            if (savePath.isEmpty())
            {
                var res = PathHelper.ResourcesPath;
                if (!res.IsSuccess)
                    return Result.Fail<ApplicationSettings>("Can't get path to save Path");
                savePath = res.Value;
            }

            return new ApplicationSettings(imageExt.Value, readerSettings.Value, layouterSettings.Value,
                drawerSettings.Value, savePath);
        }

        private static Result<LayouterSettings> GetLayouterSettings(string x, string y, string spiralCoefficient,
            string spiralType)
        {
            if (!int.TryParse(x, out var xInt))
                return Result.Fail<LayouterSettings>("Cant parse x");

            if (!int.TryParse(y, out var yInt))
                return Result.Fail<LayouterSettings>("Cant parse y");

            var sType = (SpiralType) Enum.Parse(typeof(SpiralType), spiralType);
            if (!int.TryParse(spiralCoefficient, out var spiralCoefficientInt))
                return Result.Fail<LayouterSettings>("Cant parse spiral coefficient");

            return new LayouterSettings(new Point(xInt, yInt), spiralCoefficientInt, sType);
        }

        private static Result<ReaderSettings> GetReaderSettings(string textDirectory, string maxObjectCount,
            string badWordsDirectory)
        {
            var resources = PathHelper.ResourcesPath;
            if (!resources.IsSuccess)
            {
                return Result.Fail<ReaderSettings>(resources.Error);
            }

            if (!int.TryParse(maxObjectCount, out var count))
                return Result.Fail<ReaderSettings>("cant parse max words count");
            badWordsDirectory =
                string.IsNullOrEmpty(badWordsDirectory)
                    ? resources.Value + "\\BadWords.txt"
                    : badWordsDirectory;
            textDirectory = string.IsNullOrEmpty(textDirectory)
                ? resources.Value + "\\HarryPotter.txt"
                : textDirectory;
            return new ReaderSettings(textDirectory, count, badWordsDirectory);
        }

        private static Result<DrawerSettings> GetDrawerSettings(string brushColorName, string backGroundColorName,
            string fontName, string fontSize, string imageHeight, string imageWidth, string sizer)
        {
            if (!int.TryParse(fontSize, out var size))
                return Result.Fail<DrawerSettings>("Cant parse fontSize");
            if (!int.TryParse(imageHeight, out var height) || height > 15000)
                return Result.Fail<DrawerSettings>("Cant parse height");
            if (!int.TryParse(imageWidth, out var width) || width > 15000)
                return Result.Fail<DrawerSettings>("Cant parse width");

            var brushColor = Color.FromName(brushColorName);
            var backGroundColor = Color.FromName(backGroundColorName);

            var textBrush = new SolidBrush(brushColor);
            var font = Result.Of(() => new Font(fontName, size));
            if (!font.IsSuccess)
                return Result.Fail<DrawerSettings>("Cant parse font");

            var sizerType = Result.Of(() => (SizeSelectorType) Enum.Parse(typeof(SizeSelectorType), sizer));
            if (!sizerType.IsSuccess)
                return Result.Fail<DrawerSettings>("Cant parse sizerType");

            return new DrawerSettings(textBrush, backGroundColor, font.Value, height, width, sizerType.Value);
        }
    }
}