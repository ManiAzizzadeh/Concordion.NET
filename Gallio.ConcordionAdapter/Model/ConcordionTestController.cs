using System;
using System.Collections.Generic;
using System.Linq;
using Gallio.Model.Filters;
using Gallio.Runtime.ProgressMonitoring;
using Gallio.Model;
using Gallio.ConcordionAdapter.Properties;
using System.Reflection;
using Concordion.Integration;
using Concordion.Internal;
using Gallio.Model.Commands;
using Gallio.Model.Helpers;
using Gallio.Model.Tree;
using Gallio.Model.Contexts;



namespace Gallio.ConcordionAdapter.Model
{
    /// <summary>
    /// Controls the execution of Concordion tests
    /// </summary>
    public class ConcordionTestController : TestController
    {
        private static readonly Dictionary<Type, ConcordionTestGroupTracker> testSuiteMap = new Dictionary<Type, ConcordionTestGroupTracker>();

        /// <inheritdoc />
        protected override TestResult RunImpl(ITestCommand rootTestCommand, TestStep parentTestStep, TestExecutionOptions options, IProgressMonitor progressMonitor)
        {
            IList<Test> allTheTest = parentTestStep.Test.Children;
            PopulateSuiteFixtureData(allTheTest, options);
            using (progressMonitor.BeginTask(Resources.ConcordionTestController_RunningConcordionTests, rootTestCommand.TestCount))
            {
                if (progressMonitor.IsCanceled)
                {
                    return new TestResult(TestOutcome.Canceled);
                }

                if (options.SkipTestExecution)
                {
                    return SkipAll(rootTestCommand, parentTestStep);
                }
                else
                {
                    return RunTest(rootTestCommand, parentTestStep, progressMonitor);
                }
            }
        }

        private static TestResult RunTest(ITestCommand testCommand, TestStep parentTestStep, IProgressMonitor progressMonitor)
        {
            Test test = testCommand.Test;
            progressMonitor.SetStatus(test.Name);

            TestResult result;
            ConcordionTest concordionTest = test as ConcordionTest;
            if (concordionTest == null)
            {
                result = RunChildTests(testCommand, parentTestStep, progressMonitor);
            }
            else
            {
                result = RunTestFixture(testCommand, concordionTest, parentTestStep);
            }

            progressMonitor.Worked(1);
            return result;
        }

        private static TestResult RunTestFixture(ITestCommand testCommand, ConcordionTest concordionTest, TestStep parentTestStep)
        {
           ITestContext testContext = testCommand.StartPrimaryChildStep(parentTestStep);
            
            // The magic happens here!
            var concordion = new ConcordionBuilder()
                                    .WithSource(concordionTest.Source)
                                    .WithTarget(concordionTest.Target)
                                    .WithSpecificationListener(new GallioResultRenderer())
                                    .Build();
            concordionTest.ProcessGroupStart();
            ConstructorInfo constructor = concordionTest.FixtureType.GetConstructor(Type.EmptyTypes);
            var fixture=constructor.Invoke(new object[]{});
            var summary = concordion.Process(concordionTest.Resource, fixture);
            bool passed = !(summary.HasFailures || summary.HasExceptions);
            concordionTest.ProcessGroupEnd();
            testContext.AddAssertCount((int)summary.SuccessCount + (int)summary.FailureCount);
            return testContext.FinishStep(passed ? TestOutcome.Passed : TestOutcome.Failed, null);
        }

        private static ConcordionBuilder CreateConcordionBuilder(Type builderType)
        {
            ConstructorInfo constructor = builderType.GetConstructor(Type.EmptyTypes);
            var builder = constructor.Invoke(new object[] { }) as ConcordionBuilder;
            return builder;
        }

        private static TestResult RunChildTests(ITestCommand testCommand, TestStep parentTestStep, IProgressMonitor progressMonitor)
        {
            ITestContext testContext = testCommand.StartPrimaryChildStep(parentTestStep);

            bool passed = true;
            foreach (ITestCommand child in testCommand.Children)
                passed &= RunTest(child, testContext.TestStep, progressMonitor).Outcome.Status == TestStatus.Passed;

            return testContext.FinishStep(passed ? TestOutcome.Passed : TestOutcome.Failed, null);
        }

        private static void PopulateSuiteFixtureData(IList<Test> allTest, TestExecutionOptions options)
        {
            if (testSuiteMap.Count == 0)
            {
                foreach (var test in allTest)
                {

                    var concordionTest = test as ConcordionTest;
                    if (concordionTest != null)
                    {
                        bool testIsIncluded = true;
                        if (!options.FilterSet.IsEmpty)
                        {
                            var result = options.FilterSet.Evaluate(test);
                            if (result != FilterSetResult.Include)
                            {
                                testIsIncluded = false;
                            }
                        }
                        if (testIsIncluded)
                        {
                            ConcordionTestGroupTracker groupTracker = TryGetGroupTracker(concordionTest.FixtureType);
                            if (groupTracker != null)
                            {
                                groupTracker.AddTest();
                                concordionTest.TestGroupTracker = groupTracker;
                            }
                        }
                    }
                }
            }
        }


        private static ConcordionTestGroupTracker TryGetGroupTracker(Type fixtureType)
        {
            ConcordionTestGroupTracker testGroupTracker = null;
            var attributes = fixtureType.GetCustomAttributes(true);
            foreach (object t in attributes)
            {
                var suiteAttribute = t as ConcordionTestGroupConfigAttribute;
                if (suiteAttribute != null)
                {
                    testGroupTracker = TryGetConcordionTestGroupFixture(suiteAttribute.TestGroupFixture);
                }
            }
            return testGroupTracker;
        }

        private static ConcordionTestGroupTracker TryGetConcordionTestGroupFixture(Type suiteType)
        {

            ConcordionTestGroupTracker holder = null;
            if (!testSuiteMap.TryGetValue(suiteType, out holder))
            {
                
                if (HasClassAttribute(suiteType, typeof(ConcordionTestGroupFixtureAttribute)))
                {
                    ConstructorInfo constructor = suiteType.GetConstructor(Type.EmptyTypes);
                    if (constructor != null)
                    {
                        var suite = constructor.Invoke(new Object[] { });
                        holder = new ConcordionTestGroupTracker(suite);
                    }
                    testSuiteMap.Add(suiteType, holder);
                    return holder;
                }
            }
            return holder;
        }

        private static bool HasClassAttribute(Type type, Type attributeType)
        {
            var attributes = type.GetCustomAttributes(true);
            if (attributes.Any(attribute => attribute.GetType().Equals(attributeType)))
            {
                return true;
            }
            return false;
        }
    }
}
