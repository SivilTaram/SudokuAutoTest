
# Usages:

## 提取Github项目链接

**命令行**：/grab -blogPath [blog file] -gitPath [git file]

- 本功能用于从学生的作业中自动提取Github链接, 并将Git仓库Clone到文件夹 Projects 下, 错误日志在文件夹 Log 下, Github项目链接映射表在 RepoMap.txt 中。
- 文件 [blog file] 提供学号与作业地址的对应关系, 多行分开。如不指定该参数则默认为当前目录 BlogList.txt。
    每行的格式如: 031502334        http://cnblogs.com/easteast/p/1234.html 【分隔符为\t】
- 文件 [git file] 提供学号与Github主页的对应关系, 多行分开。如不指定该参数则默认为当前目录 GithubRepos.txt。
    每行的格式如: 031502334        http://github.com/easteast 【分隔符为\t】
- 文本 [score mode] 指定测试选用的模式，目前提供三种选择，如该参数不填则默认为s模式：
    - a : 跳过当前 Projects目录下已有工程, 分析其他同学的博客 ,并将项目克隆到本地。
    - w : 将 Projects 文件夹重命名,爬取所有学生的博客并将项目克隆到 Projects文件夹下。
    - s : 已有正确测试结果的不再重新爬取，只测试存在错误情况的项目。
- 学号 [number id] (可选参数) 提供单个学号, 当本参数存在时, 将只抓取单个同学的博客并重新克隆工程。\n\n" +

**使用示例**：SudoAutoTest.exe /grab -mode w

## 自动评测测试点时长

**命令行**：/score -blogPath [blog file] -limit [max limit second]

- 本功能用于给学生的作业进行评分,并记录每份作业在不同测试数据下耗费的时间。最终生成的评分文件为 Scores.txt, 可直接复制到Excel中使用。
- 文件 [blog file] 提供学号与作业地址的对应关系, 多行分开。如不指定该参数则默认为当前目录 BlogList.txt。
每行的格式如: 031502334 http://cnblogs.com/easteast/p/1234.html 【分隔符为\t】
- 数字 [limit second] 指定效率测试运行的最大时长, 默认为 600秒
- 文本 [score mode] 指定测试选用的模式，目前提供三种选择，如该参数不填则默认为s模式：
    - a : 跳过当前已有评分,对其他同学进行测试,将结果追加写入score.txt中。
    - w : 全部重新测试，生成新的score.txt文件。
    - s : 已有正确测试结果的不再重新测试，只测试不正确的。
- 学号 [number id] (可选参数) 提供单个学号, 当本参数存在时，将只测试单个同学的工程，并将结果存储至 `学号-score.txt` 中。\n\n"+

**使用示例**：SudoAutoTest.exe /score -mode s

## 使用流程

在实际使用时, 先使用 /grab 再直接使用 /score 即可。在 DEMO 中有已经编译好的版本，目前仅支持 Windows/.NET45 平台。

## 结果说明

在 /score 命令执行成功后，评分表 Score.txt 中会记录测试程序的成绩。如果该项为正值，即为该项测试花费的时间；如果该项为负值，即为出错。出错码对应表如下：

- NoSudokuExe = -1,
- NoGeneratedSudokuTxt = -2,
- RuntimeError = -3,
- OutOfTimeCloseExe = -4,
- RunOutOfTime = -5,
- RepeatedPanels = -6,
- SudokuPanelInvalid = -7,
- NotEnoughCount = -8,
- CanNotDoEfficientTest = -9

错误的细节与描述等均在 `{学号}-log.txt` 中可以找到，方便追查是程序原因还是学生自身的错误。