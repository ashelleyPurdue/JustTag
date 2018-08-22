using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace JustTag.Tagging
{
    public enum SortMethod
    {
        name,
        date,
        comic,
        shuffle
    }

    public delegate IComparable SortFunction(TaggedFilePath f);
    public static class SortMethodExtensions
    {
        private static Dictionary<SortMethod, SortFunction> sorters = new Dictionary<SortMethod, SortFunction>();

        private static Random randGen = new Random();

        static SortMethodExtensions()
        {
            // Associate each value of the enum with a sort function
            sorters.Add(SortMethod.name, f => f.Name);
            sorters.Add(SortMethod.date, f => File.GetCreationTime(f.FullPath));
            sorters.Add(SortMethod.comic, ComicSort);
            sorters.Add(SortMethod.shuffle, f => randGen.Next());
        }

        /// <summary>
        /// Maps each enum value to a sort function
        /// </summary>
        /// <param name="sortMethod"></param>
        /// <returns></returns>
        public static SortFunction GetSortFunction(this SortMethod sortMethod) => sorters[sortMethod];

        /// <summary>
        /// A mode that sorts files by a number at the beginning of their name
        /// eg: 0.jpg < 1.jpg < 11.jpg
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        private static IComparable ComicSort(TaggedFilePath f)
        {
            // Extract the first number you can find out of the name

            var beforeDigits = f.Name.SkipWhile(c => !Char.IsDigit(c));     // Skip to the first digit
            var digits = beforeDigits.TakeWhile(c => Char.IsDigit(c));      // Go all the way up to the first non-digit

            // Try to parse it as an int
            string s = new string(digits.ToArray());
            int result;
            bool success = int.TryParse(s, out result);

            // If it doesn't have a number, just default it to a super-high number so it appears last
            if (!success)
                return int.MaxValue;

            return result;
        }
    }
}
