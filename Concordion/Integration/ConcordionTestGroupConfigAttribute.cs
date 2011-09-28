using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concordion.Integration
{   
    [AttributeUsage(AttributeTargets.Class)]
    public class ConcordionTestGroupConfigAttribute : Attribute
    {
        public Type TestGroupFixture { get; set; }
    }
}
