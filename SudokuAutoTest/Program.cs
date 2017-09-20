using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Hosting;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;


namespace SudokuAutoTest
{
    class Program
    {
        //Need files
        public static string BlogFile = @"./BlogList.txt";
        public static string GithubFile = @"./GithubRepos.txt";

        //Gen directories
        public static string LogDir = @"./Log";
        public static string ProjectDir = @"./Projects";

        //Gen files
        public static string ResultFile = @"./Scores.txt";
        public static string WrongFile = @"./Wrong.txt";
        public static string RepoFile = @"./RepoMap.txt";

        //Max Limit
        public static int MaxLimitTime = 600;
        
        //Cover all students' score
        public static string ModeInput = "s";

        //For single mode
        public static string Number;

        public static void Main(string[] args)
        {
            try
            {
                string command = args[0];
                for (int i = 2; i < args.Length; i += 2)
                {
                    switch (args[i-1])
                    {
                        case "-blogPath":
                            BlogFile = args[i];
                            break;
                        case "-gitPath":
                            GithubFile = args[i];
                            break;
                        case "-limit":
                            MaxLimitTime = int.Parse(args[i]);
                            break;
                        case "-mode":
                            ModeInput = args[i];
                            break;
                        case "-number":
                            Number = args[i];
                            break;
                    }
                }
                CheckFileStatus(command);
                switch (command)
                {
                    case "/grab":
                        GrabRepos();
                        break;
                    case "/score":
                        ScoreRepos();
                        break;
                    case "/wrong":
                        ProcessWrong();
                        break;
                    default:
                        throw new Exception("暂不支持此类型的参数!");
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Hint();
            }
        }

        public static void Hint()
        {
            Console.WriteLine("Usages: \n"+
                              "/grab -blogPath [blog file] -gitPath [git file] -mode [score mode] -number [number id]\n\n"+
                              "\t- 本功能用于从学生的作业中自动提取Github链接, 并将Git仓库Clone到文件夹 Projects 下, 错误日志在文件夹 Log 下, Github项目链接映射表在 RepoMap.txt 中。\n\n"+
                              "\t- 文件 [blog file] 提供学号与作业地址的对应关系, 多行分开。如不指定该参数则默认为当前目录 BlogList.txt。\n\t 每行的格式如: 031502334\thttp://cnblogs.com/easteast/p/1234.html 【分隔符为\\t】\n\n" +
                              "\t- 文件 [git file] 提供学号与Github主页的对应关系, 多行分开。如不指定该参数则默认为当前目录 GithubRepos.txt。\n\t 每行的格式如: 031502334\thttp://github.com/easteast 【分隔符为\\t】\n\n\n" +
                              "\t- 文本 [score mode] 指定测试选用的模式，目前提供三种选择：\n\t\ta : 跳过当前 Projects目录下已有工程, 分析其他同学的博客 ,并将项目克隆到本地。\n\t\tw : 将 Projects 文件夹重命名,爬取所有学生的博客并将项目克隆到 Projects文件夹下。\n\t\ts: 已有正确测试结果的不再重新爬取，只测试存在错误情况的项目。\n\n" +
                              "\t- 学号 [number id] 提供单个学号, 当本参数存在时, 将只抓取单个同学的博客并重新克隆工程。\n\n" +
                              "/score -blogPath [blog file] -limit [max limit second] -mode [score mode] -number [number id]\n\n" +
                              "\t- 本功能用于给学生的作业进行评分,并记录每份作业在不同测试数据下耗费的时间。最终生成的评分文件为 Scores.txt, 可直接复制到Excel中使用。\n\n" +
                              "\t- 文件 [blog file] 提供学号与作业地址的对应关系, 多行分开。如不指定该参数则默认为当前目录 BlogList.txt。\n\t每行的格式如: 031502334\thttp://cnblogs.com/easteast/p/1234.html 【分隔符为\\t】\n\n" +
                              "\t- 数字 [limit second] 指定效率测试运行的最大时长, 默认为 600秒。\n\n"+
                              "\t- 文本 [score mode] 指定测试选用的模式，目前提供三种选择：\n\t\ta : 跳过当前已有评分,对其他同学进行测试,将结果追加写入score.txt中。\n\t\tw : 全部重新测试，生成新的score.txt文件。\n\t\ts: 已有正确测试结果的不再重新测试，只测试不正确的。\n\n" +
                              "\t- 学号 [number id] (可选参数)提供单个学号, 当本参数存在时，将只测试单个同学的工程，并将结果存储至 学号-score.txt中。\n\n"+
                              "在实际使用时, 先使用 /grab 再直接使用 /score即可");
        }

        //检查文件状态,确保文件存在
        public static void CheckFileStatus(string command)
        {
            ReCreateDir(LogDir);
            //Grab 所有同学的博客并克隆项目
            if (command.Equals("/grab"))
            {
                if (!Directory.Exists(ProjectDir))
                {
                    //如果没有 Projects 目录, 就创建一个
                    Directory.CreateDirectory(ProjectDir);
                    //单个测试的情况下
                    if (Number != null)
                    {
                        //如果已经有了项目目录, 就删除它
                        var clonePath = Path.Combine(ProjectDir, Number);
                        RemoveDir(clonePath);
                    }
                }
                else if (Number == null
                    && ModeInput.Equals(Mode.Written)
                    && Directory.Exists(ProjectDir))
                {
                    ReCreateDir(ProjectDir);
                }
            }
            //Score 所有同学的项目
            else
            {
                if (!Directory.Exists(ProjectDir))
                {
                    throw new Exception($"当前目录下缺少 {ProjectDir} 仓库目录, 请先使用 /grab 功能生成后再使用评分！");
                }
                //Score 指定学号的项目
                if (Number != null)
                {
                    var clonePath = Path.Combine(ProjectDir, Number);
                    if (!Directory.Exists(clonePath))
                    {
                        throw new Exception($"当前目录下缺少 {clonePath} 仓库目录, 请先使用 /grab -number {Number} 功能生成后再使用评分！");
                    }
                }
            }
            if (!File.Exists(BlogFile))
            {
                throw new Exception("作业文件路径不正确!");
            }
            if (!File.Exists(GithubFile))
            {
                throw new Exception("Git仓库指定文件路径不正确!");
            }
        }

        //Overview: 重新生成目录 dirPath
        public static void ReCreateDir(string dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                if (Directory.GetDirectories(dirPath).Any() || Directory.GetFiles(dirPath).Any())
                {
                    TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                    var timeStr = Convert.ToInt64(ts.TotalSeconds).ToString();
                    string newPath = dirPath + "-" + timeStr;
                    Directory.Move(dirPath, newPath);
                }
                else
                {
                    Directory.Delete(dirPath);
                }
            }
            Directory.CreateDirectory(dirPath);
        }

