using ExecGraph.Builtins.Registration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecGraph.Builtins.DI
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddExecGraphBuiltins(this IServiceCollection services)
        {
            services.AddSingleton<NodeTypeRegistry>(sp =>
            {
                var reg = new NodeTypeRegistry();
                ExecGraph.Builtins.Registration.BuiltinRegistration.RegisterAll(reg);
                return reg;
            });

            return services;
        }
    }
}
