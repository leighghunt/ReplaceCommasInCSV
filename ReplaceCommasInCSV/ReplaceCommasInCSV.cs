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

            if (outputFilename.Length == 0)
            {
                outputFilename = inputFilename;
            }

            Console.WriteLine(string.Format("inputFilename: {0}", inputFilename));
            Console.WriteLine(string.Format("replacementString: {0}", replacementString));
            Console.WriteLine(string.Format("outputFilename: {0}", outputFilename));

            if (argsOK)
            {
                ReplaceCommasInCSV replaceCommasInCSV = new ReplaceCommasInCSV(inputFilename, replacementString, outputFilename);
                replaceCommasInCSV.Replace();
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

    class ReplaceCommasInCSV
    {
        const int _indicateProgressAfter = 1000; // Number of lines read between indication of progress.
        const int _flushAfter = 100000; // Number of lines written between writer flushes.
        string _inputFilename = "";
        string _replacementString = "";
        string _outputFilename = "";
        DateTime _started;

        public string InputFilename
        {
            get
            {
                return this._inputFilename;
            }
            set
            {
                this._inputFilename = value;
            }
        }

        public string ReplacementString
        {
            get
            {
                return this._replacementString;
            }
            set
            {
                this._replacementString = value;
            }
        }

        public string OutputFilename
        {
            get
            {
                return this._outputFilename;
            }
            set
            {
                this._outputFilename = value;
            }
        }

        public ReplaceCommasInCSV(string inputFilename, string replacementString, string outputFilename)
        {
            InputFilename = inputFilename;
            ReplacementString = replacementString;
            OutputFilename = outputFilename;
        }

        public bool Replace()
        {
            try
            {
                this._started = DateTime.Now;
                if (!System.IO.File.Exists(InputFilename))
                {
                    Console.WriteLine("Could not find file: " + InputFilename);
                }
                else
                {
                    string tempOutputFilename = OutputFilename;
                    if (InputFilename == OutputFilename)
                    {
                        tempOutputFilename = System.IO.Path.GetTempFileName();
                    }

                    Console.WriteLine(string.Format("Opening file {0}...", InputFilename));
                    Console.WriteLine(string.Format("Writing to {0}...", tempOutputFilename));

                    System.IO.TextReader reader = System.IO.File.OpenText(InputFilename);
                    System.IO.StreamWriter writer = new System.IO.StreamWriter(tempOutputFilename);
                    
                    //System.IO.StringWriter writer ;
                    StringBuilder lineBuilder = new StringBuilder();
                    int linesRead = 0;
                    int commasReplaced = 0;
                    int commasPreserved = 0;

                    while (true)
                    {
                        string line = reader.ReadLine();
                        ++linesRead;
                        if (linesRead % ReplaceCommasInCSV._indicateProgressAfter == 0)
                        {
                            Console.Write(".");
                        }

                        if (linesRead % ReplaceCommasInCSV._flushAfter == 0)
                        {
                            Console.Write("F");
                            writer.Flush();
                        }

                        if (line != null)
                        {
                            bool inString = false;
                            lineBuilder.Clear();

                            for (int charIndex = 0; charIndex < line.Length; ++charIndex)
                            {
                                if (line[charIndex] == '"')
                                {
                                    inString = !inString;
                                }

                                if (!inString && line[charIndex] == ',')
                                {
                                    lineBuilder.Append(ReplacementString);
                                    ++commasReplaced;
                                }
                                else
                                {
                                    lineBuilder.Append(line[charIndex]);
                                    if (line[charIndex] == ',')
                                    {
                                        ++commasPreserved;
                                    }
                                }
                            }

                            writer.WriteLine(lineBuilder.ToString());
                        }
                        else
                        {
                            break;
                        }
                    }

                    Console.WriteLine("\nClosing reader...");
                    reader.Close();
                    Console.WriteLine("Closing writer...");
                    writer.Close();

                    // If we write to temp file, now save it.
                    if (OutputFilename != tempOutputFilename)
                    {
                        Console.WriteLine("Moving temporary file...");
                        System.IO.File.Copy(tempOutputFilename, OutputFilename, true);
                        Console.WriteLine("Deleting temporary file...");
                        System.IO.File.Delete(tempOutputFilename);
                    }

                    TimeSpan duration = DateTime.Now - this._started;

                    Console.WriteLine("\nProcessed {0} lines in {1} hrs, {2} mins, {3} secs, commas replaced = {4}, commas preserved = {5}",  linesRead, (int)duration.TotalHours, duration.Minutes, duration.Seconds, commasReplaced, commasPreserved);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
    }

}
