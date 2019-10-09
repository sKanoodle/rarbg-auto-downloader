using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace RarbgAutoDownloader
{
    class BaseHandler
    {
        protected readonly HashSet<DateTime> CoveredDates = new HashSet<DateTime>();

        protected void AddFromFilePath(IEnumerable<string> paths)
        {
            foreach (string path in paths)
            {
                var date = Parse(Path.GetFileNameWithoutExtension(path));
                if (date.HasValue)
                    CoveredDates.Add(date.Value);
            }
        }

        private DateTime? Parse(string fileName)
        {
            var match = Regex.Match(fileName, @"[ \.](\d{2}\.\d{2}\.\d{2})[ \.]"); // match by date
            if (match.Success)
                return DateTime.ParseExact(match.Groups[1].Value, "yy.MM.dd", System.Globalization.CultureInfo.InvariantCulture);
            match = Regex.Match(fileName, @"[ \.]E(\d+)[ \.]"); // match by episode
            if (match.Success)
                return new DateTime(long.Parse(match.Groups[1].Value));
            return null;
        }

        public bool EpisodeWanted(string fileName)
        {
            var date = Parse(fileName);
            if (!date.HasValue)
                return true;
            if (CoveredDates.Contains(date.Value))
                return false;
            if (date.Value.AddYears(1) < DateTime.Now)
                return false;
            return true;
        }
    }

    class SeriesHandler : BaseHandler
    {
        public SeriesHandler(string basePath)
        {
            try
            {
                AddFromFilePath(Directory.EnumerateFiles(basePath));
            }
            catch (DirectoryNotFoundException) { }

        }
    }

    class ActorHandler : BaseHandler
    {
        public ActorHandler(string basePath, string actorName)
        {
            Console.WriteLine($"start search in directory for {actorName}");
            AddFromFilePath(Directory.GetFiles(basePath, $"*{actorName}*", SearchOption.AllDirectories));
            Console.WriteLine($"finish search in directory for {actorName}");
        }
    }
}
