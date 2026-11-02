using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using WinArma.Models;

namespace WinArma.Services
{
    public class CalculatorService
    {
        // Only letters that can be part of a function / constant name reach the parser
        private static readonly Regex AllowedChars = new(@"^[\p{L}\d\s\+\-\*\/\^\(\)\.,%×÷]+$", RegexOptions.Compiled);

        // An expression needs an operator, or a function call like sin(30)
        private static readonly Regex LooksLikeMath = new(
            @"[\+\-\*\/\^%×÷]|\p{L}+\s*\(|\b(?:sqrt|kok|kök|abs|sin|cos|tan|asin|acos|atan|ln|log2|log|exp|round|floor|ceil)\s+[\d.,]",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex HyphenatedNumber = new(@"^\d{3,}(-\d+)+$", RegexOptions.Compiled);

        public SearchResultItem? TryCalculate(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return null;

            var trimmed = query.Trim();

            // 1. Unit conversion: "5 km mil", "100 c f", "2 gb to mb"
            if (UnitConverter.TryConvert(trimmed, out var converted, out var description))
            {
                return CreateCalculatorResult(converted, description);
            }

            // 2. Expression
            if (!AllowedChars.IsMatch(trimmed) || !LooksLikeMath.IsMatch(trimmed)) return null;

            // Dates and phone/order numbers (2024-05-10, 0555-123-45) are not subtractions
            if (HyphenatedNumber.IsMatch(trimmed)) return null;
            if (!ExpressionEvaluator.TryEvaluate(trimmed, out double numResult)) return null;

            // No digit grouping, so the copied answer pastes cleanly; keeps small values like 0.001
            string formatted = Math.Round(numResult, 10).ToString("0.##########", CultureInfo.CurrentCulture);

            return CreateCalculatorResult(formatted, trimmed);
        }

        private SearchResultItem CreateCalculatorResult(string answer, string originalExpr)
        {
            return new SearchResultItem
            {
                Title = $"= {answer}",
                Subtitle = $"{originalExpr} (Sonucu panoya kopyalamak için Enter)",
                Category = SearchResultCategory.Calculator,
                IconSymbol = "🧮",
                ActionTarget = answer,
                Score = 1200,
                CustomAction = () =>
                {
                    try
                    {
                        System.Windows.Clipboard.SetText(answer);
                    }
                    catch { }
                }
            };
        }
    }
}
