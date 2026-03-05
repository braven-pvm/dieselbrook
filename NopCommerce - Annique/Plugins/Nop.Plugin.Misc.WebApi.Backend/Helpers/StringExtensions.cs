using System;
using System.Linq;

namespace Nop.Plugin.Misc.WebApi.Backend.Helpers
{
    public static class StringExtensions
    {
        public static int[] ToIdArray(this string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return Array.Empty<int>();

            var rezArray = ids.Split(";")
                .Where(s => int.TryParse(s, out _))
                .Select(int.Parse).ToArray();

            return rezArray;
        }

        public static string[] ToSkuArray(this string skuArray)
        {
            if (string.IsNullOrEmpty(skuArray))
                return Array.Empty<string>();

            var rezArray = skuArray.Replace(',',';').Split(";");

            return rezArray;
        }

        public static Guid[] ToGuidArray(this string guidsArray)
        {
            if (string.IsNullOrEmpty(guidsArray))
                return Array.Empty<Guid>();

            var rezArray = guidsArray.Replace(',', ';').Split(";")
                .Where(s => Guid.TryParse(s, out _))
                .Select(Guid.Parse).ToArray();

            return rezArray;
        }

        
    }
}
