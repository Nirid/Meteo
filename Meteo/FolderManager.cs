using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Globalization;
using System.IO;

namespace Meteo
{
    class FolderManager
    {
        public FolderManager(string path)
        {
            Path = path;
            
        }
    
        public readonly string Path;
        private static readonly CultureInfo IC = CultureInfo.InvariantCulture;

        public string GetName(Location location, DateTime date) => $"Date{date.ToString("yyyy-MM-dd-HH", IC)} Name{location.Name.ToString(IC)} X{location.X.ToString(IC)} Y{location.Y.ToString(IC)} Update{location.Update.ToString(IC)} .png]";
        
        public (Location location, DateTime date)? GetLocationAndDate(string str)
        {
            Regex regex = new Regex(@"Date(\d{4})-(\d{1,2})-(\d{1,2})-(\d{1,2}) Name(\S*) X(\d{2,3}) Y(\d{2,3}) Update(True|False) .png");
            Match match = regex.Match(str);
            if (!match.Success)
                return null;
            Location location = new Location(match.Groups[5].Value, Convert.ToInt32(match.Groups[6].Value, IC), Convert.ToInt32(match.Groups[7].Value, IC), Convert.ToBoolean(match.Groups[8].Value, IC));
            DateTime date = new DateTime(Convert.ToInt32(match.Groups[1].Value, IC), Convert.ToInt32(match.Groups[2].Value, IC), Convert.ToInt32(match.Groups[3].Value, IC), Convert.ToInt32(match.Groups[4].Value, IC), 0, 0);
            return (location, date);
        }

        private Dictionary<Location,DateTime> CheckFolder(string path)
        {
            var set = from file in Directory.GetFiles(Path)
                      let data = GetLocationAndDate(file)
                      where data != null && data.Value.location != null
                      select (data.Value.location, data.Value.date);

            return set.ToDictionary(x => x.location, x => x.date);
        }

        private void RemoveOutdatedFiles(string path)
        {
            var duplicates = from file in Directory.GetFiles(Path)
                             let data = GetLocationAndDate(file)
                             where data != null && data.Value.location != null
                             group data.Value.date by data.Value.location into all
                             orderby all descending
                             select (all.Key, all.Skip(1)) into zipped
                             let key = zipped.Key
                             from date in zipped.Item2
                             select (key, date);

            var outdated = from file in Directory.GetFiles(Path)
                           let data = GetLocationAndDate(file)
                           where data != null && data.Value.location != null
                           where data.Value.date < DateTime.Now.AddDays(-2)
                           select data.Value;

            foreach(var remove in duplicates.Concat(outdated))
            {
                File.Delete(Path + GetName(remove.Item1, remove.date));
            }

        }
    }
}
