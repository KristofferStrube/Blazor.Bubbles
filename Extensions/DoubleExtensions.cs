using System.Globalization;

namespace Blazor.Bubbles.Extensions
{
    public static class DoubleExtensions
    {
        public static string AsString(this double d)
        {
            return d.ToString(CultureInfo.InvariantCulture);
        }
    }
}