        //Overview: 间接删除目录 dirPath
        //Require: 只能用于带学号场景
        public static void RemoveDir(string dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                var timeStr = Convert.ToInt64(ts.TotalSeconds).ToString();
                string newPath = Path.Combine(ProjectDir, Number + "-" + timeStr);
                Directory.Move(dirPath, newPath);
            }
        }

        //分析博客,自动获取Github仓库链接并存储至指定目录
        public static void GrabRepos()
        {
            GitRepoHandler handler = new GitRepoHandler(ProjectDir);
            //集体测试模式
            if (Number == null)
            {
                //把每个学生的Github Repo都存储在文件夹BASE_DIR下,并以学号命名文件夹
                Console.WriteLine("开始爬取所有学生的博客并匹配生成Github项目目录 ...");
                handler.GetAllGithubRepos();
            }
            //独立测试模式
            else
            {
                //仅抓取并克隆一个同学的博客
                Console.WriteLine($"开始爬取 {Number} 的博客并匹配生成Github项目目录 ...");
                handler.GetOneGithubRepo(Number);
            }
        }

        //测试程序,自动打分
        public static void ScoreRepos()
        {
            if (Number == null)
            {
                ProcessScore();
            }
            else
            {
                SingleScore();
            }
        }

        //单次测试某位同学的成绩,生成文件到 学号-score.txt 中
        public static void SingleScore()
        {
            var writePath = $"./{Number}-Score.txt";
            using (var writer = new StreamWriter(writePath, false))
            {
                SudokuTester tester = new SudokuTester(ProjectDir, Number);
                try
                {
                    tester.GetCorrectScore();
                    var arguments = tester.Scores.Select(i => i.Item1).ToList();
                    writer.Write("NumberID\t");
                    writer.WriteLine(string.Join("\t", arguments));
                    writer.Write(tester.NumberId + "\t");
                    writer.WriteLine(string.Join("\t", tester.Scores.Select(i => i.Item2)));
                    writer.Flush();
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message, tester._logFile);
                }
            }
        }

        //测试全部程序,对所有程序自动打分
        public static void ProcessScore()
        {
            //挑选出需要测试的名单
            var testList = ExtractTestList(File.ReadAllLines(BlogFile).Select(i => i.Split('\t')[0]).ToArray());
            bool header = true;
            //是否以追加形式写入文件,不用的测试结果用不同的header标识
            var isAppend = !ModeInput.Equals(Mode.Written);
            //把成绩写入文件,两次不同的测试结果以不同的header作为区分
            using (var writer = new StreamWriter(ResultFile, isAppend))
            {
                foreach (var line in testList)
                {
                    string[] param = line.Split('\t');
                    SudokuTester tester = new SudokuTester(ProjectDir, param[0]);
                    try
                    {
                        tester.GetCorrectScore();
                        if (header)
                        {
                            var arguments = tester.Scores.Select(i => i.Item1).ToList();
                            writer.WriteLine();
                            writer.Write("NumberID\t");
                            writer.WriteLine(string.Join("\t", arguments));
                            header = false;
                        }
                        writer.Write(tester.NumberId + "\t");
                        writer.WriteLine(string.Join("\t", tester.Scores.Select(i => i.Item2)));
                        writer.Flush();
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e.Message, tester._logFile);
                    }
                }
            }
        }

        public static void ProcessWrong()
        {
            //挑选出需要测试的名单
            var testList = ExtractTestList(File.ReadAllLines(BlogFile).Select(i => i.Split('\t')[0]).ToArray());
            bool header = true;
            //是否以追加形式写入文件,不用的测试结果用不同的header标识
            var isAppend = !ModeInput.Equals(Mode.Written);
            //把成绩写入文件,两次不同的测试结果以不同的header作为区分
            using (var writer = new StreamWriter(WrongFile, isAppend))
            {
                foreach (var line in testList)
                {
                    string[] param = line.Split('\t');
                    SudokuTester tester = new SudokuTester(ProjectDir, param[0]);
                    try
                    {
                        tester.GetWrongScore();
                        if (header)
                        {
                            var arguments = tester.Wrongs.Select(i => i.Item1).ToList();
                            writer.WriteLine();
                            writer.Write("NumberID\t");
                            writer.WriteLine(string.Join("\t", arguments));
                            header = false;
                        }
                        writer.Write(tester.NumberId + "\t");
                        writer.WriteLine(string.Join("\t", tester.Wrongs.Select(i => i.Item2)));
                        writer.Flush();
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e.Message, tester._logFile);
                    }
                }
            }
        }

        //Overview:挑选出需要测试的名单
        public static IEnumerable<string> ExtractTestList(string[] allStudents)
        {
            switch (ModeInput)
            {
                //直接覆盖原先的评测结果
                case Mode.Written:
                    return allStudents;
                //已有结果的不再进行评测,其他人的追加评测
                case Mode.Append:
                    var skipList = ExtractSkipList(false);
                    return allStudents.Where(i => !skipList.Contains(i));
                case Mode.SkipGreat:
                    skipList = ExtractSkipList(true);
                    return allStudents.Where(i => !skipList.Contains(i));
                default:
                    return allStudents;
            }
        }

        //Overview:返回不需要测试的名单
        public static IEnumerable<string> ExtractSkipList(bool isSkipGreat)
        {
            //如果Score.txt存在,则读取其中内容,将其按照策略读取出来
            if (File.Exists(ResultFile))
            {
                var allLines = File.ReadAllLines(ResultFile);
                var headerLine = allLines.First();
                var existScores = allLines.Skip(1);
                //如果略过已经得到优异分数的同学,则对结果进行筛选
                if (isSkipGreat)
                {
                    //不包含负数的字符串即不需要测试的名单
                    var writeLines = existScores.Where(i => !i.Contains("-")).ToArray();
                    using (StreamWriter writer = new StreamWriter(ResultFile, false))
                    {
                        writer.WriteLine(headerLine);
                        foreach (var writeLine in writeLines)
                        {
                            writer.WriteLine(writeLine);
                        }
                    }
                    return writeLines.Select(i => i.Split('\t')[0]);
                }
                return existScores.Select(i => i.Split('\t')[0]);
            }
            //否则没有不需要测试的名单
            return new string[] {};
        }
    }

    class Mode
    {
        public const string Written = "w";
        public const string Append = "a";
        public const string SkipGreat = "s";
    }
}
