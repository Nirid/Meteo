using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meteo
{
     static class FolderManager
    {

        public static void CheckNewestWeather(Location location, DateTime lastUpdateDate)
        {
            var now = DateTime.Now;
            DateTime startDate;
            if (now.Hour > 18)
            {
                startDate = now.Date;
            }
            else if (now.Hour > 12)
            {
                startDate = now.Date.AddHours(-6);
            }
            else if (now.Hour > 6)
            {
                startDate = now.Date.AddHours(-12);
            }
            else
            {
                startDate = now.Date.AddHours(-18);
            }
            while (startDate < now)
            {
                var set = new FileSet(location, startDate, FileSet.DownloadStatus.ToBeDownloaded);
                if (startDate > lastUpdateDate && (FileHandler.FileList.Where(x => x.Location == set.Location && x.Date == set.Date).Count() == 0))
                    FileHandler.FileList.Add(set);
                startDate = startDate.AddHours(6);
            }
        }

        /// <summary>
        /// Checks folder for existing files and returns FileSets cooresponding to those files.
        /// </summary>
        /// <param name="path">Path to folder</param>
        public static IEnumerable<FileSet> CheckFolder(string path)
        {
            RemoveOutdatedFiles(path);
            var set = from file in Directory.GetFiles(path)
                      let data = FileSet.GetLocationAndDate(file)
                      where data != null && data.Value.location != null
                      select new FileSet(data.Value.location, data.Value.date, FileSet.DownloadStatus.Downloaded);

            return set;
        }

        /// <returns>FileSets cooresponding to damaged and outdated files</returns>
        /// <param name="path">Path to folder</param>
        private static IEnumerable<FileSet> GetOutdatedAndDamagedFiles(string path)
        {
            var allData = (from file in Directory.GetFiles(path)
                           let data = FileSet.GetLocationAndDate(file)
                           where data != null && data.Value.location != null
                           where (new FileInfo(file)).Length > 100000
                           select new FileSet(data.Value.location, data.Value.date, FileSet.DownloadStatus.Downloaded)).ToList();

            var duplicates = from data in allData
                             group data.Date by data.Location into all
                             select (all.Key, all.OrderByDescending(x => x).Skip(1)) into zipped
                             from date in zipped.Item2
                             select new FileSet(zipped.Key, date, FileSet.DownloadStatus.Downloaded);

            var outdated = from data in allData
                           where data.Date < DateTime.Now.AddDays(-2)
                           select data;

            var tooSmall = from file in Directory.GetFiles(path)
                           let data = FileSet.GetLocationAndDate(file)
                           where data != null && data.Value.location != null
                           where new FileInfo(file).Length < 100000
                           select new FileSet(data.Value.location, data.Value.date, FileSet.DownloadStatus.Downloaded);

            return duplicates.Concat(outdated.Concat(tooSmall)).ToList();
        }
        /// <summary>
        /// Removes outdated and damaged files
        /// </summary>
        /// <param name="path">Path to folder</param>
        public static void RemoveOutdatedFiles(string path)
        {
            foreach (var file in GetOutdatedAndDamagedFiles(path))
            {
                var set = FileHandler.FileList.Where(x => x == file);
                if (set.Count() != 0)
                {
                    set.Single().Status = FileSet.DownloadStatus.ToBeDeleted;
                }
                else
                {
                    try
                    {
                        File.Delete(FileSet.GetFilename(file));
                    }
                    catch (IOException Ex)
                    {
                        Logging.Log(Ex.ToString());
                    }
                }
            }

        }
    }
}
