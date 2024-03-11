using System.Reflection;
using System.Runtime.InteropServices;

namespace Hyprsoft.Webhooks.Core
{
    public sealed record BuildInfo(string Version, DateTime BuildDateTimeUtc, string Framework)
    {
        public static BuildInfo FromAssembly(Assembly assembly)
        {
            var buildAttribute = assembly.GetCustomAttribute<BuildDateAttribute>();
            var informationalVersionAttribute = assembly.GetCustomAttribute(typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;
            return new BuildInfo
            (
                informationalVersionAttribute?.InformationalVersion ?? "0.0.0",
                buildAttribute?.DateTimeUtc ?? default,
                $"{RuntimeInformation.FrameworkDescription} {RuntimeInformation.RuntimeIdentifier}"
            );
        }
    }
}
