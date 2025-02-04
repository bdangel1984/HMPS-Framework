# Natural Language Translator

## Description
This is a program project that defines a programming language using natural language and translates it into specified program statements, built on C#.

## Getting Started

### Prerequisites
- **Microsoft Visual Studio Community 2022 (64-bit)**
- **EPPlus**

### Installation
- No `.sln` file is provided. Please create a new project and add the files manually. This project uses a C# Console application.
- **EPPlus**: Install via Manage NuGet Packages.

### Usage
1. The parameters need to be provided in sequence:
    ```plaintext
    中文
    C#
    "..\05 - Natural Language Delimiters.xlsx"
    "..\03 - Nature Language Programming Pattern Table.xlsx"
    "..\04 - Programming Language Format Table.xlsx"
    "..\Natural Programming Language.txt"
    ```
    - **Explanation**:
        - **Parameter 1**: The natural language used for programming.
        - **Parameter 2**: The type of programming language to be generated.
        - **Parameter 3**: The delimiter definition table for natural language.
        - **Parameter 4**: The natural language programming pattern table.
        - **Parameter 5**: The programming language format table.
        - **Parameter 6**: The program code written in natural language.
2. The translated statements will be output to the Console after execution.