using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using TagCloud.Infrastructure.Settings.SettingsProviders;
using TagCloud.Infrastructure.Text.Information;

namespace TagCloud.Infrastructure.Settings.UISettingsManagers
{
    public class ColorPickerSettingsManager : IMultiOptionsManager
    {
        private readonly Regex regex;
        private readonly Func<IColorPickerSettingProvider> colorSettings;
        private readonly ColorConverter colorConverter;

        public ColorPickerSettingsManager(Func<IColorPickerSettingProvider> colorSettings, ColorConverter colorConverter)
        {
            this.colorSettings = colorSettings;
            this.colorConverter = colorConverter;
            regex = new Regex(@"\s*(?<type>.*?)\s*=\s*(?<color>.*?)\s*(;|$)");
        }

        private static IEnumerable<WordType> SupportedWordTypes =>
            Enum.GetValues(typeof(WordType)).Cast<WordType>();

        private static IEnumerable<Color> SupportedColors =>
            Enum.GetValues(typeof(KnownColor))
                .Cast<KnownColor>()
                .Select(Color.FromKnownColor);
        public string Title => "Color";
        public string Help => $"Choose color for word types {string.Join(" ", SupportedWordTypes)}";

        public Result<string> TrySet(string input)
        {
            var inputDictionary = ParseInput(input);
            return ProcessInput(inputDictionary);
        }

        private Dictionary<string, string> ParseInput(string input) =>
            regex.Matches(input)
                .OfType<Match>()
                .Select(m => (m.Groups["type"].Value, m.Groups["color"].Value))
                .GroupBy(pair => pair.Item1, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.Last().Item2);

        private Result<string> ProcessInput(Dictionary<string, string> inputDictionary) =>
            Result.Of(() =>
                {
                    var failed = new List<string>();
                    foreach (var typeName in inputDictionary.Keys)
                        Result.Of(() => ProcessPair(typeName, inputDictionary).GetValueOrThrow())
                            .OnFail(s => failed.Add(s));
                    return failed;
                }
            ).Then(failed => failed.Any()
                ? Result.Fail<string>(string.Join("\n", failed))
                : Result.Ok(""));

        private Result<string> ProcessPair(string typeName, Dictionary<string, string> inputDictionary) =>
            Result.Of(() => (WordType) Enum.Parse(typeof(WordType), typeName))
                .ReplaceError(s=>$"Unknown WordType {typeName}")
                .Then(typeResult =>
                {
                    var inputColor = inputDictionary[typeName];
                    var color = (Color) colorConverter.ConvertFromString(inputColor);
                    return (typeResult, color);
                })
                .ReplaceError(s => $"{s}. Unknown Color {inputDictionary[typeName]}")
                .Then(pair =>
                {
                    var (type, color) = pair;
                    // Console.Out.WriteLine("colorSettings() = {0}", colorSettings().ColorMap[type]);
                    colorSettings().ColorMap[type] = color;
                    // Console.Out.WriteLine("colorSettings() = {0}", colorSettings().ColorMap[type]);
                    return Result.Ok("");
                });

        public string Get() =>
            string.Join("; ", colorSettings().ColorMap.Select(pair => $"{pair.Key} = {colorConverter.ConvertToString(pair.Value.Name)}"));

        public Dictionary<string, IEnumerable<string>> GetOptions() =>
            SupportedWordTypes.ToDictionary(
                type => type.ToString(),
                type => SupportedColors.Select(color => colorConverter.ConvertToString(color.Name)));

        public Dictionary<string, string> GetSelectedOptions()
        {
            return colorSettings().ColorMap.ToDictionary(
                kv => kv.Key.ToString(),
                pair => colorConverter.ConvertToString(pair.Value.Name));
        }
    }
}