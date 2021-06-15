namespace BindingAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Gherkin;
    using Gherkin.Ast;
    using System.Text.RegularExpressions;
    using MoreLinq;

    /// <summary>
    /// Counts how often a Given/When/Then attribute is used in feature files.
    /// </summary>
    public class GivenWhenThenCounter
    {
        /// <summary>
        /// The attributes found in the code.
        /// </summary>
        public IEnumerable<DiscoveredAttributeUsage> DiscoveredAttributes { get; set; }

        /// <summary>
        /// The full paths to the feature files to count usage of the attributes in <see cref="DiscoveredAttributes"/> in.
        /// </summary>
        public IEnumerable<string> FeatureFiles { get; set; }
        
        /// <summary>
        /// Result of the analysis. Contains the usage count per attribute.
        /// </summary>
        public IDictionary<DiscoveredAttributeUsage, int> BindingsUsage { get; private set; }
       
        /// <summary>
        /// Performs the analysis.
        /// </summary>
        /// <returns>this</returns>
        public GivenWhenThenCounter Analyze()
        {   
            if (null == DiscoveredAttributes) throw new InvalidOperationException($" {nameof(DiscoveredAttributes)} is null");
            if (null == FeatureFiles) throw new InvalidOperationException($" {nameof(FeatureFiles)} is null");

            BindingsUsage = DiscoveredAttributes.ToDictionary(x => x, x => 0);
            FeatureFiles.ForEach(f => AnalyzeFeatureFile(f));
            return this;
        }

        #region privates

        private string LastStepKeyWord = string.Empty;

        private void AnalyzeFeatureFile(string FullPath)
        {
            GherkinDocument gherkinDocument = new Parser().Parse(FullPath);
            gherkinDocument
                .Feature
                .Children
                .OfType<StepsContainer>()
                .ForEach(c => AnalyzeStepContainer(c))
            ;
        }

        private void AnalyzeStepContainer(StepsContainer c)
        {
            LastStepKeyWord = string.Empty;
            c.Steps.ForEach(step => AnalyzeStep(step));
        }
        private void AnalyzeStep(Step step)
        {
            if (step.Keyword.Trim().ToLower() != "and")
            {
                LastStepKeyWord = step.Keyword.Trim() + "Attribute";
            }
            string StepText = step.Text.Trim();

            DiscoveredAttributes
                .Where(attr => attr.Keyword == LastStepKeyWord)
                .Where(attr => Regex.IsMatch(StepText, attr.Text))
                .ForEach(attr => { BindingsUsage[attr] = BindingsUsage[attr] + 1; })
           ;
        }
        #endregion
    }
}
