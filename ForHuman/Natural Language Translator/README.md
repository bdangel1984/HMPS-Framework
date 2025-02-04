# 自然语言翻译器

## 描述
这是一个用自然语言定义编译语言，并将其翻译成指定程序语句的程序项目，建立在C#上

## 开始
### 前提需求
- Microsoft Visual Studio Community 2022 (64 位) 
- EPPlus

### 安装
- 暂未提供sln文件，请自行建立工程并将文件加入，使用的是C# Console项目
- **EPPlus** ，使用Manage NuGet Packages安装

### 使用说明
1. 需要依次放入参数
```
中文
C#
"..\05 - Natural Language Delimiters.xlsx"
"..\03 - Nature Language Programming Pattern Table.xlsx"
"..\04 - Programming Language Format Table.xlsx"
"..\Natural Programming Language.txt"
```
- 说明：
  - 参数1：编程使用的自然语言
  - 参数2：需要写出的代码的语言类别
  - 参数3：自然语言的分隔符定义表
  - 参数4：自然语言编程格式表
  - 参数5：程序语言格式表
  - 参数6：用自然语言所编写的程序代码
2. 会在运行后在 Console 输出转换后的语句