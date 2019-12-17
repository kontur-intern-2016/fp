﻿using System.Drawing;
using TagsCloudGenerator.Attributes;
using TagsCloudGenerator.Interfaces;
using FailuresProcessing;
using TagsCloudGenerator.DTO;
using System.Linq;

namespace TagsCloudGenerator.Painters
{
    [Factorial("PainterWithUserColors")]
    public class PainterWithUserColors : IPainter
    {
        private readonly IPainterSettings painterSettings;

        public PainterWithUserColors(IPainterSettings painterSettings) =>
            this.painterSettings = painterSettings;

        public Result<None> DrawWords(WordDrawingDTO[] layoutedWords, Graphics graphics)
        {
            return
                GetUserColors()
                .Then(CheckOnKnownColors)
                .Then(CheckOnColorsCount)
                .Then(userColors =>
                {
                    ResetCounter();
                    graphics.Clear(userColors.backgroundColor);
                    using (var solidBrush = new SolidBrush(Color.Black))
                        foreach (var wordDrawing in layoutedWords)
                        {
                            solidBrush.Color = userColors.colors[GetNextNotNegativeNumber(userColors.colors.Length)];
                            var drawingResult =
                                CheckFont(wordDrawing.FontName)
                                .Then(n => Result.Ok(new Font(wordDrawing.FontName, wordDrawing.MaxFontSymbolWidth)))
                                .ThenWithDisposableObject(font =>
                                {
                                    graphics.DrawString(wordDrawing.Word, font, solidBrush, wordDrawing.WordRectangle);
                                    return Result.Ok();
                                });
                            if (!drawingResult.IsSuccess)
                                return drawingResult;
                        }
                    return Result.Ok();
                })
                .RefineError($"{nameof(PainterWithUserColors)} failure");
        }

        private Result<(Color backgroundColor, Color[] colors)> GetUserColors() =>
            Result.Ok(
                (Color.FromName(painterSettings.BackgroundColorName),
                painterSettings.ColorsNames
                    .Select(colorName => Color.FromName(colorName))
                    .ToArray()));

        private Result<(Color backgroundColor, Color[] colors)> CheckOnKnownColors(
            (Color backgroundColor, Color[] colors) userColors)
        {
            var unknownColorsNames = userColors.colors
                .Append(userColors.backgroundColor)
                .Where(color => !color.IsKnownColor)
                .Aggregate(
                    string.Empty,
                    (str, color) => $"{str} {$"\'{color.Name}\'"}");
            return
                unknownColorsNames.Length == 0 ?
                Result.Ok(userColors) :
                Result.Fail<(Color backgroundColor, Color[] colors)>($"Unknown colors:{unknownColorsNames}");
        }

        private Result<(Color backgroundColor, Color[] colors)> CheckOnColorsCount(
            (Color backgroundColor, Color[] colors) userColors) =>
            userColors.colors.Length > 0 ?
            Result.Ok(userColors) :
            Result.Fail<(Color backgroundColor, Color[] colors)>("Number of colors must be greater than 0");

        private Result<None> CheckFont(string userFontName) =>
            Result.Ok(new Font(userFontName, 10))
            .ThenWithDisposableObject(font =>
                font.Name.ToLower() == userFontName.ToLower() ?
                Result.Ok() :
                Result.Fail<None>($"Font with name \'{userFontName}\' not found in system"));


        private int colorsCounter = 0;
        private int GetNextNotNegativeNumber(int maxValue) => colorsCounter = ++colorsCounter % maxValue;
        private void ResetCounter() => colorsCounter = 0;
    }
}