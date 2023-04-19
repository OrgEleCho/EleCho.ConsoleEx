using System;
using System.Globalization;

namespace EleCho.ConsoleUtilities
{
    internal static class GlobalizationUtils
    {
        private static CultureInfo ZhCn = new CultureInfo("zh-CN");
        private static CultureInfo ZhHk = new CultureInfo("zh-HK");
        private static CultureInfo ZhMO = new CultureInfo("zh-MO");
        private static CultureInfo ZhHK = new CultureInfo("zh-HK");
        private static CultureInfo ZhHans = new CultureInfo("zh-Hans");
        private static CultureInfo ZhHant = new CultureInfo("zh-Hant");

        public static bool IsZh(CultureInfo culture)
        {
            return culture.TwoLetterISOLanguageName.Equals("zh", StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool IsZhHans(CultureInfo culture)
        {
            return
                culture.Equals(ZhHans) ||
                culture.Equals(ZhCn);
        }

        public static bool IsZhHant(CultureInfo culture)
        {
            return
                culture.Equals(ZhHant) ||
                culture.Equals(ZhHK) ||
                culture.Equals(ZhMO) ||
                culture.Equals(ZhHK);
        }
    }
}
