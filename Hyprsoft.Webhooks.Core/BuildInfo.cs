using System;
using System.Reflection;

namespace Hyprsoft.Webhooks.Core
{
    public class BuildInfo
    {
        public string Version { get; private set; }
        
        public DateTime BuildDateTimeUtc { get; private set; }

        public static BuildInfo FromAssembly(Assembly assembly)
        {
            var attribute = assembly.GetCustomAttribute<BuildDateAttribute>();
            return new BuildInfo
            {
                Version = (((AssemblyInformationalVersionAttribute)assembly.GetCustomAttribute(typeof(AssemblyInformationalVersionAttribute))).InformationalVersion),
                BuildDateTimeUtc = attribute?.DateTimeUtc ?? default
            };
        }
    }
}
