using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Natural_Language_Translator
{
	internal class Program
	{
		public enum HumanLanguage
		{
			中文,
			英文,
			日文
		}

		static void Main(string[] args)
		{
			// 检查是否提供了足够的参数
			if (args.Length < 6)
			{
				Console.WriteLine("请提供以下参数：人类语言类型、目标编程语言、分割符配置文件路径、自然语言格式配置文件路径、程序语言格式表文件路径、自然语言所写的程序文件路径。");
				return;
			}

			// 获取命令行参数
			string humanLanguageStr = args[0];
			string targetProgrammingLanguage = args[1];
			string delimitersFilePath = args[2];
			string natureLanguagePatternFilePath = args[3];
			string targetProgrammingLanguagePatternFilePath = args[4];
			string inputFilePath = args[5];

			// 检查文件是否存在
			if (!File.Exists(delimitersFilePath) || !File.Exists(natureLanguagePatternFilePath) || !File.Exists(inputFilePath))
			{
				Console.WriteLine("提供的文件路径中有一个或多个文件不存在。");
				return;
			}

			// 解析人类语言类型
			if (!Enum.TryParse(humanLanguageStr, out HumanLanguage humanLanguage))
			{
				Console.WriteLine("无效的人类语言类型。");
				return;
			}

			ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

			// 读取分割符配置文件
			var delimiters = ReadDelimitersConfig(delimitersFilePath, humanLanguage);

			// 读取自然语法配置文件
			var natureLanguageConfig = ReadHumanLanguageConfig(natureLanguagePatternFilePath, humanLanguage);

			// 读取程序语言表
			var targetProgrammingLanguageConfig = ReadTargetProgrammingLanguageConfig(targetProgrammingLanguagePatternFilePath, targetProgrammingLanguage);

			// 读取代码文件
			string input = File.ReadAllText(inputFilePath);

			// 使用读取到的分割符分割输入
			string[] lines = input.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

			foreach (var line in lines)
			{
				string trimmedLine = line.Trim();
				trimmedLine = Regex.Replace(trimmedLine, @"\s+", " ");

				foreach (var entry in natureLanguageConfig)
				{
					string identifier = entry.Key;
					string regexPattern = entry.Value.Item1;
					List<string> meanings = entry.Value.Item2;

					// 将 {变量名} 转换为 \s+(\w+)\s+
					regexPattern = Regex.Replace(regexPattern, @"\{(\w+)\}", match => @"\s*(\w+)\s*");

					Regex regex = new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
					Match match = regex.Match(trimmedLine);
					if (match.Success)
					{
						// 获取对应的代码生成模式
						if (targetProgrammingLanguageConfig.TryGetValue(identifier, out string codePattern))
						{
							// 替换占位符生成代码
							string code = codePattern;
							for (int i = 0; i < meanings.Count; i++)
							{
								string meaning = meanings[i];
								string replacement = match.Groups[i + 1].Value; // Groups[0] 是整个匹配，从 Groups[1] 开始是捕获组
								code = code.Replace($"{{{meaning}}}", replacement);
							}
							Console.WriteLine(code);
						}
						else
						{
							throw new Exception("code pattern not valid for :" + identifier);
						}
					}
				}
			}
		}

		static char[] ReadDelimitersConfig(string filePath, HumanLanguage humanLanguage)
		{
			using (var package = new ExcelPackage(new FileInfo(filePath)))
			{
				var worksheet = package.Workbook.Worksheets[0];
				var rows = worksheet.Dimension.Rows;

				for (int row = 2; row <= rows; row++)
				{
					var language = worksheet.Cells[row, 1].Text;
					if (language == humanLanguage.ToString())
					{
						var delimiters = new List<char>();
						for (int col = 2; col <= worksheet.Dimension.Columns; col++)
						{
							var delimiter = worksheet.Cells[row, col].Text;
							if (string.IsNullOrEmpty(delimiter))
							{
								break;
							}
							delimiters.Add(delimiter[0]);
						}
						return delimiters.ToArray();
					}
				}
			}

			throw new Exception("Human Language Type's Delimiter is not set:" + humanLanguage.ToString());
		}

		static Dictionary<string, Tuple<string, List<string>>> ReadHumanLanguageConfig(string filePath, HumanLanguage humanLanguage)
		{
			var config = new Dictionary<string, Tuple<string, List<string>>>();

			using (var package = new ExcelPackage(new FileInfo(filePath)))
			{
				var worksheet = package.Workbook.Worksheets[humanLanguage.ToString()];
				var rows = worksheet.Dimension.Rows;

				for (int row = 2; row <= rows; row++)
				{
					var identifier = worksheet.Cells[row, 1].Text;
					var regexPattern = worksheet.Cells[row, 3].Text;

					if (identifier != null && regexPattern != null)
					{
						var meanings = new List<string>();
						for (int col = 4; ; col++)
						{
							var meaning = worksheet.Cells[row, col].Text;
							if (string.IsNullOrEmpty(meaning))
							{
								break;
							}

							meanings.Add(meaning);
						}
						config[identifier] = new Tuple<string, List<string>>(regexPattern, meanings);
					}
				}
			}

			return config;
		}

		static Dictionary<string, string> ReadTargetProgrammingLanguageConfig(string filePath, string targetLanguage)
		{
			var config = new Dictionary<string, string>();

			using (var package = new ExcelPackage(new FileInfo(filePath)))
			{
				var worksheet = package.Workbook.Worksheets[targetLanguage];
				var rows = worksheet.Dimension.Rows;

				for (int row = 2; row <= rows; row++)
				{
					var identifier = worksheet.Cells[row, 1].Text;
					var codePattern = worksheet.Cells[row, 2].Text;

					if (identifier != null && codePattern != null)
					{
						config[identifier] = codePattern;
					}
				}
			}

			return config;
		}
	}
}