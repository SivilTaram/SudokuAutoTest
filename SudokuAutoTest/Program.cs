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
        public const string BaseDir = @"H:\福大软工\PersonalProject\Projects";
        public const string BlogFile = @"H:\福大软工\PersonalProject\BlogTest.txt";
        public const string GithubFile = @"H:\福大软工\PersonalProject\GithubRepos.txt";
        public const string ResultFile = @"H:\福大软工\PersonalProject\Scores.txt";
        public const string RepoFile = @"H:\福大软工\PersonalProject\RepoMap.txt";

        static void Main(string[] args)
        {
            Process();
        }

        public static void Process()
        {
            //把每个学生的Github Repo都存储在文件夹BASE_DIR下,并以学号命名文件夹
            GitRepoHandler handler = new GitRepoHandler(BaseDir);
            handler.GetAllGithubRepos();
            //遍历每个学生的学号, 并计算其分数
            List<SudokuTester> testList = new List<SudokuTester>();
            var lines = File.ReadAllLines(BlogFile);
            foreach (var line in lines)
            {
                string[] param = line.Split('\t');
                SudokuTester tester = new SudokuTester(BaseDir, param[0]);
                tester.GetCorrectScore();
                testList.Add(tester);
            }
            //Write file to txt
            using (var writer = new StreamWriter(ResultFile, false))
            {
                var arguments = testList.First().Scores.Select(i => i.Item1).ToList();
                writer.Write("NumberID\t");
                writer.WriteLine(string.Join("\t",arguments));
                //Write results to file
                foreach (var tester in testList)
                {
                    writer.Write(tester.NumberId+"\t");
                    writer.WriteLine(string.Join("\t", tester.Scores.Select(i => i.Item2)));
                }
            }
        }
    }
}
