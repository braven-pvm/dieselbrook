using System;
namespace XcellenceIT.Plugin.ProductRibbons
{
    class AssemblyAttributes
    {
        [AttributeUsage(AttributeTargets.Assembly)]
        public sealed class BuildDateAttribute : Attribute
        {
            public BuildDateAttribute(string value)
            {
                DateTime = Convert.ToDateTime(value);
            }
            public DateTime DateTime { get; }
        }
    }
}

