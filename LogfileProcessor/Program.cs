using System;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace LogfileProcessor
{
    static class Functions
    {
        public static void tail(string filename, List<Regex> patterns)
        {
            long offset = -1;

            while (true)
            {
                using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    // First scan.
                    if (offset < 0) offset = Math.Max(fs.Length - 1024, 0);

                    // New log file generated.
                    if (offset > fs.Length) offset = 0;

                    using (StreamReader sr = new StreamReader(fs))
                    {
                        sr.BaseStream.Seek(offset, SeekOrigin.Begin);
                        while(true)
                        {
                            string line = sr.ReadLine();
                            if (line is not null)
                            {
                                if (patterns.Count > 0)
                                {
                                    int cntPattern = 0;
                                    while (cntPattern < patterns.Count)
                                    {
                                        if (patterns[cntPattern++].IsMatch(line))
                                        {
                                            Console.WriteLine(line);
                                            break;
                                        }
                                    }
                                }
                                else Console.WriteLine(line);
                            }
                            else break;
                        }
                        offset = fs.Length;
                    }
                }
                Thread.Sleep(250);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Retrieve arguments.
            bool tail = false;
            string file = "";
            string filePattern = "";
            string files = "";
            string output = "";
            string startTime = "";
            string endTime = "";
            var patternsRegex = new List<Regex>();
            for(int idxarg = 0; idxarg < args.Length; idxarg++)
            {
                if(args[idxarg].Equals("-Tail")) tail = true;
                if(args[idxarg].Equals("-File")) file = args[++idxarg];
                if(args[idxarg].Equals("-FilePattern")) filePattern = args[++idxarg];
                if(args[idxarg].Equals("-Files")) files = args[++idxarg];
                if(args[idxarg].Equals("-Output")) output = args[++idxarg];
                if(args[idxarg].Equals("-StartTime")) startTime = args[++idxarg];
                if(args[idxarg].Equals("-EndTime")) endTime = args[++idxarg];
                if (args[idxarg].Equals("-Patterns"))
                {
                    idxarg++;
                    while (idxarg < args.Length)
                    {
                        string pattern = args[idxarg++];
                        patternsRegex.Add(new Regex(pattern, RegexOptions.Compiled));
                    }
                }
            }

            try
            {
                // If tailing file, ensure that the file can be found.
                if(tail)
                {
                    // Process regex patterns.
                    Functions.tail(file, patternsRegex);
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"Cannot find {file}. Returning.");
            }

            Console.ReadLine();
        }
    }
}