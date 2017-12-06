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
        /// <summary>
        /// Checks folder for existing files and returns FileSets cooresponding to those files.
        /// </summary>
        /// <param name="path">Path to folder</param>
        public static IEnumerable<FileSet> CheckFolder(string path)
        {
            RemoveOutdatedFiles(path);
            var set = from file in Directory.GetFiles(Path)
                      let data = GetLocationAndDate(file)
                      where data != null && data.Value.location != null
                      select new FileSet(data.Value.location, data.Value.date, FileSet.DownloadStatus.Downloaded);

            return set;
        }

        /// <returns>FileSets cooresponding to damaged and outdated files</returns>
        /// <param name="path">Path to folder</param>
        private static IEnumerable<FileSet> GetOutdatedAndDamagedFiles(string path)
        {
            var allData = (from file in Directory.GetFiles(Path)
                           let data = GetLocationAndDate(file)
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

            var tooSmall = from file in Directory.GetFiles(Path)
                           let data = GetLocationAndDate(file)
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
                var set = FileList.Where(x => x == file);
                if (set.Count() != 0)
                {
                    set.Single().Status = FileSet.DownloadStatus.ToBeDeleted;
                }
                else
                {
                    try
                    {
                        File.Delete(GetFilename(file));
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
