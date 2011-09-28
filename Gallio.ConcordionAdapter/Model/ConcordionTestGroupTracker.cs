using System;
using System.Reflection;
using Concordion.Integration;

namespace Gallio.ConcordionAdapter.Model
{
    class ConcordionTestGroupTracker
    {
        public int RegisteredTestCount
        {
            get;
            private set;
        }

        private int SetupRunCounter
        {
            get;
            set;
        }

        private int TearDownRunCounter
        {
            get; 
            set; 
        }
        

        private object SuiteFixture;

        public ConcordionTestGroupTracker(object suiteFixture)
        {
            RegisteredTestCount = 0;
            SetupRunCounter = 0;
            this.SuiteFixture = suiteFixture;
        }


        ///<summary>
        /// Is any Setup to run? if so, runs it
        ///</summary>
        public void ProcessGroupStart()
        {
            if (SetupRunCounter == 0)
            {
                TryRunGroupStartMethod();
            }
            SetupRunCounter++;
            
        }

        private void TryRunGroupStartMethod()
        {

            var t = SuiteFixture.GetType();
            MethodInfo[] methods = t.GetMethods();
            foreach (MethodInfo m in methods)
            {
                foreach (Attribute attr in m.GetCustomAttributes(false))
                {
                    if (attr is TestGroupStartAttribute)
                    {
                        m.Invoke(SuiteFixture, new object[] {});
                    }
                }
            }

        }

        public void AddTest()
        {
            RegisteredTestCount++;
        }

        ///<summary>
        /// Is any TearDown to run? if so, runs it
        ///</summary>
        public void ProcessGroupEnd()
        {            
            TearDownRunCounter++;
            if (TearDownRunCounter == RegisteredTestCount)
            {
                TryRunGroupEndMethod();
            }
        }

        private void TryRunGroupEndMethod()
        {
            var t = SuiteFixture.GetType();
            MethodInfo[] methods = t.GetMethods();
            foreach (MethodInfo m in methods)
            {
                foreach (Attribute attr in m.GetCustomAttributes(false))
                {
                    if (attr is TestGroupEndAttribute)
                    {
                        m.Invoke(SuiteFixture, new object[] { });
                    }
                }
            }
        }
    }
}
