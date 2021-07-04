namespace Adrichem.Test.SpecFlowCodeAnalyzers.Common
{
    using System;
    using System.Collections.Generic;

    public static class ForEachExtension
    {
        /// <summary>
        /// Executes <paramref name="action"/> on each item in the input sequence.
        /// </summary>
        /// <param name="input">The input sequence.</param>
        /// <param name="action">The action to run for each element.</param>
        /// <exception cref="ArgumentNullException"/>
        public static void ForEach<T>(this IEnumerable<T> input, Action<T> action)
        {
            if(input == null) throw new ArgumentNullException(nameof(input));
            if (action == null) throw new ArgumentNullException(nameof(action));

            foreach(T item in input)
            {
                action(item);
            }
        }
    }
}
