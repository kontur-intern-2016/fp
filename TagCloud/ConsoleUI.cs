﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace WordCloudGenerator
{
    public class ConsoleUI : IUserInterface
    {
        private readonly IPainter.Factory painterFactory;
        private readonly IPalette.Factory paletteFactory;
        private readonly IPreparer.Factory preparerFactory;

        public ConsoleUI(IPreparer.Factory preparerFactory, IPalette.Factory paletteFactory,
            IPainter.Factory painterFactory)
        {
            this.preparerFactory = preparerFactory;
            this.paletteFactory = paletteFactory;
            this.painterFactory = painterFactory;
        }

        public void Run()
        {
            var text = AskFilePathAndRead();
            var (wordsToSkip, wordCount) = AskPreparerSettings();
            var preparer = preparerFactory(wordsToSkip);
            var frequencies = PerformAndReport("Обработка текста", () => preparer.CreateWordFreqList(text, wordCount));

            var algorithm = AskGenerationAlgorithm();
            var graphicStrings = PerformAndReport("Генерация облака", () => algorithm(frequencies));

            var palette = AskPalette();
            var font = AskFont();
            var painter = painterFactory(palette, font);

            var img = Result.RepeatUntilOk(() =>
            {
                var size = AskSize();
                return painter.Paint(graphicStrings, size);
            }, Console.WriteLine).Value;

            var pathToSave = AskUntilCorrect("путь по которому сохранить изображение", Saver.IsPathCorrect,
                "путь неверный");
            Saver.SaveImage(img, pathToSave);

            Console.WriteLine($"Изображение сохранено в {pathToSave}");
        }

        private string Ask(string what)
        {
            Console.WriteLine($"Введите {what}");

            return Console.ReadLine();
        }

        private string AskUntilCorrect(string what, Func<string, bool> isCorrect, string ifErr)
        {
            var read = Ask(what);
            while (!isCorrect(read))
            {
                Console.WriteLine($"{ifErr}, введите снова");
                read = Console.ReadLine();
            }

            return read;
        }

        private IEnumerable<T> RepeatUntil<T>(Func<T> get, Func<T, bool> shouldStop)
        {
            while (true)
            {
                var getted = get();
                if (shouldStop(getted))
                    yield break;

                yield return getted;
            }
        }

        private T PerformAndReport<T>(string actionName, Func<T> func)
        {
            Console.Write($"{actionName} выполняется");
            var result = func();
            ClearLine();
            Console.WriteLine($"{actionName} закончена");
            return result;
        }

        private void ClearLine()
        {
            Console.Write('\r' + new string(' ', Console.BufferWidth));
        }

        private string AskFilePathAndRead()
        {
            var readingResult = Result.RepeatUntilOk(() =>
            {
                var pathToText = Ask("полный путь к файлу с текстом");
                return Reader.ReadFile(pathToText);
            }, Console.WriteLine);

            return readingResult.Value;
        }

        private (IEnumerable<string> wordsToSkip, int wordCount) AskPreparerSettings()
        {
            var wordsToSkip = RepeatUntil(
                () => Ask("Введите слово которое нужно пропускать (пустая строка, чтобы закончить)"),
                string.IsNullOrEmpty);

            var wordCount = Result.RepeatUntilOk(() => int.Parse(Ask("колличество слов в облаке")),
                s => Console.WriteLine("Невозможно привести к целому числу"));
            return (wordsToSkip, wordCount.Value);
        }

        private Func<IEnumerable<WordFrequency>, IEnumerable<GraphicString>> AskGenerationAlgorithm()
        {
            var algoChoice = AskUntilCorrect(
                "какой алгоритм использовать - экспоненциальный или пропорциональный? (1/2)",
                str => str == "1" || str == "2", "нужно выбрать 1 или 2");
            return AlgorithmFabric.Create((AlgorithmType) int.Parse(algoChoice));
        }

        private FontFamily AskFont()
        {
            var fonts = FontFamily.Families;
            var fontResult = Result.RepeatUntilOk(() =>
            {
                var fontName = Ask("каким шрифтом рисовать").ToLower();
                return fonts.First(font => fontName.ToLower() == fontName);
            }, err => Console.WriteLine("Такого шрифта не существует"));

            return fontResult.Value;
        }

        private IPalette AskPalette()
        {
            var colorConverter = new ColorConverter();

            var bgColor = colorConverter.ConvertFromString(AskUntilCorrect("цвет фона",
                colorConverter.CanConvertFrom, "такого цвета не существует"));

            var colorsStr =
                RepeatUntil(() => AskUntilCorrect("цвета палитры (пустая строка чтобы прекратить)",
                    colorConverter.CanConvertFrom,
                    "такого цвета не существует"), string.IsNullOrEmpty);
            var mainColors = colorsStr.Select(colorConverter.ConvertFromString).Cast<Color>();

            var palette = paletteFactory(mainColors, (Color) bgColor);
            return palette;
        }

        private Size AskSize()
        {
            var way = AskUntilCorrect("задать размер самому, или посчитать автоматически (1/2)",
                s => s == "1" || s == "2", "нужно ввести 1 или 2");
            if (way == "2")
                return Size.Empty;

            var width = AskUntilCorrect("ширину", s => int.TryParse(s, out var n) && n > 0,
                "Должна быть числом, больше 0");

            var height = AskUntilCorrect("высоту", s => int.TryParse(s, out var n) && n > 0,
                "Должна быть числом, больше 0");

            return new Size(int.Parse(width), int.Parse(height));
        }
    }
}