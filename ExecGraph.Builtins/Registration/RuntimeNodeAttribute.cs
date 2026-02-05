using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecGraph.Builtins.Registration
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class RuntimeNodeAttribute : Attribute
    {
        public string Name { get; }
        public RuntimeNodeAttribute(string name) => Name = name;
    }
}
