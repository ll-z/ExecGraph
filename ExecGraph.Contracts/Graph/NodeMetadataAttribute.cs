using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecGraph.Contracts.Graph
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class NodeMetadataAttribute : Attribute
    {
        public string EditorNodeType { get; }
        public string DisplayName { get; }


        public NodeMetadataAttribute(string editorNodeType, string displayName)
        {
            EditorNodeType = editorNodeType;
            DisplayName = displayName;
        }
    }
}
