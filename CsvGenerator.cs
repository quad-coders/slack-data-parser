using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// Credit
// https://twitter.com/utterbbq
// https://medium.com/@utterbbq/c-serializing-list-of-objects-to-csv-9dce02519f6b
public static class DelimitedGenerator
{
    public static string Generate<T>(IEnumerable<T> items, bool writeHeader) where T : class
    {
        var output = "";
        var delimiter = "\t";

        var properties = typeof(T).GetProperties()
            .Where(n => n.PropertyType == typeof(string)
                || n.PropertyType == typeof(bool)
                || n.PropertyType == typeof(char)
                || n.PropertyType == typeof(byte)
                || n.PropertyType == typeof(decimal)
                || n.PropertyType == typeof(int)
                || n.PropertyType == typeof(DateTime)
                || n.PropertyType == typeof(DateTime?));

        using (var sw = new StringWriter())
        {
            var header = properties
                .Select(n => n.Name)
                .Aggregate((a, b) => a + delimiter + b);

            if (writeHeader)
            {
                sw.WriteLine(header);
            }

            foreach (var item in items)
            {
                var row = properties
                    .Select(n => n.GetValue(item, null))
                    .Select(n => n == null ? "null" : n.ToString())
                    .Aggregate((a, b) => a + delimiter + b);

                sw.WriteLine(row);
            }
            output = sw.ToString();
        }

        return output;
    }
}