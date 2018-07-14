using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace JustTag
{
    public enum SortMethod
    {
        name,
        date,
        shuffle
    }

    public static class SortMethodExtensions
    {
        public delegate IComparable SortFunction(FileSystemInfo f);
        private static Dictionary<SortMethod, SortFunction> sorters = new Dictionary<SortMethod, SortFunction>();

        private static Random randGen = new Random();

        static SortMethodExtensions()
        {
            // Associate each value of the enum with a sort function
            sorters.Add(SortMethod.name, f => f.Name);
            sorters.Add(SortMethod.date, f => f.CreationTime);
            sorters.Add(SortMethod.shuffle, f => randGen.Next());
        }

        /// <summary>
        /// Maps each enum value to a sort function
        /// </summary>
        /// <param name="sortMethod"></param>
        /// <returns></returns>
        public static SortFunction GetSortFunction(SortMethod sortMethod) => sorters[sortMethod];
    }
}
