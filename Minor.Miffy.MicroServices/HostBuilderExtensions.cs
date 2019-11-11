using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Miffy.MicroServices
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder ConfigureMicroServiceHostDefaults(this IHostBuilder builder, Action<IHostBuilder> configuration)
        {
            // TODO:

            return builder;
        }
    }
}
