using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CosmicMachine.Lang;
using CosmicMachine.VisualLang;

using NAudio.Wave;
using NAudio.Wave.SampleProviders;

using SixLabors.ImageSharp.Formats.Png;

namespace CosmicMachine
{
    public class MessageRendering
    {
        public static void RenderPages(string messageFilename, int scale, bool withWav, string outputDir)
        {
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);
            var messageDir = Path.GetDirectoryName(messageFilename)!;
            var message = File.ReadAllText(messageFilename);
            var pages = message.Replace("\r", "").Split("\n\n");
            for (int i = 0; i < pages.Length; i++)
            {
                var renderer = new RenderContext(Renderer.Instance);
                var lines = pages[i].Split("\n");
                var needBorder = false;
                foreach (var line in lines)
                {
                    try
                    {
                        if (line.StartsWith("//")) continue;
                        if (line.StartsWith("IMAGE_"))
                        {
                            var imageName = line.Split("_", 2)[1];
                            using var stream = new FileStream(Path.Combine(messageDir, imageName), FileMode.Open);
                            var pixels = CoreImplementations.ImageToPixels(stream);
                            renderer.AddPixels(pixels);
                        }
                        else
                        {
                            ReportIncorrectMessagesToConsole(line);
                            needBorder = true;
                            renderer.AddLine(line);
                        }

                    }
                    catch (Exception e)
                    {
                        throw new Exception(line, e);
                    }
                }
                if (renderer.Points.Count == 0)
                    throw new Exception(i.ToString());
                var humanReadableIndex = i + 1;
                var points = needBorder ? AddBorder(renderer.Points.ToList()).ToList() : renderer.Points;
                SaveImageFile(scale, points, Path.Combine(outputDir, $"message{humanReadableIndex}.png"));
                if (withWav && i == 0)
                    SaveAudioFile(points, Path.Combine(outputDir, $"message{humanReadableIndex}.wav"));
            }
        }

        private static void SaveImageFile(int scale, List<Vec> points, string filename)
        {
            using var image = new ImageRendered(scale).FromPoints(points);
            using var s = new FileStream(filename, FileMode.Create);
            image.Save(s, new PngEncoder());
        }

        private static void SaveAudioFile(List<Vec> points, string filename)
        {
            var width = points.Max(p => p.X)+1;
            var height = points.Max(p => p.Y)+1;
            var pixels = points.Select(p => p.Y*width + p.X).ToHashSet();

            var sineZero = new SignalGenerator(44100, 1)
            {
                Gain = 0.2,
                Frequency = 500,
                Type = SignalGeneratorType.Sin
            };
            var sineOne = new SignalGenerator(44100, 1)
            {
                Gain = 0.2,
                Frequency = 600,
                Type = SignalGeneratorType.Sin
            };
            var tick = TimeSpan.FromSeconds(0.05);
            var messageSampleProviders = Enumerable.Range(0, width * height).Select(i => pixels.Contains(i) ? sineOne.Take(tick) : sineZero.Take(tick));
            var combinedMessageSampleProvider = new ConcatenatingSampleProvider(messageSampleProviders);

            var backgroundNoiseSampleProvider = new SignalGenerator(44100, 1)
            {
                Gain = 0.1,
                Type = SignalGeneratorType.Pink
            }.Take(tick * width * height);
            var messageWithNoiseSampleProvider = new MixingSampleProvider(new List<ISampleProvider>{combinedMessageSampleProvider, backgroundNoiseSampleProvider});

            var paddingSampleProvider = new SignalGenerator(44100, 1)
            {
                Gain = 0.1,
                Type = SignalGeneratorType.Pink
            }.Take(TimeSpan.FromSeconds(3));
            var messageWithPaddingSampleProvider = new ConcatenatingSampleProvider(new List<ISampleProvider>{paddingSampleProvider, messageWithNoiseSampleProvider, paddingSampleProvider});

            WaveFileWriter.CreateWaveFile16(filename, messageWithPaddingSampleProvider);
        }

        private static IEnumerable<Vec> AddBorder(List<Vec> points)
        {
            var maxX = points.Max(p => p.X) + 1;
            var maxY = points.Max(p => p.Y) + 1;
            for (int x = 0; x <= maxX + 2; x++)
            {
                yield return new Vec(x, 0);
                yield return new Vec(x, maxY + 2);
            }
            for (int y = 0; y < maxY + 2; y++)
            {
                yield return new Vec(0, y);
                yield return new Vec(maxX + 2, y);
            }
            foreach (var point in points)
            {
                yield return point + new Vec(1, 1);
            }
        }

        private static void ReportIncorrectMessagesToConsole(string line)
        {
            try
            {
                var ex = ValidateWithValues(line, "1", "2", "3");
                if (ex == null)
                    return;
                ex = ValidateWithValues(line, "K", "F", "K");
                if (ex == null)
                    return;
                throw ex;
            }
            catch (Exception e)
            {
                Console.WriteLine(line + " -> " + e.Message);
            }
        }

        private static Exception? ValidateWithValues(string line, string x, string y, string z)
        {
            line = (" " + line + " ")
                   .Replace(" x ", $" {x} ").Replace(" y ", $" {y} ").Replace(" z ", $" {z} ")
                   .Replace(" x ", $" {x} ").Replace(" y ", $" {y} ").Replace(" z ", $" {z} ")
                   .Trim();
            //Console.WriteLine(line);
            var tokens = line.Split(" ");
            if (tokens.Contains("=") && tokens.Length > 1 && tokens[1] != "=" && !"Y ( ) computer chess decode drawAll ifzero".Split(" ").Any(v => tokens.Contains(v)))
            {
                try
                {
                    var valuesStr = line.Split(" = ", StringSplitOptions.RemoveEmptyEntries);
                    if (valuesStr[0].StartsWith("` encode"))
                    {
                        var values1 = valuesStr[0].Substring(2).Replace(" ", "_");
                        if (values1 != valuesStr[1])
                            throw new Exception($"WRONG! {values1} != {valuesStr[1]}");
                    }
                    else
                    {
                        var values = valuesStr.Select(Parser.ParseSKI).ToList();
                        if (values.Count == 2)
                        {
                            var e1 = values[0].Eval();
                            var e2 = values[1].Eval();
                            if (!IsEqual(e1, e2))
                                throw new Exception($"WRONG! {e1} != {e2}");
                        }
                    }
                }
                catch (Exception e)
                {
                    return e;
                }
            }
            return null;
        }

        private static bool IsEqual(Exp e1, Exp e2)
        {
            if (ReferenceEquals(e1, e2))
                return true;
            try
            {
                return (bool)(e1 == e2);
            }
            catch
            {
                // ReSharper disable once EmptyGeneralCatchClause
            }
            try
            {
                return (bool)e1 == (bool)e2;
            }
            catch
            {
                // ReSharper disable once EmptyGeneralCatchClause
            }

            if (e1 is IContinuation && e2 is IContinuation)
            {
                return IsEqual(e1.Call(2).Eval(), e2.Call(2).Eval());
            }
            return false;
        }
    }
}
