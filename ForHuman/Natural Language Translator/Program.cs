using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Natural_Language_Translator
{
	internal partial class Program
	{
		static void Main(string[] args)
		{
			if (args.Length != 1)
			{
				Console.WriteLine("请指定输入文件路径。");
				return;
			}

			string inputFilePath = args[0];
			string outputFilePath = Path.ChangeExtension(inputFilePath, ".cs");

			try
			{
				// 读取文件内容
				string content = File.ReadAllText(inputFilePath);

				// 解析自然语言代码并生成C#代码
				string csharpCode = ParseNaturalLanguage(content);

				// 将结果写入输出文件
				//File.WriteAllText(outputFilePath, csharpCode);
				Console.WriteLine(csharpCode);

				Console.WriteLine($"转换完成，结果已保存到 {outputFilePath}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"发生错误：{ex.Message}");
			}
		}


		static string ParseNaturalLanguage(string nlCode,int cutPreTabInEveryLine = 0)
		{
			// 按行分割内容
			var lines = nlCode.Split('\n').Select(line => line.TrimEnd()).ToArray();

			bool isSomeLineNotEnoughTab = false;
			List<string> b = new();
			List<string> b2 = new();
			foreach (var v in lines)
			{
				b.Add(new string(v));
			}

			for (int i = 0; i < cutPreTabInEveryLine; i++)
			{
				b2 = new();
				string nv;
				foreach (var v in b)
				{
					nv = new string(v);
					if (nv.StartsWith("\t")) nv = new string(nv.Substring(1, v.Length - 1));
					else
					{
						isSomeLineNotEnoughTab = true;
						break;
					}
					b2.Add(nv);
				}

				if (true == isSomeLineNotEnoughTab) break;
				b = b2;
			}

			if(0 < cutPreTabInEveryLine && false == isSomeLineNotEnoughTab)
			{
				lines = b.ToArray();
			}

			// 用于存储解析结果
			var result = new List<string>();

			for (int i = 0; i < lines.Length; )
			{
				string line = lines[i];
				int level = line.Count(c => c == '\t');
				string content = line.Trim();

				if (content.StartsWith("声明变量"))
				{
					ParseVariableDeclaration(lines, ref i, level, result);
				}
				else if (content.StartsWith("如果"))
				{
					ParseIfStatement(lines, ref i, level, result);
				}
				else if (content.StartsWith("循环操作"))
				{
					ParseForLoop(lines, ref i, level, result);
				}
				else if (content.StartsWith("调用函数"))
				{
					ParseMethodCall(lines, ref i, level, result);
				}
				else if (content.StartsWith("函数定义"))
				{
					ParseFunctionDefinition(lines, ref i, level, result);
				}
				else if (content.StartsWith("数组声明"))
				{
					ParseArrayDeclaration(lines, ref i, level, result);
				}
				else if (content.StartsWith("代码块开始"))
				{
					result.Add(GetBlockContent(lines, ref i, 0));
				}
				else
				{
					if (0 < content.Length) result.Add(content);
					i++;
				}
			}

			return string.Join("\n", result);
		}

		static void ParseVariableDeclaration(string[] lines, ref int i, int level, List<string> result)
		{
			string type = "", name = "", value = "";
			bool isFirstLine = true;

			while (i < lines.Length)
			{
				string line = lines[i];
				int currentLevel = line.Count(c => c == '\t');
				string content = line.Trim();

				if (isFirstLine)
				{
					isFirstLine = false;
					i++;
					continue;
				}
				else if (currentLevel <= level)
				{
					break;
				}

				if (content.StartsWith("类型"))
				{
					type = GetNextLineValue(lines, ref i, currentLevel);
				}
				else if (content.StartsWith("名称"))
				{
					name = GetNextLineValue(lines, ref i, currentLevel);
				}
				else if (content.StartsWith("赋值"))
				{
					value = GetNextLineValue(lines, ref i, currentLevel);
				}else
				{
					throw new Exception("unknown command:" + content);
				}

				i++;
			}

			i++;
			result.Add($"{type} {name} = {value};");
		}

		static void ParseIfStatement(string[] lines, ref int i, int level, List<string> result)
		{
			string condition = "", block = "";
			bool isFirstLine = true;

			while (i < lines.Length)
			{
				string line = lines[i];
				int currentLevel = line.Count(c => c == '\t');
				string content = line.Trim();

				if (isFirstLine)
				{
					isFirstLine = false;
					i++;
					continue;
				}
				else if (currentLevel <= level)
				{
					break;
				}

				if (content.StartsWith("条件为真"))
				{
					condition = GetNextLineValue(lines, ref i, currentLevel);
				}
				else if (content.StartsWith("代码块开始"))
				{
					block = GetBlockContent(lines, ref i, currentLevel);
				}
				else
				{
					throw new Exception("unknown command:" + content);
				}

				i++;
			}

			i++;
			result.Add($"if ({condition}) \n{block}\n");
		}

		static void ParseForLoop(string[] lines, ref int i, int level, List<string> result)
		{
			string init = "", condition = "", increment = "", block = "";
			bool isFirstLine = true;

			while (i < lines.Length)
			{
				string line = lines[i];
				int currentLevel = line.Count(c => c == '\t');
				string content = line.Trim();

				if (isFirstLine)
				{
					isFirstLine = false;
					i++;
					continue;
				}
				else if (currentLevel <= level)
				{
					break;
				}

				if (content.StartsWith("初始操作"))
				{
					init = GetNextLineValue(lines, ref i, currentLevel);
				}
				else if (content.StartsWith("条件判断"))
				{
					condition = GetNextLineValue(lines, ref i, currentLevel);
				}
				else if (content.StartsWith("循环结束操作"))
				{
					increment = GetNextLineValue(lines, ref i, currentLevel);
				}
				else if (content.StartsWith("代码块开始"))
				{
					block = GetBlockContent(lines, ref i, currentLevel);
				}
				else
				{
					throw new Exception("unknown command:" + content);
				}

				i++;
			}

			i++;
			result.Add($"for ({init}; {condition}; {increment}) \n{block}\n");
		}

		static void ParseMethodCall(string[] lines, ref int i, int level, List<string> result)
		{
			string methodName = "";
			List<string> parameters = new List<string>();
			bool isFirstLine = true;

			while (i < lines.Length)
			{
				string line = lines[i];
				int currentLevel = line.Count(c => c == '\t');
				string content = line.Trim();

				if (isFirstLine)
				{
					isFirstLine = false;
					i++;
					continue;
				}
				else if (currentLevel <= level)
				{
					break;
				}

				if (content.StartsWith("方法名"))
				{
					methodName = GetNextLineValue(lines, ref i, currentLevel);
				}
				else if (content.StartsWith("参数列表"))
				{
					i = GetParameterList(lines, i, currentLevel, parameters);
				}
				else
				{
					throw new Exception("unknown command:" + content);
				}

				i++;
			}

			i++;
			result.Add($"{methodName}({string.Join(", ", parameters)});");
		}

		static void ParseFunctionDefinition(string[] lines, ref int i, int level, List<string> result)
		{
			string functionName = "";
			string returnType = "";
			List<string> parameters = new List<string>();
			string block = "";
			bool isFirstLine = true;

			while (i < lines.Length)
			{
				string line = lines[i];
				int currentLevel = line.Count(c => c == '\t');
				string content = line.Trim();

				if (isFirstLine)
				{
					isFirstLine = false;
					i++;
					continue;
				}
				else if (currentLevel <= level)
				{
					break;
				}

				if (content.StartsWith("方法名"))
				{
					functionName = GetNextLineValue(lines, ref i, currentLevel);
				}
				else if (content.StartsWith("返回类型"))
				{
					returnType = GetNextLineValue(lines, ref i, currentLevel);
				}
				else if (content.StartsWith("参数列表"))
				{
					i = GetParameterList(lines, i, currentLevel, parameters);
				}
				else if (content.StartsWith("代码块开始"))
				{
					block = GetBlockContent(lines, ref i, currentLevel);
				}
				else
				{
					throw new Exception("unknown command:" + content);
				}

				i++;
			}

			i++;
			// 格式化输出
			//string formattedBlock = block.Replace("\n", "\n    ");
			result.Add($"{returnType} {functionName}({string.Join(", ", parameters)})\n{block}");
		}

		static void ParseArrayDeclaration(string[] lines, ref int i, int level, List<string> result)
		{
			string type = "", name = "", length = "";
			bool isFirstLine = true;

			while (i < lines.Length)
			{
				string line = lines[i];
				int currentLevel = line.Count(c => c == '\t');
				string content = line.Trim();

				if (isFirstLine)
				{
					isFirstLine = false;
					i++;
					continue;
				}
				else if (currentLevel <= level)
				{
					break;
				}

				if (content.StartsWith("类型"))
				{
					type = GetNextLineValue(lines, ref i, currentLevel);
				}
				else if (content.StartsWith("变量名"))
				{
					name = GetNextLineValue(lines, ref i, currentLevel);
				}
				else if (content.StartsWith("长度"))
				{
					length = GetNextLineValue(lines, ref i, currentLevel);
				}
				else
				{
					throw new Exception("unknown command:" + content);
				}

				i++;
			}

			i++;
			result.Add($"{type}[] {name} = new {type}[{length}];");
		}

		static int GetParameterList(string[] lines, int currentLineIdx, int currentLevel, List<string> parameters, bool isTrim = true)
		{
			int curLineIdx = currentLineIdx + 1; // Move to the next line
			while (curLineIdx < lines.Length)
			{
				string line = lines[curLineIdx];
				int nextLevel = line.Count(c => c == '\t');
				if (nextLevel <= currentLevel)
				{
					break; // End of parameter list
				}

				if(true == isTrim)parameters.Add(line.Trim());
				else parameters.Add(line);

				curLineIdx++;
			}

			return curLineIdx - 1; // Return the last processed line index
		}

		static string GetNextLineValue(string[] lines, ref int i, int level)
		{
			i++;
			if (i < lines.Length)
			{
				string nextLine = lines[i];
				int nextLevel = nextLine.Count(c => c == '\t');
				if (nextLevel > level)
				{
					return nextLine.Trim();
				}
			}
			throw new Exception("无法获取值，格式可能不正确。");
		}

		static string GetBlockContent(string[] lines, ref int i, int level)
		{
			StringBuilder blockContent = new StringBuilder();
			i++; // Skip the current line

			while (i < lines.Length)
			{
				string line = lines[i];
				int currentLevel = line.Count(c => c == '\t');
				string content = line.Trim();

				if (content == "代码块结束" || currentLevel <= level)
				{
					i++;
					break;
				}

				List<string> parameters = new();
				parameters.Add(line);
				i = GetParameterList(lines, i, currentLevel - 1 , parameters, false);

				{
					string subFullText = string.Join("\n", parameters.ToArray());
					//Console.Write(subFullText);
					string nestedCode = ParseNaturalLanguage(subFullText, currentLevel);

					var nblines = nestedCode.Split("\n");
					string nbl = "";
					foreach (var v in nblines)
					{
						if (0 < v.Trim().Length)
						{
							nbl += ("\t" + v) + "\n";
						}
					}

					blockContent.AppendLine("{").AppendLine(nbl.Substring(0, nbl.Length - 1)).AppendLine("}");
				}
				i++;
			}

			return blockContent.ToString().TrimEnd();
		}
	}
}