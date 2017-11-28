using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meteo
{
    partial class FileHandler
    {
        public static IEnumerable<FileSet> CheckFolder(string path)
        {
            RemoveOutdatedFiles(path);
            var set = from file in Directory.GetFiles(Path)
                      let data = GetLocationAndDate(file)
                      where data != null && data.Value.location != null
                      select new FileSet(data.Value.location, data.Value.date, FileSet.DownloadStatus.Downloaded);

            return set;
        }

        private static IEnumerable<FileSet> GetOutdatedAndDamagedFiles(string path)
        {
            var allData = (from file in Directory.GetFiles(Path)
                           let data = GetLocationAndDate(file)
                           where data != null && data.Value.location != null
                           select new FileSet(data.Value.location, data.Value.date, FileSet.DownloadStatus.Downloaded)).ToList();

            var duplicates = from data in allData
                             group data.Date by data.Location into all
                             select (all.Key, all.OrderByDescending(x => x).Skip(1)) into zipped
                             let key = zipped.Key
                             from date in zipped.Item2
                             select new FileSet(zipped.Key, date, FileSet.DownloadStatus.Downloaded);

            var outdated = from data in allData
                           where data.Date < DateTime.Now.AddDays(-2)
                           select data;

            var tooSmall = from file in Directory.GetFiles(Path)
                           let data = GetLocationAndDate(file)
                           where data != null && data.Value.location != null
                           where new FileInfo(file).Length < 10000
                           select new FileSet(data.Value.location, data.Value.date, FileSet.DownloadStatus.Downloaded);

            return duplicates.Concat(outdated.Concat(tooSmall)).ToList();
        }

        public static void RemoveOutdatedFiles(string path)
        {
            foreach (var file in GetOutdatedAndDamagedFiles(path).Select(x => GetFilename(x)))
            {
                try
                {
                    File.Delete(file);
                }
                catch (IOException Ex)
                {
                    Logging.Log(Ex.ToString());
                }
            }

        }
    }
}
