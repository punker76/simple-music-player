using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using SchwabenCode.QuickIO;
using Splat;
using TagLib;

namespace SimpleMusicPlayer.Core
{
    public class CoverManager : IEnableLogger
    {
        public CoverManager()
        {
        }

        public BitmapImage GetImageFromFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || !QuickIOFile.Exists(fileName))
            {
                return null;
            }
            // try getting the cover by picture tag
            var image = GetImageFromPictureTag(fileName);
            // if no cover was found try getting the cover from disk
            return image ?? GetImageFromDirectory(fileName);
        }

        private static BitmapImage GetImageFromPictureTag(string fileName)
        {
            try
            {
                using (var file = TagLib.File.Create(fileName))
                {
                    var pictures = file.Tag.Pictures;
                    if (pictures != null)
                    {
                        var pic = pictures.FirstOrDefault(p => p.Type == PictureType.FrontCover
                                                               || p.Type == PictureType.BackCover
                                                               || p.Type == PictureType.FileIcon
                                                               || p.Type == PictureType.OtherFileIcon
                                                               || p.Type == PictureType.Media
                                                               || p.Type == PictureType.Other);
                        if (pic != null)
                        {
                            var bi = new BitmapImage();
                            bi.BeginInit();
                            bi.CreateOptions = BitmapCreateOptions.DelayCreation;
                            bi.CacheOption = BitmapCacheOption.OnDemand;
                            bi.StreamSource = new MemoryStream(pic.Data.Data);
                            bi.EndInit();
                            bi.Freeze();
                            return bi;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Fail to load cover from picture tag: {0}, {1}", fileName, e);
            }
            return null;
        }

        private static BitmapImage GetImageFromDirectory(string fileName)
        {
            try
            {
                var path2Image = Path.GetDirectoryName(fileName);
                if (string.IsNullOrEmpty(path2Image))
                {
                    return null;
                }

                var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {".jpg", ".jpeg", ".png", ".bmp"};
                var regexCoverFiles = new Regex(".*(folder|cover|front|back|band|artist).*", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                var allPossibleCoverFiles = QuickIODirectory.EnumerateFiles(path2Image).Where(f => extensions.Contains(Path.GetExtension(f.Name))).ToList();
                var cover = allPossibleCoverFiles.FirstOrDefault(f => regexCoverFiles.IsMatch(f.Name));
                cover = cover ?? allPossibleCoverFiles.FirstOrDefault();
                if (cover != null)
                {
                    var bi = new BitmapImage();
                    bi.BeginInit();
                    bi.CreateOptions = BitmapCreateOptions.DelayCreation;
                    bi.CacheOption = BitmapCacheOption.OnDemand;
                    bi.UriSource = new Uri(cover.FullName, UriKind.RelativeOrAbsolute);
                    bi.EndInit();
                    bi.Freeze();
                    return bi;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Fail to load cover from directory: {0}, {1}", fileName, e);
            }
            return null;
        }
    }
}