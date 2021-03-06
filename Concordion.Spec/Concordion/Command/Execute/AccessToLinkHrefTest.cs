﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concordion.Integration;

namespace Concordion.Spec.Concordion.Command.Execute
{
    [ConcordionTest]
    public class AccessToLinkHrefTest
    {
        public bool fragmentSucceeds(string fragment) 
        {
            ProcessingResult result = new TestRig()
                .WithFixture(this)
                .ProcessFragment(fragment);
            
            return result.IsSuccess && result.SuccessCount > 0;
        }
    
        public string myMethod(string s) 
        {
            return s;
        }
    }
}
