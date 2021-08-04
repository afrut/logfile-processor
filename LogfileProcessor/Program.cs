using System;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace LogfileProcessor
{
    static class Functions
    {
        internal static void processLog(string filename, List<Regex> patterns)
        {
            if (patterns.Count > 0)
            {
                using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        while (true)
                        {
                            string line = sr.ReadLine();
                            if (line is null) break;
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
                    }
                }
            }
            else
            {
                using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        while (true)
                        {
                            string line = sr.ReadLine();
                            if (line is null) break;
                            else Console.WriteLine(line);
                        }
                    }
                }
            }
        }

        internal static void processLog(List<string> filenames)
        {
            var patterns = new List<Regex>();
            foreach (String filename in filenames)
                processLog(filename, patterns);
        }

        internal static void processLog(List<FileInfo> files, List<Regex> patterns)
        {
            foreach (FileInfo fi in files)
                processLog(fi.FullName, patterns);
        }

        internal static void tail(string filename, List<Regex> patterns)
        {
            long offset = -1;

            if (patterns.Count > 0)
            {
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
                            while (true)
                            {
                                string line = sr.ReadLine();
                                if (line is not null)
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
                                else break;
                            }
                            offset = fs.Length;
                        }
                    }
                    Thread.Sleep(250);
                }
            }
            else
            {
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
                            while (true)
                            {
                                string line = sr.ReadLine();
                                if (line is not null)
                                {
                                    Console.WriteLine(line);
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

        internal static bool checkArg(string s)
        {
            if (s.Equals("-Tail")
                || s.Equals("-File")
                || s.Equals("-FilePattern")
                || s.Equals("-Files")
                || s.Equals("-Output")
                || s.Equals("-StartTime")
                || s.Equals("-EndTime")
                || s.Equals("-Patterns"))
                return true;
            else return false;
        }

        internal static int collectArgs(List<string> ls, int idx, string[] args)
        {
            while(idx < args.Length)
            {
                if (checkArg(args[idx]))
                {
                    idx--;
                    return idx;
                }
                ls.Add(args[idx++]);
            }
            return idx;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Retrieve arguments.
            bool tail = false;
            string filePattern = "";
            var filenames = new List<string>();
            string output = "";
            string startTime = "";
            string endTime = "";
            var patternsRegex = new List<Regex>();
            for(int idxarg = 0; idxarg < args.Length; idxarg++)
            {
                if(args[idxarg].Equals("-Tail")) tail = true;
                else if(args[idxarg].Equals("-FilePattern")) filePattern = args[++idxarg];
                else if(args[idxarg].Equals("-Output")) output = args[++idxarg];
                else if(args[idxarg].Equals("-StartTime")) startTime = args[++idxarg];
                else if(args[idxarg].Equals("-EndTime")) endTime = args[++idxarg];
                else if(args[idxarg].Equals("-Files"))
                {
                    idxarg++;
                    idxarg = Functions.collectArgs(filenames, idxarg, args);
                }
                else if (args[idxarg].Equals("-Patterns"))
                {
                    idxarg++;
                    var patterns = new List<String>();
                    idxarg = Functions.collectArgs(patterns, idxarg, args);
                    foreach(String pattern in patterns)
                        patternsRegex.Add(new Regex(pattern, RegexOptions.Compiled));
                }
            }

            try
            {
                // If tailing file, ensure that the file can be found.
                if (tail && filenames.Count > 0)
                {
                    // Process regex patterns.
                    Functions.tail(filenames[0], patternsRegex);
                }
                else
                {
                    // Sort the files by ascending last modified time.
                    var files = new FileInfo[filenames.Count];
                    int cnt = 0;
                    foreach (string filename in filenames)
                    {
                        files[cnt++] = new FileInfo(filename);
                    }
                    var sorted = files.OrderBy(f => f.LastWriteTime).ToList();
                    Functions.processLog(sorted, patternsRegex);
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Cannot find {filenames[0]}. Returning.");
            }

            Console.ReadLine();
        }
    }
}