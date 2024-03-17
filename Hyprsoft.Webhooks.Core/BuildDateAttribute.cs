﻿using System;
using System.Globalization;

namespace Hyprsoft.Webhooks.Core
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class BuildDateAttribute : Attribute
    {
        public BuildDateAttribute(string value)
        {
            /* .csproj addition
             <ItemGroup>
                <AssemblyAttribute Include="Hyprsoft.Enp.Web.BuildDateAttribute">
                    <_Parameter1>$([System.DateTime]::UtcNow.ToString("yyyyMMddHHmmss"))</_Parameter1>
                </AssemblyAttribute>
            </ItemGroup>
            */
            DateTimeUtc = DateTime.ParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None);
        }

        public DateTime DateTimeUtc { get; }
    }
}
