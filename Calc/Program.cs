using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers;
using Roslyn.Scripting.CSharp;
using Roslyn.Scripting;
using Roslyn.Compilers.CSharp;
using System.Text.RegularExpressions;

// THE COMMAND-LINE CALCULATOR (CC.EXE)
// Basic calculator app created as a replacement for the abomination that is Microsoft Calculator
// and to leverage the new capabilities of the NET Compiler Platform in particular dynamic compilation.

namespace Calc
{
    class Program
    {
        static void Main(string[] args)
        {
            string arg = args.Length > 0 ? string.Join(" ", args) : String.Empty;

            var engine = new ScriptEngine();
            var session = engine.CreateSession();
            session.ImportNamespace("System");
            double lastResult = 0;
            Regex powReplacer = new Regex(@"(\d*\.\d+|\d+)\s*\^\s*(\d*\.\d+|\d+)"); // ex: 4^5 is replaced with Math.Pow(4, 5)
            Regex doubleInsertion = new Regex(@"(?<NUMBER>\d*\.\d+|\d+)"); // for convenience, force all numerical values to evaluate as doubles
            string scriptline;

            while (true)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(arg))
                    {
                        Console.Write("> ");
                        arg = Console.ReadLine().Trim(); // wait for input from user
                    }

                    if (!string.IsNullOrWhiteSpace(arg))
                    {
                        if (arg.Equals("quit", StringComparison.OrdinalIgnoreCase)) break;
                        else if (arg.Equals("cls", StringComparison.OrdinalIgnoreCase)) Console.Clear();
                        else
                        {
                            scriptline = (arg.StartsWith("*", StringComparison.OrdinalIgnoreCase) || arg.StartsWith("/", StringComparison.OrdinalIgnoreCase) ||
                                arg.StartsWith("-", StringComparison.OrdinalIgnoreCase) || arg.StartsWith("+", StringComparison.OrdinalIgnoreCase) || arg.StartsWith("^", StringComparison.OrdinalIgnoreCase))
                                ? string.Concat(lastResult, arg) : arg;


                            scriptline = powReplacer.Replace(scriptline, new MatchEvaluator(delegate(Match m) { return "Math.Pow(" + m.Groups[1] + ", " + m.Groups[2] + ")"; }));
                            scriptline = doubleInsertion.Replace(scriptline, "${NUMBER}d"); //string.Format(@"System.Console.WriteLine({0});", arg);
                            
                            lastResult = Convert.ToDouble(session.Execute(scriptline));
                            Console.WriteLine(lastResult);
                        }
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine(Environment.NewLine + "Could not evaluate expression");
                }

                arg = string.Empty;
            }
        }
    }
}
