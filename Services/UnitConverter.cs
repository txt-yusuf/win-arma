using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace WinArma.Services
{
    // "5 km mil", "100 c f", "2 gb to mb", "3 saat dk": offline unit conversion (no exchange rates, no network)
    public static class UnitConverter
    {
        private sealed record Unit(string Category, double ToBase, string Symbol);

        private static readonly Dictionary<string, Unit> Units = Build();

        private static readonly Regex Pattern = new(
            @"^(-?\d+(?:[.,]\d+)?)\s*([\p{L}°/]+)\s+(?:(?:to|->|=>|=|→|çevir|cevir)\s+)?([\p{L}°/]+)$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public static bool TryConvert(string input, out string answer, out string description)
        {
            answer = description = string.Empty;

            var m = Pattern.Match(input.Trim());
            if (!m.Success) return false;

            if (!double.TryParse(m.Groups[1].Value.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var amount))
            {
                return false;
            }

            if (!Units.TryGetValue(m.Groups[2].Value.ToLowerInvariant(), out var from)) return false;
            if (!Units.TryGetValue(m.Groups[3].Value.ToLowerInvariant(), out var to)) return false;
            if (from.Category != to.Category || from == to) return false;

            double result = from.Category == "temp"
                ? FromCelsius(to.Symbol, ToCelsius(from.Symbol, amount))
                : amount * from.ToBase / to.ToBase;

            if (double.IsNaN(result) || double.IsInfinity(result)) return false;

            string number = Math.Round(result, 6).ToString("0.######", CultureInfo.CurrentCulture);
            answer = $"{number} {to.Symbol}";
            description = $"{amount.ToString("0.######", CultureInfo.CurrentCulture)} {from.Symbol}";
            return true;
        }

        private static double ToCelsius(string symbol, double v) => symbol switch
        {
            "°F" => (v - 32) * 5 / 9,
            "K" => v - 273.15,
            _ => v
        };

        private static double FromCelsius(string symbol, double c) => symbol switch
        {
            "°F" => c * 9 / 5 + 32,
            "K" => c + 273.15,
            _ => c
        };

        private static Dictionary<string, Unit> Build()
        {
            var d = new Dictionary<string, Unit>(StringComparer.OrdinalIgnoreCase);

            void Add(string category, double toBase, string symbol, params string[] names)
            {
                var u = new Unit(category, toBase, symbol);
                d[symbol.ToLowerInvariant()] = u;
                foreach (var n in names) d[n] = u;
            }

            // length (base: metre)
            Add("len", 0.001, "mm", "milimetre");
            Add("len", 0.01, "cm", "santimetre", "santim");
            Add("len", 1, "m", "metre", "meter");
            Add("len", 1000, "km", "kilometre");
            Add("len", 0.0254, "in", "inç", "inc", "inch", "inches");
            Add("len", 0.3048, "ft", "feet", "foot", "fit");
            Add("len", 0.9144, "yd", "yard");
            Add("len", 1609.344, "mi", "mil", "mile", "miles");

            // mass (base: kilogram)
            Add("mass", 1e-6, "mg", "miligram");
            Add("mass", 0.001, "g", "gram");
            Add("mass", 1, "kg", "kilogram", "kilo");
            Add("mass", 1000, "ton", "t");
            Add("mass", 0.45359237, "lb", "lbs", "pound", "libre");
            Add("mass", 0.028349523125, "oz", "ounce");

            // volume (base: litre)
            Add("vol", 0.001, "ml", "mililitre");
            Add("vol", 1, "L", "lt", "litre", "liter");
            Add("vol", 3.785411784, "gal", "galon", "gallon");

            // data (base: byte)
            Add("data", 1, "B", "byte", "bayt");
            Add("data", 1024, "KB", "kib");
            Add("data", 1024.0 * 1024, "MB", "mib");
            Add("data", 1024.0 * 1024 * 1024, "GB", "gib");
            Add("data", 1024.0 * 1024 * 1024 * 1024, "TB", "tib");

            // time (base: second)
            Add("time", 0.001, "ms", "milisaniye");
            Add("time", 1, "sn", "s", "sec", "saniye", "second");
            Add("time", 60, "dk", "min", "dakika", "minute");
            Add("time", 3600, "sa", "h", "hr", "saat", "hour");
            Add("time", 86400, "gün", "gun", "d", "day");
            Add("time", 604800, "hafta", "week");

            // speed (base: m/s)
            Add("speed", 1, "m/s");
            Add("speed", 1000.0 / 3600, "km/h", "kmh", "kmsa");
            Add("speed", 0.44704, "mph");
            Add("speed", 0.514444, "knot", "kt", "deniz mili");

            // temperature (converted through Celsius)
            Add("temp", 1, "°C", "c", "celsius", "santigrat");
            Add("temp", 1, "°F", "f", "fahrenheit");
            Add("temp", 1, "K", "kelvin");

            return d;
        }
    }
}
