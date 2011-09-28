// Copyright 2009 Jeffrey Cameron
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using Concordion.Api;

namespace Concordion.Internal
{
    /// <summary>
    /// Parses the Concordion.config file and stores the results in a <see cref="ConcordionConfig"/> object
    /// </summary>
    public class ConcordionConfigParser
    {
        #region Properties

        /// <summary>
        /// Gets or sets the config.
        /// </summary>
        /// <value>The config.</value>
        public ConcordionConfig Config
        {
            get;
            private set;
        } 

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcordionConfigParser"/> class.
        /// </summary>
        /// <param name="config">The config.</param>
        public ConcordionConfigParser(ConcordionConfig config)
        {
            this.Config = config;
        } 

        #endregion

        #region Methods

        /// <summary>
        /// Parses the specified reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        public void Parse(TextReader reader)
        {
            var document = XDocument.Load(reader);
            LoadConfiguration(document);
        }

        /// <summary>
        /// Loads the configuration.
        /// </summary>
        /// <param name="document">The document.</param>
        private void LoadConfiguration(XDocument document)
        {
            var configElement = document.Root;

            if (configElement.Name == "Concordion")
            {
                LoadRunners(configElement);
            }
        }

        /// <summary>
        /// Loads the runners.
        /// </summary>
        /// <param name="element">The element.</param>
        private void LoadRunners(XElement element)
        {
            var runners = element.Element("Runners");

            if (runners != null)
            {
                foreach (var runner in runners.Elements("Runner"))
                {
                    var alias = runner.Attribute("alias");
                    var runnerTypeText = runner.Attribute("type");

                    if (alias != null && runnerTypeText != null)
                    {
                        var runnerType = Type.GetType(runnerTypeText.Value);

                        if (runnerType != null)
                        {
                            var runnerObject = Activator.CreateInstance(runnerType);
                            if (runnerObject != null && runnerObject is IRunner)
                            {
                                Config.Runners.Add(alias.Value, runnerObject as IRunner);
                            }
                        }
                    }
                }
            }
        } 

        #endregion
    }
}