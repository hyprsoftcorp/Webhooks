using Hangfire;
using System;

namespace Hyprsoft.Webhooks.Core.Hangfire
{
    internal class HangfireActivator : JobActivator
    {
        private readonly IServiceProvider _serviceProvider;

        public HangfireActivator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override object ActivateJob(Type type) => _serviceProvider.GetService(type);
    }
}
