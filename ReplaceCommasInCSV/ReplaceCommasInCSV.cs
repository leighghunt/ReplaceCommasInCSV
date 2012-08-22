using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReplaceCommasInCSV
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputFilename = "";
            string replacementString = "|";
            string outputFilename = "";
            bool argsOK = true;
            
            for(int argIndex = 0; (argIndex < args.Count()) && (argsOK == true); ++ argIndex)
            {
                string argument = args[argIndex];

                if (argument.StartsWith("/") || argument.StartsWith("-"))
                {
                    ++argIndex;

                    string argumentFlag = argument.Substring(1, argument.Length - 1);

                    if (argIndex < args.Count())
                    {
                        switch (argumentFlag.ToUpper())
                        {
                            case "R":
                                replacementString = args[argIndex];
                                break;
                            case "O":
                                outputFilename = args[argIndex];
                                break;
                            default:
                                Console.WriteLine("Unexpected argument flag " + argumentFlag);
                                argsOK = false;
                                break;
                        }
                    } else
                    {
                        Console.WriteLine("Missing argument " + argumentFlag);
                        argsOK = false;
                    }
                }
                else
                {
                    if (inputFilename.Length == 0)
                    {
                        inputFilename = argument;
                    }
                    else
                    {
                        // inputFilename already specified
                        argsOK = false;
                    }
                }
            }

            if (inputFilename.Length == 0)
            {
                Console.WriteLine("Missing filename argument");
                argsOK = false;
            }

            Console.WriteLine(string.Format("inputFilename: {0}", inputFilename));
            Console.WriteLine(string.Format("replacementString: {0}", replacementString));
            Console.WriteLine(string.Format("outputFilename: {0}", outputFilename));

            if (argsOK)
            {
            } 
            else
            {
                Usage();
            }

        }

        static private void Usage()
        {
            string usage = "ReplaceCommasInCSV filename [/R replacement string] [/O output filename]\n" +
                "filename       Input filename\n" +
                "R              Replacement string - commas not within quoted strings will be replaced with this string.\n" +
                "               Optional - if omitted, a pipe ('|') is used.\n" +
                "O              Output filename - optional, if omitted, original file will be overwritten";

            Console.WriteLine(usage);
        }
    }
}
