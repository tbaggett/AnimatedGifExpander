using Gifed;
using System;
using System.Drawing;
using System.IO;

namespace AnimatedGifExpander
{
    // Coded up by Tommy Baggett to take care of a hobby project I'm working on. 
    // I do real actual professional stuff, too! ;-)
    // http://tommyb.com, http://github.com/tbaggett
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("TommyB's Animated GIF Expander - tommyb.com, github.com/tbaggett/AnimatedGifExpander");
            Console.WriteLine("Converts variable FPS animated GIFs to fixed FPS GIFs for video conversion");
            Console.WriteLine("");

            try
            {
                if (args.Length == 3)
                {
                    var srcGifImagePath = args[0].ToString();
                    var destGifImagePath = args[1].ToString();
                    if (int.TryParse(args[2].ToString(), out int destFPS))
                    {
                        Console.WriteLine($"Converting \"{srcGifImagePath}\" to \"{destGifImagePath}\" at a fixed {destFPS} frames per second.");
                        // Check for wildcards
                        if (srcGifImagePath.Contains("*") || srcGifImagePath.Contains("?"))
                        {
                            var srcPath = Path.GetDirectoryName(srcGifImagePath);
                            if (string.IsNullOrWhiteSpace(srcPath))
                            {
                                srcPath = Directory.GetCurrentDirectory();
                            }
                            var srcFiles = Path.GetFileName(srcPath);
                            var di = new DirectoryInfo(srcPath);
                            var srcGifPaths = Directory.GetFiles(srcPath, Path.GetFileName(srcGifImagePath), SearchOption.TopDirectoryOnly);
                            foreach (var srcGifPath in srcGifPaths)
                            {
                                var destGifPath = Path.Combine(srcPath, Path.GetFileNameWithoutExtension(srcGifPath) + destGifImagePath + ".gif");

                                Console.WriteLine($"Converting \"{Path.GetFileName(srcGifPath)}\" to \"{Path.GetFileName(destGifPath)}\". ");

                                if (!Convert(srcGifPath, destGifPath, destFPS))
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            // No wildcards, just converting a single file
                            Convert(srcGifImagePath, destGifImagePath, destFPS);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Hmmm, looks like you didn't enter the expected parameters.");
                    Console.WriteLine("");
                    Console.WriteLine("Usage: AnimatedGifExpander {Source Animated GIF} {Destination Animated GIF} {Target FPS}");
                    Console.WriteLine("");
                    Console.WriteLine("       Source can include wildcards. Destination will be used as file name suffix if so.");
                    Console.WriteLine("");
                    Console.WriteLine("Example 1: AnimatedGifExpander MyAnimation.gif Fixed.gif 25");
                    Console.WriteLine("           Will look for a GIF file called MyAnimation.gif");
                    Console.WriteLine("           and write a new fixed 25 FPS GIF called Fixed.gif");
                    Console.WriteLine("");
                    Console.WriteLine("Example 2: AnimatedGifExpander *.gif -Fixed 25");
                    Console.WriteLine("           Will look for any GIF files in the current path");
                    Console.WriteLine("           and write new fixed 25 FPS GIFs called *-Fixed.gif");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Bummer. An error occurred while magically transforming your GIFs!");
                Console.WriteLine("");
                Console.WriteLine(ex.ToString());
            }
            finally { }

            Console.WriteLine("");
            Console.WriteLine("Press any key to exit.");
            Console.Read();
        }

        static bool Convert(string srcPath, string destPath, int destFPS)
        {
            var result = false;

            try
            {
                using (var srcGif = AnimatedGif.LoadFrom(srcPath))
                {
                    var fixedGifOffset = TimeSpan.FromMilliseconds(1000 / destFPS);
                    var crntSrcOffset = new TimeSpan();
                    var crntDestOffset = new TimeSpan();

                    using (var destGif = new AnimatedGif())
                    {
                        foreach (var srcFrame in srcGif)
                        {
                            crntSrcOffset += srcFrame.Delay;

                            do
                            {
                                crntDestOffset += fixedGifOffset;

                                var destFrame = (Image)srcFrame.Image.Clone();
                                destGif.AddFrame(destFrame, fixedGifOffset);
                            }
                            while (crntSrcOffset > crntDestOffset);
                        }

                        destGif.Save(destPath);

                        Console.WriteLine($"Successfully converted \"{Path.GetFileName(srcPath)}\" to \"{Path.GetFileName(destPath)}\". New file contains {destGif.FrameCount} frames.");

                        result = true;
                    }
                }
            }
            catch (Exception) { }

            return result;
        }
    }
}
