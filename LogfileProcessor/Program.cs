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
        internal static void processLog(IEnumerable<string> filenames, List<Regex> patterns = null, Action<string> print = null)
        {
            if (print is null) print = (x) => Console.WriteLine(x);

            if (patterns is not null && patterns.Count > 0)
            {
                foreach (string filename in filenames)
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
                                        print(line);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (string filename in filenames)
                {
                    using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (StreamReader sr = new StreamReader(fs))
                        {
                            while (true)
                            {
                                string line = sr.ReadLine();
                                if (line is null) break;
                                else print(line);
                            }
                        }
                    }
                }
            }
        }

        internal static void processLog(List<string> filenames)
        {
            processLog(filenames);
        }

        internal static void processLog(List<FileInfo> files, List<Regex> patterns, string output = "")
        {
            var filenames = new List<string>();
            foreach (FileInfo fi in files)
                filenames.Add(fi.FullName);
            if (output.Length > 0)
            {
                using StreamWriter outfile = new(output);
                Action<string> print = (x) => outfile.WriteLine(x);
                processLog(filenames, patterns, print);
            }
            else
                processLog(filenames, patterns);
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

        internal static DateTime parseDate(string date)
        {
            return new DateTime(int.Parse(date.Substring(0, 4))
                ,int.Parse(date.Substring(4, 2))
                ,int.Parse(date.Substring(6, 2))
                ,int.Parse(date.Substring(9,2))
                ,int.Parse(date.Substring(11,2))
                ,int.Parse(date.Substring(13,2)));
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
                else if(args[idxarg].Equals("-Output")) output = args[++idxarg].Trim();
                else if(args[idxarg].Equals("-StartTime")) startTime = args[++idxarg];
                else if(args[idxarg].Equals("-EndTime")) endTime = args[++idxarg];
                else if(args[idxarg].Equals("-Files"))
                {
                    idxarg++;
                    var ls = new List<string>();
                    idxarg = Functions.collectArgs(ls, idxarg, args);
                    foreach (String filename in ls)
                        filenames.Add(filename.Trim());
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
                        files[cnt++] = new FileInfo(filename);
                    var sorted = files.OrderBy(f => f.LastWriteTime).ToList();

                    // Remove files that are not within start and end times.
                    startTime = startTime.Trim();
                    endTime = endTime.Trim();
                    DateTime start = DateTime.Now;
                    DateTime end = DateTime.Now;
                    if(startTime.Length > 0)
                        start = Functions.parseDate(startTime);
                    if(endTime.Length > 0)
                        end = Functions.parseDate(endTime);
                    cnt = 0;
                    while(cnt < sorted.Count)
                    {
                        if ((startTime.Length > 0 && sorted[cnt].LastWriteTime < start)
                            || (endTime.Length > 0 && sorted[cnt].CreationTime > end))
                            sorted.RemoveAt(cnt);
                        else cnt++;
                    }

                    // Check if an output file is specified.
                    Functions.processLog(sorted, patternsRegex, output);
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Cannot find {filenames[0]}. Returning.");
            }

            Console.WriteLine("Done.");
            Console.ReadLine();
        }
    }
}