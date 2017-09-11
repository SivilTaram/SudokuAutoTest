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
        public static string RepoFile = @"./RepoMap.txt";

        //Max Limit
        public static int MaxLimitTime = 600;

        static void Main(string[] args)
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
                    }
                }
                CheckFileStatus();
                switch (command)
                {
                    case "/grab":
                        GrabRepos();
                        break;
                    case "/score":;
                        ProcessScore();
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
                              "/grab -blogPath [blog file] -gitPath [git file]\n\n"+
                              "\t- 本功能用于从学生的作业中自动提取Github链接, 并将Git仓库Clone到文件夹 Projects 下, 错误日志在文件夹 Log 下, Github项目链接映射表在 RepoMap.txt 中。\n\n"+
                              "\t- 文件 [blog file] 提供学号与作业地址的对应关系, 多行分开。如不指定该参数则默认为当前目录 BlogList.txt。\n\t 每行的格式如: 031502334\thttp://cnblogs.com/easteast/p/1234.html 【分隔符为\\t】\n\n" +
                              "\t- 文件 [git file] 提供学号与Github主页的对应关系, 多行分开。如不指定该参数则默认为当前目录 GithubRepos.txt。\n\t 每行的格式如: 031502334\thttp://github.com/easteast 【分隔符为\\t】\n\n\n" +
                              "/score -blogPath [blog file] -limit [max limit second]\n" +
                              "\t- 本功能用于给学生的作业进行评分,并记录每份作业在不同测试数据下耗费的时间。最终生成的评分文件为 Scores.txt, 可直接复制到Excel中使用。\n\n" +
                              "\t- 文件 [blog file] 提供学号与作业地址的对应关系, 多行分开。如不指定该参数则默认为当前目录 BlogList.txt。\n\t每行的格式如: 031502334\thttp://cnblogs.com/easteast/p/1234.html 【分隔符为\\t】\n\n" +
                              "\t- 数字 [limit second] 指定效率测试运行的最大时长, 默认为 600秒 ");
        }

        //检查文件状态,确保文件存在
        public static void CheckFileStatus()
        {
            ReCreateDir(LogDir);
            ReCreateDir(ProjectDir);
            if(!File.Exists(BlogFile))
            {
                throw new Exception("作业文件路径不正确!");
            }
            if (!File.Exists(GithubFile))
            {
                throw new Exception("Git仓库指定文件路径不正确!");
            }
        }

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

        //分析博客,自动获取Github仓库链接并存储至指定目录
        public static void GrabRepos()
        {
            //把每个学生的Github Repo都存储在文件夹BASE_DIR下,并以学号命名文件夹
            GitRepoHandler handler = new GitRepoHandler(ProjectDir);
            Console.WriteLine(" 开始爬取博客并匹配生成Github项目目录 ...");
            handler.GetAllGithubRepos();
        }

        //测试程序,自动打分
        public static void ProcessScore()
        {
            //遍历每个学生的学号, 并计算其分数
            List<SudokuTester> testList = new List<SudokuTester>();
            var lines = File.ReadAllLines(BlogFile);
            bool header = true;
            //Write file to txt
            using (var writer = new StreamWriter(ResultFile, false))
            {
                foreach (var line in lines)
                {
                    string[] param = line.Split('\t');
                    SudokuTester tester = new SudokuTester(ProjectDir, param[0]);
                    tester.GetCorrectScore();
                    if (header)
                    {
                        var arguments = tester.Scores.Select(i => i.Item1).ToList();
                        writer.Write("NumberID\t");
                        writer.WriteLine(string.Join("\t", arguments));
                        header = false;
                    }
                    writer.Write(tester.NumberId + "\t");
                    writer.WriteLine(string.Join("\t", tester.Scores.Select(i => i.Item2)));
                    writer.Flush();
                }
            }
        }
    }
}
