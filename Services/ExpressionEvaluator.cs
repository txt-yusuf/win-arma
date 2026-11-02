using System;
using System.Collections.Generic;
using System.Globalization;

namespace WinArma.Services
{
    // Small recursive-descent calculator: + - * / ^ mod(%) percent, parentheses, constants and functions.
    // Trig functions work in degrees. "100 + 10%" means 110 (percent of the left operand), "10%" alone is 0.1.
    public static class ExpressionEvaluator
    {
        private enum T { Num, Id, Plus, Minus, Mul, Div, Pow, Pct, LP, RP, End }

        private readonly record struct Token(T Type, double Num, string Text);

        public static bool TryEvaluate(string input, out double value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(input) || input.Length > 200) return false;

            try
            {
                var tokens = Tokenize(input);
                if (tokens == null) return false;

                var p = new Parser(tokens);
                var result = p.ParseExpression();
                if (!p.AtEnd) return false;
                if (double.IsNaN(result) || double.IsInfinity(result)) return false;

                value = result;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static List<Token>? Tokenize(string s)
        {
            var list = new List<Token>();
            int i = 0;

            while (i < s.Length)
            {
                char c = s[i];

                if (char.IsWhiteSpace(c)) { i++; continue; }

                if (char.IsDigit(c) || ((c == '.' || c == ',') && i + 1 < s.Length && char.IsDigit(s[i + 1])))
                {
                    int start = i;
                    bool sep = false;
                    while (i < s.Length && (char.IsDigit(s[i]) || ((s[i] == '.' || s[i] == ',') && !sep)))
                    {
                        if (s[i] == '.' || s[i] == ',') sep = true;
                        i++;
                    }
                    var text = s.Substring(start, i - start).Replace(',', '.');
                    if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var n)) return null;
                    list.Add(new Token(T.Num, n, text));
                    continue;
                }

                if (char.IsLetter(c))
                {
                    int start = i;
                    while (i < s.Length && char.IsLetter(s[i])) i++;
                    list.Add(new Token(T.Id, 0, s.Substring(start, i - start).ToLowerInvariant()));
                    continue;
                }

                switch (c)
                {
                    case '+': list.Add(new Token(T.Plus, 0, "+")); break;
                    case '-': list.Add(new Token(T.Minus, 0, "-")); break;
                    case '*': case '×': list.Add(new Token(T.Mul, 0, "*")); break;
                    case '/': case '÷': list.Add(new Token(T.Div, 0, "/")); break;
                    case '^': list.Add(new Token(T.Pow, 0, "^")); break;
                    case '%': list.Add(new Token(T.Pct, 0, "%")); break;
                    case '(': list.Add(new Token(T.LP, 0, "(")); break;
                    case ')': list.Add(new Token(T.RP, 0, ")")); break;
                    default: return null;
                }
                i++;
            }

            list.Add(new Token(T.End, 0, ""));
            return list;
        }

        private sealed class Parser
        {
            private readonly List<Token> _t;
            private int _p;
            private int _depth;

            public Parser(List<Token> tokens) { _t = tokens; }

            public bool AtEnd => _t[_p].Type == T.End;

            private Token Cur => _t[_p];
            private Token Peek(int n) => _p + n < _t.Count ? _t[_p + n] : _t[^1];

            // expr := term (('+'|'-') term)*   (a +/- b% scales a by b percent)
            public double ParseExpression()
            {
                var (left, _) = ParseTerm();

                while (Cur.Type is T.Plus or T.Minus)
                {
                    bool plus = Cur.Type == T.Plus;
                    _p++;
                    var (right, isPercent) = ParseTerm();

                    if (isPercent) right = left * right;
                    left = plus ? left + right : left - right;
                }

                return left;
            }

            // term := factor (('*'|'/'|'%'mod) factor)*
            private (double Value, bool IsPercent) ParseTerm()
            {
                var (left, isPercent) = ParseFactor();

                while (true)
                {
                    if (Cur.Type == T.Mul) { _p++; left *= ParseFactor().Value; isPercent = false; }
                    else if (Cur.Type == T.Div) { _p++; left /= ParseFactor().Value; isPercent = false; }
                    else if (Cur.Type == T.Pct && StartsOperand(Peek(1)))
                    {
                        _p++;
                        left %= ParseFactor().Value;
                        isPercent = false;
                    }
                    else break;
                }

                return (left, isPercent);
            }

            private static bool StartsOperand(Token t) => t.Type is T.Num or T.LP or T.Id;

            // factor := ('-'|'+') factor | power
            private (double Value, bool IsPercent) ParseFactor()
            {
                if (++_depth > 60) throw new InvalidOperationException("too deep");
                try
                {
                    if (Cur.Type == T.Minus) { _p++; return (-ParseFactor().Value, false); }
                    if (Cur.Type == T.Plus) { _p++; return ParseFactor(); }
                    return ParsePower();
                }
                finally { _depth--; }
            }

            // power := primary ('^' factor)? '%'?
            private (double Value, bool IsPercent) ParsePower()
            {
                var v = ParsePrimary();

                if (Cur.Type == T.Pow)
                {
                    _p++;
                    v = Math.Pow(v, ParseFactor().Value);
                }

                // postfix percent: only when no operand follows (otherwise it is modulo)
                if (Cur.Type == T.Pct && !StartsOperand(Peek(1)))
                {
                    _p++;
                    return (v / 100.0, true);
                }

                return (v, false);
            }

            private double ParsePrimary()
            {
                var t = Cur;

                if (t.Type == T.Num) { _p++; return t.Num; }

                if (t.Type == T.LP)
                {
                    _p++;
                    var v = ParseExpression();
                    Expect(T.RP);
                    return v;
                }

                if (t.Type == T.Id)
                {
                    _p++;

                    if (Cur.Type == T.LP)
                    {
                        _p++;
                        var arg = ParseExpression();
                        Expect(T.RP);
                        return Apply(t.Text, arg);
                    }

                    return t.Text switch
                    {
                        "pi" => Math.PI,
                        "e" => Math.E,
                        _ => throw new InvalidOperationException("unknown identifier")
                    };
                }

                throw new InvalidOperationException("unexpected token");
            }

            private void Expect(T type)
            {
                if (Cur.Type != type) throw new InvalidOperationException("expected " + type);
                _p++;
            }

            private static double Apply(string fn, double x)
            {
                const double deg = Math.PI / 180.0;

                return fn switch
                {
                    "sqrt" or "kok" or "kök" => Math.Sqrt(x),
                    "abs" => Math.Abs(x),
                    "sin" => Math.Round(Math.Sin(x * deg), 12),
                    "cos" => Math.Round(Math.Cos(x * deg), 12),
                    "tan" => Math.Round(Math.Tan(x * deg), 12),
                    "asin" => Math.Asin(x) / deg,
                    "acos" => Math.Acos(x) / deg,
                    "atan" => Math.Atan(x) / deg,
                    "ln" => Math.Log(x),
                    "log" => Math.Log10(x),
                    "log2" => Math.Log2(x),
                    "exp" => Math.Exp(x),
                    "round" => Math.Round(x, MidpointRounding.AwayFromZero),
                    "floor" => Math.Floor(x),
                    "ceil" => Math.Ceiling(x),
                    _ => throw new InvalidOperationException("unknown function")
                };
            }
        }
    }
}
