using System;
using System.Globalization;
using System.Reflection;

[assembly: AssemblyVersion("0.4")]

namespace SDK
{
    public class AssemblyInfo
    {
        private const string BuildVersionMetadataPrefix = "+build";
        private const string dateFormat = "yyyy-MM-ddTHH:mm:ss:fffZ";

        public static DateTime GetLinkerTime(Assembly assembly)
        {
            var attribute = assembly
              .GetCustomAttribute<AssemblyInformationalVersionAttribute>();

            if (attribute?.InformationalVersion != null)
            {
                var value = attribute.InformationalVersion;
                var index = value.IndexOf(BuildVersionMetadataPrefix);
                if (index > 0)
                {
                    value = value[(index + BuildVersionMetadataPrefix.Length)..];

                    return DateTime.ParseExact(
                        value,
                      dateFormat,
                      CultureInfo.InvariantCulture);
                }
            }
            return default;
        }
    }
}
