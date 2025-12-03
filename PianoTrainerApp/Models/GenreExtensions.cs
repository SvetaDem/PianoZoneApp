using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PianoTrainerApp.Models
{
    public static class GenreExtensions
    {
        public static string ToDisplayString(this Genre genre)
        {
            var field = genre.GetType().GetField(genre.ToString());
            var attr = field.GetCustomAttribute<DescriptionAttribute>();

            return attr?.Description ?? SplitCamelCase(genre.ToString());
        }

        private static string SplitCamelCase(string input)
        {
            return System.Text.RegularExpressions.Regex
                .Replace(input, "(\\B[A-Z])", " $1");
        }
    }
}
