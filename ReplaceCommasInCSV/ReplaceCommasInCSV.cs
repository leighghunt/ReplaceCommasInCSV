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
            bool preserveQuotes = false;
            bool handleUnmatchedQuotes = false;
            bool argsOK = true;
            
            for(int argIndex = 0; (argIndex < args.Count()) && (argsOK == true); ++ argIndex)
            {
                string argument = args[argIndex];

                if (argument.StartsWith("/") || argument.StartsWith("-"))
                {
                    string argumentFlag = argument.Substring(1, argument.Length - 1);

                    if (argIndex < args.Count())
                    {
                        switch (argumentFlag.ToUpper())
                        {
                            case "R":
                                replacementString = args[++argIndex];
                                break;
                            case "O":
                                outputFilename = args[++argIndex];
                                break;
                            case "Q":
                                preserveQuotes = true;
                                break;
                            case "U":
                                handleUnmatchedQuotes = true;
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

            /*
            Console.WriteLine(string.Format("inputFilename: {0}", inputFilename));
            Console.WriteLine(string.Format("replacementString: {0}", replacementString));
            Console.WriteLine(string.Format("outputFilename: {0}", outputFilename));
            Console.WriteLine(string.Format("preserveQuotes: {0}", preserveQuotes));
            Console.WriteLine(string.Format("handleUnmatchedQuotes: {0}", handleUnmatchedQuotes));
            */

            if (argsOK)
            {
                ReplaceCommasInCSV replaceCommasInCSV = new ReplaceCommasInCSV(inputFilename, replacementString, outputFilename, preserveQuotes, handleUnmatchedQuotes);
                replaceCommasInCSV.Replace();
            } 
            else
            {
                Usage();
            }

        }

        static private void Usage()
        {
            string usage = "ReplaceCommasInCSV filename [/R replacement string] [/O output filename] /Q\n" +
                "filename       Input filename\n" +
                "/R             Replacement string - commas not within quoted strings will be replaced\n" +
                "               with this string. Optional - if omitted, a pipe ('|') is used\n" +
                "/O             Output filename - optional, if omitted, original file will be overwritten\n" +
                "/Q             Preserve double quotes, if specified, all double quotes will be left in output.\n" +
                "               Default is to strip double quotes from output\n" +
                "/U             Handle unmatched quotes. If a newline is encountered inside a string, the \n" +
                "               newline will be replaced by '\\n'. Default is to not handle, warn user and exit";

            Console.WriteLine(usage);
        }
    }

    class ReplaceCommasInCSV
    {
        const int _indicateProgressAfter = 1000; // Number of lines read between indication of progress.
        string _inputFilename = "";
        string _replacementString = "";
        string _outputFilename = "";
        bool _preserveQuotes = true;
        bool _handleUnmatchedQuotes = false;
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

        public bool PreserveQuotes
        {
            get
            {
                return this._preserveQuotes;
            }
            set
            {
                this._preserveQuotes = value;
            }
        }

        public bool HandleUnmatchedQuotes
        {
            get
            {
                return this._handleUnmatchedQuotes;
            }
            set
            {
                this._handleUnmatchedQuotes = value;
            }
        }

        public ReplaceCommasInCSV(string inputFilename, string replacementString, string outputFilename, bool preserveQuotes, bool handleUnmatchedQuotes)
        {
            InputFilename = inputFilename;
            ReplacementString = replacementString;
            OutputFilename = outputFilename;
            PreserveQuotes = preserveQuotes;
            HandleUnmatchedQuotes = handleUnmatchedQuotes;
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

                    System.IO.FileInfo fileInfo = new System.IO.FileInfo(InputFilename);
                    System.IO.TextReader reader = System.IO.File.OpenText(InputFilename);
                    System.IO.StreamWriter writer = new System.IO.StreamWriter(tempOutputFilename);
                    
                    //System.IO.StringWriter writer ;
                    StringBuilder lineBuilder = new StringBuilder();
                    long linesRead = 0;
                    long charsRead = 0;
                    long commasReplaced = 0;
                    long commasPreserved = 0;
                    long unmatchedQuotesEncountered = 0;
                    bool inString = false;
                    long totalChars = fileInfo.Length;

                    string ETA = "estimating...";

                    while (true)
                    {
                        string line = reader.ReadLine();
                        ++linesRead;

                        if (line != null)
                        {
                            charsRead += line.Length;
                            if (linesRead % (ReplaceCommasInCSV._indicateProgressAfter * 100) == 0)
                            {
                                TimeSpan durationSoFar = DateTime.Now - this._started;
                                double charsPerMillisecond = charsRead / durationSoFar.TotalMilliseconds;
                                DateTime projectedEndTime = DateTime.Now.AddMilliseconds(((totalChars - charsRead) / charsPerMillisecond));
                                TimeSpan projectedTimeLeft = projectedEndTime - DateTime.Now;
                                ETA = string.Format("{0} hrs, {1} mins, {2} secs     ", (int)projectedTimeLeft.TotalHours, projectedTimeLeft.Minutes, projectedTimeLeft.Seconds);
                            }

                            if (linesRead % ReplaceCommasInCSV._indicateProgressAfter == 0)
                            {
                                //Console.Write(".");
                                Console.SetCursorPosition(0, Console.CursorTop);
                                Console.Write(string.Format("{0:#}% complete, {1:#,###} lines processed. ETA {2}", (charsRead * 100) / totalChars, linesRead, ETA));
                            }

                            if (inString) // We're processing the remaining part from previous line.
                            {

                            }
                            else
                            {
                                lineBuilder.Clear();
                            }

                            for (int charIndex = 0; charIndex < line.Length; ++charIndex)
                            {
                                if (line[charIndex] == '"')
                                {
                                    inString = !inString;

                                    if (!PreserveQuotes)
                                    {
                                        // Log if we're changing string state, but skip adding the quote into the output.
                                        continue;
                                    }
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

                            // Check we didn't have a non-terminated double quote
                            if (inString)
                            {
                                ++unmatchedQuotesEncountered;
                                if (HandleUnmatchedQuotes)
                                {
                                    lineBuilder.Append("\\n");
                                    // Carry on - we'll handle this in next iteration of while loop
                                }
                                else
                                {
                                    Console.WriteLine(string.Format("\nERROR - found unmatched double quote in line {0} - EXITING!\n\nTry running with /U switch to concatenate string field values that span multiple lines.\n\nFailing line:\n", linesRead));
                                    Console.WriteLine(line);
                                    Console.WriteLine(reader.ReadLine());

                                    return false;
                                }
                            }
                            else
                            {
                                writer.WriteLine(lineBuilder.ToString());
                            }
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

                    TimeSpan duration = DateTime.Now - this._started;

                    Console.WriteLine("\nProcessed {0} lines in {1} hrs, {2} mins, {3} secs, commas replaced = {4}, commas preserved = {5}, unmatched quotes encountered = {6}", linesRead, (int)duration.TotalHours, duration.Minutes, duration.Seconds, commasReplaced, commasPreserved, unmatchedQuotesEncountered);

                    // If we write to temp file, now save it.
                    if (OutputFilename != tempOutputFilename)
                    {
                        Console.WriteLine("Moving temporary file...");
                        System.IO.File.Copy(tempOutputFilename, OutputFilename, true);
                        Console.WriteLine("Deleting temporary file...");
                        System.IO.File.Delete(tempOutputFilename);
                    }
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
