
# Usages:

## 提取Github项目链接

**命令行**：/grab -blogPath [blog file] -gitPath [git file]

- 本功能用于从学生的作业中自动提取Github链接, 并将Git仓库Clone到文件夹 Projects 下, 错误日志在文件夹 Log 下, Github项目链接映射表在 RepoMap.txt 中。
- 文件 [blog file] 提供学号与作业地址的对应关系, 多行分开。如不指定该参数则默认为当前目录 BlogList.txt。
    每行的格式如: 031502334        http://cnblogs.com/easteast/p/1234.html 【分隔符为\t】
- 文件 [git file] 提供学号与Github主页的对应关系, 多行分开。如不指定该参数则默认为当前目录 GithubRepos.txt。
    每行的格式如: 031502334        http://github.com/easteast 【分隔符为\t】

**使用示例**：SudoAutoTest.exe /grab

## 自动评测测试点时长

**命令行**：/score -blogPath [blog file] -limit [max limit second]

- 本功能用于给学生的作业进行评分,并记录每份作业在不同测试数据下耗费的时间。最终生成的评分文件为 Scores.txt, 可直接复制到Excel中使用。
- 文件 [blog file] 提供学号与作业地址的对应关系, 多行分开。如不指定该参数则默认为当前目录 BlogList.txt。
每行的格式如: 031502334 http://cnblogs.com/easteast/p/1234.html 【分隔符为\t】
- 数字 [limit second] 指定效率测试运行的最大时长, 默认为 600秒

**使用示例**：SudoAutoTest.exe /score

## 使用流程

在实际使用时, 先使用 /grab 再直接使用 /score 即可。在 DEMO 中有已经编译好的版本，目前仅支持 Windows/.NET45 平台。