using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SudokuAutoTest
{
    public class SudokuTester
    {
        private ProcessStartInfo _binaryInfo;
        private string _binaryDir;
        public string NumberId { get; }
        public List<Tuple<string, int>> Scores { get; }

        public SudokuTester(string baseDir,string numberId)
        {
            Scores = new List<Tuple<string, int>>();
            NumberId = numberId;
            _binaryDir = Path.Combine(baseDir, NumberId, "BIN");
            _binaryInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = Path.Combine(_binaryDir, "sudoku.exe"),
                UseShellExecute = false,
                WorkingDirectory = _binaryDir
            };
            Trace.Listeners.Add(new TextWriterTraceListener(Path.Combine(_binaryDir, "log.txt")));
            Trace.AutoFlush = true;
        }

        //If success,return time; Else, return "error message"
        public int ExecuteTest(string arguments, int timeLimit)
        {
            if (!File.Exists(_binaryInfo.FileName))
            {
                return (int)ErrorType.NoSudokuExe;
            }
            _binaryInfo.Arguments = arguments;
            try
            {
                Stopwatch timeWatch = new Stopwatch();
                timeWatch.Start();
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(_binaryInfo))
                {
                    //Cancel task
                    var cancelTokenSource = new CancellationTokenSource(timeLimit * 1000);
                    var task = Task.Factory.StartNew(() =>
                    {
                        //monitor the timeWatch
                        while (!cancelTokenSource.IsCancellationRequested)
                        {
                            Thread.Sleep(1000);
                        }
                        //Release the resource of exe files.
                        exeProcess.Close();
                    }, cancelTokenSource.Token);
                    //Start monitor
                    exeProcess.WaitForExit();
                    timeWatch.Stop();
                    if (task.Status == TaskStatus.Running)
                    {
                        //Shutdown the task
                        cancelTokenSource.Cancel();
                    }
                    //if task already being canceled
                    else
                    {
                        return (int) ErrorType.RunOutOfTime;
                    }
                }
                //Check the sudoku file
                string checkFile = Path.Combine(_binaryDir, "sudoku.txt");
                if (!File.Exists(checkFile))
                {
                    return (int) ErrorType.NoGeneratedSudokuTxt;
                }
                var isCorrect = CheckValid(checkFile, int.Parse(Regex.Match(arguments, @"\d+").Value));
                if (isCorrect)
                {
                    return timeWatch.Elapsed.Seconds;
                }
                else
                {
                    return (int) ErrorType.InvalidSudokuPanels;
                }
            }
            catch(Exception e)
            {
                //Log into file to record the runtime error
                Trace.WriteLine($"Arguments:{arguments}\nRuntimeError:{e.Message}\n\n");
                return (int) ErrorType.RuntimeError;
            }
        }

        //Overview:对可执行文件测试执行每一个测试点,得到每个点的运行时长或错误类别
        public void GetCorrectScore()
        {
            //正确性测试占分25,共5个测试点
            //其中10分为错误情况得分,在自动化测试中不进行
            //剩余15分共有5个正确性测试点
            string[] argumentScoreMap = new string[]
            {
                "-c 1",
                "-c 5",
                "-c 100",
                "-c 500",
                "-c 1000"
            };
            foreach (var argument in argumentScoreMap)
            {
                Scores.Add(new Tuple<string, int>(argument, ExecuteTest(argument, 60)));
            }
            //剩下10分,分为2组测试
            //5万+
            //100万+
            argumentScoreMap = new string[]
            {
                "-c 50000",
                "-c 1000000"
            };
            foreach (var argument in argumentScoreMap)
            {
                //Limit is 600s
                Scores.Add(new Tuple<string, int>(argument, ExecuteTest(argument, 600)));
            }
        }

        public bool CheckValid(string filePath, int count)
        {
            //新申请一个数独棋盘
            var sudokuSets = new HashSet<SudokuPanel>();
            //从路径中读取相应内容
            var content = File.ReadAllText(filePath);
            var multipleLines = content.Split(new [] {"\n\n"}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var lines in multipleLines)
            {
                var sudokuPanel = new SudokuPanel(lines.Split('\n'));
                if (sudokuSets.Contains(sudokuPanel))
                {
                    return false;
                }
                if (!sudokuPanel.Valid)
                {
                    return false;
                }
                sudokuSets.Add(sudokuPanel);
            }
            return sudokuSets.Count == count;
        }
    }

    class SudokuPanel
    {
        public string[,] Grid { get; set; }
        public bool Valid { get; }

        public SudokuPanel(string[] rows)
        {
            int length = rows.Length;
            Grid = new string[length,length];
            for (int rowIndex = 0; rowIndex < length; rowIndex++)
            {
                var row = rows[rowIndex];
                string[] columns = row.Split(null);
                for (int colIndex = 0; colIndex < columns.Length; colIndex++)
                {
                    Grid[rowIndex,colIndex] = columns[colIndex];
                }
            }
            Valid = Validiate();
        }

        private bool Validiate()
        {
            int length = Grid.GetLength(0);
            bool[,] rowCheck = new bool[length, length];
            bool[,] colCheck = new bool[length, length];
            bool[,] squareCheck = new bool[length, length];
            for (int i = 0; i < Grid.GetLength(0); i++)
            {
                for (int j = 0; j < Grid.GetLength(1); j++)
                {
                    if (!string.Equals(Grid[i, j],"0"))
                    {
                        int num = int.Parse(Grid[i, j]) - 1, k = i/3 * 3 + j/3;
                        if (rowCheck[i,num] || colCheck[j,num] || squareCheck[k,num])
                        {
                            return false;
                        }
                        rowCheck[i, num] = colCheck[j, num] = squareCheck[k, num] = true;
                    }
                }
            }
            return true;
        }
    }

    class Comparator : IEqualityComparer<SudokuPanel>
    {
        public bool Equals(SudokuPanel x, SudokuPanel y)
        {
            return GetHashCode(x) == GetHashCode(y);
        }

        public int GetHashCode(SudokuPanel obj)
        {
            return string.Join("", obj.Grid).GetHashCode();
        }
    }

    public enum ErrorType
    {
        NoSudokuExe = -1,
        NoGeneratedSudokuTxt = -2,
        RuntimeError = -3,
        InvalidSudokuPanels = -4,
        RunOutOfTime = -5
    }
}
