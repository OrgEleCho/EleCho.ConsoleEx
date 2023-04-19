using System;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace EleCho.ConsoleUtilities
{
    internal class GlobalizationStrings
    {
        private static Assembly CurrentAssembly = typeof(GlobalizationStrings).Assembly;

        private static readonly Lazy<ResourceManager> LaziedDefaultResourceManager =
            new Lazy<ResourceManager>(() => new ResourceManager("EleCho.ConsoleUtilities.Globalization.Strings", CurrentAssembly));
        private static readonly Lazy<ResourceManager> LaziedChineseResourceManager =
            new Lazy<ResourceManager>(() => new ResourceManager("EleCho.ConsoleUtilities.Globalization.StringsZh", CurrentAssembly));

        public static CultureInfo CurrentCulture { get; set; } = CultureInfo.CurrentCulture;


        public static ResourceManager ResourceManager
        {
            get
            {
                if (GlobalizationUtils.IsZh(CurrentCulture))
                    return LaziedChineseResourceManager.Value;

                return LaziedDefaultResourceManager.Value;
            }
        }

        public static string InvalidInput =>
            ResourceManager.GetString(nameof(InvalidInput)) ?? string.Empty;

        public static string PressAnyKeyToContinue => 
            ResourceManager.GetString(nameof(PressAnyKeyToContinue)) ?? string.Empty;

        public static string SelectAnOption =>
            ResourceManager.GetString(nameof(SelectAnOption)) ?? string.Empty;

        public static string EnterAString =>
            ResourceManager.GetString(nameof(EnterAString)) ?? string.Empty;

        public static string EnterAnInteger =>
            ResourceManager.GetString(nameof(EnterAnInteger)) ?? string.Empty;

        public static string EnterANumber =>
            ResourceManager.GetString(nameof(EnterANumber)) ?? string.Empty;

        public static string EnterADateTime =>
            ResourceManager.GetString(nameof(EnterADateTime)) ?? string.Empty;

        public static string EnterATimeSpan =>
            ResourceManager.GetString(nameof(EnterATimeSpan)) ?? string.Empty;

        public static string EnterAnIntegerToSelectAnOption =>
            ResourceManager.GetString(nameof(EnterAnIntegerToSelectAnOption)) ?? string.Empty;

        public static string EnterAnIntegerInSpecifiedRangeToSelectAnOption =>
            ResourceManager.GetString(nameof(EnterAnIntegerInSpecifiedRangeToSelectAnOption)) ?? string.Empty;
    }
}
