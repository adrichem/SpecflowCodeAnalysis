namespace Adrichem.Test.SpecFlowCodeAnalyzers.Common
{
    public static class SpecFlowCodeAnalyzersDiagnosticIds
    {
        public const string InvalidRegEx = "SPECFLOW0001";
        public const string MustBePublicMethod = "SPECFLOW0002";
        public const string ForbiddenModifier = "SPECFLOW0003";
        public const string MustBePublicClass = "SPECFLOW0004";
        public const string BindingAttributeMissing = "SPECFLOW0005";
        public const string DoNotDuplicateStepText = "SPECFLOW0006";
        public const string RegExContainsBannedWord = "SPECFLOW0007";
        public const string RegExContainsBannedWordInvalidFileFormat = "SPECFLOW0008";
    }
}
