using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JustTag
{
    /// <summary>
    /// A stack, except you can move backwards and fowards
    /// If you push something after moving backwards, everything
    /// after the position will be forgotten.
    /// 
    /// It's just like the forward/back buttons on your web browser.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NavigationStack<T>
    {
        /// <summary>
        /// Returns the item at the current position
        /// </summary>
        /// <returns></returns>
        public T Current
        {
            get { return items[currentPos]; }
        }

        /// <summary>
        /// True if MoveForward() will do anything
        /// </summary>
        public bool HasNext { get { return currentPos != items.Count - 1; } }

        /// <summary>
        /// True if MoveBack() will do anything
        /// </summary>
        public bool HasPrev { get { return currentPos != 0; } }

        private List<T> items = new List<T>();
        private int currentPos = 0;

        public NavigationStack(T firstItem)
        {
            items.Add(firstItem);
        }

        /// <summary>
        /// Returns the previous item and rewinds.
        /// If there are no previous items, returns the current item
        /// </summary>
        /// <returns></returns>
        public T MoveBack()
        {
            if (currentPos != 0)
                currentPos--;

            return items[currentPos];
        }

        /// <summary>
        /// Returns the next item and fast-forwards
        /// If there is no next item, returns the current item.
        /// </summary>
        /// <returns></returns>
        public T MoveForward()
        {
            if (currentPos != items.Count - 1)
                currentPos++;

            return items[currentPos];
        }

        /// <summary>
        /// Adds a new item after the current position
        /// All items after the current position are forgotten,
        /// like in a web browser
        /// </summary>
        /// <param name="item"></param>
        public void Push(T item)
        {
            // Remove all items after currentPos
            while (items.Count - 1 > currentPos)
                items.RemoveAt(items.Count - 1);

            // Add the new item
            items.Add(item);
            currentPos++;
        }

        /// <summary>
        /// Prints all items
        /// A carrot  is added to the line with the current position
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            for (int i = 0; i < items.Count; i++)
            {
                // Add a carrot if this is the current pos
                if (i == currentPos)
                    builder.Append(">");

                // Add the item
                builder.AppendLine(items[i].ToString());
            }

            return builder.ToString();
        }
    }
}
