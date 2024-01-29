using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace Lexer_01
{
    // Программа: Лексер для языка Rust
    // Назначение: Разбивает входящий файл на языке Rust, на лексемы
    // Версия: 5.0 от 29.01.2024
    // Автор: Gogik Ortey

    // Все текстовые файлы (Input.txt, Output.txt и другие)
    // лежат в конечной папке проекта: ...\Lexer_01\Lexer_01\bin\Debug\netcoreapp3.1\[Вот тут]
    // Но их можно легко открыть, запустив .cmd файлы, которые лежат в начальной папке этого проекта 

    /*
        --- ОГЛАВЛЕНИЕ (примерное) ---

        Основная программа ----------- ~ 125 строчка кода
        3. Кодогенератор ------------- ~ 300 строчка кода
        2. Парсер* ------------------- ~ 800 строчка кода
        1. Лексер -------------------- ~ 2000 строчка кода

        * Семантический анализатор встроен в Парсер (код вывода исключений)
        
        Всего ~ 3300 строчек кода

    */

    // Все выводимые исключения, и ситуации, при которых они выводятся:

    /*      
        | Код ошибки           | Краткое описание                                    |
        | -------------------- | --------------------------------------------------- |
        | OK                   | Успешная трансляция программы                       |

        | Init_Type            | Отсутствие типа у инициализируемой переменной       |
        | Const_Var_Cange      | Изменение значения константной переменной           |
        | Err_Read_Inp_File    | Входной файл не был прочитан, или массив строк пуст |
        | Unsp_KyWords         | Обнаружены неподдерживаемые ключевые слова          |
        | Def_Symv             | Встречен недопустимый символ                        |
        | Err_Def_Float_Number | Неверная инициализация значения float               |
        | Inv_Type_Op_Ins_Cond | Неверный тип операции внутри условия                |

        А так-же есть общий тип ошибок:

        | Pars_Err             | Синтаксическая или семантическая ошибка             |

        Включает в себя такие ошибки:
            
            • Переменная не была объявлена
            • Повторная инициплизация переменной
            • Несоответствие типов объявляемой переменной, и присваевомого ей значения
            • В объявлении переменной обнаружен неподдерживаемый тип данных

            • Во всех остальных случая выводится сообщение: "Error: Синтаксическая ошибка"
    */

    class Program
    {
        // Все важные настройки кода:

        // Транслятор удаляет неиспользуемые переменные?
        public static bool isDeleteUnuseVar = false;

        // Должны ли мы оборачивать наш код в функцию main(), на выходе
        public static bool isStrictAndCompleteOutput = true;

        // Включает вывод обрабатываемых лексем в парсере
        public static bool isDebugPrint = false;

        // По большей части нужен, для задания массивов строк    
        public static int maxCountOfRowsInInputFile = 200;

        // ------------------------------------------------

        static void PrintStartSettings()
        {
            print("Основные настройки: ");
            print("");
            print("> Оборачиваем ли мы на выходе наш код в функцию main : " + (isStrictAndCompleteOutput ? "Yes" : "No"));
            print("> Транслятор удаляет неиспользуемые переменные? : " + (isDeleteUnuseVar ? "Yes" : "No"));
            print("");
        }

        // ------------------------------------------------

        static StreamWriter writer = new StreamWriter("Output.txt"); // Записываю весь вывод консоли в файл
        static StreamWriter writerCpp = new StreamWriter("C-plus-plus code.txt"); // Записываю в отдельный текстовый документ код C++
        static int countCppStr = 0;

        #region MyPrint
        // Реализую свои методы вывода в консоль (для удобства)
        public static void print<Type>(Type Input)
        {
            Console.WriteLine(Input);
            writer.WriteLine(Input);
            if (isCodeGeneration)
            {
                if (countCppStr < 5) countCppStr++; // Пропускаю первые 5 строк
                else
                {
                    writerCpp.WriteLine(Input);
                }
            }
        }

        public static void println<Type>(Type Input)
        {
            // Печать строки без переноса каретки
            Console.Write(Input);
            writer.Write(Input);
            if (isCodeGeneration) writerCpp.Write(Input);
        }
        #endregion







        /*/////////////////////////////////////////////////////////////////////////////////////////
        //                                                                                       //
        //                                                                                       //
        //                                         MAIN                                          //
        //                                                                                       //
        //                                                                                       //
        //////////////////////////////////////////////////////////////////////////////////////// */

        // Описание:
        /*  
            
            Сначала загружает входной файл, Лексером
            Затем обрабатывает его Лексером
            
            Дальше запускает парсер [На основе массива распознанных лексем outpTable]
            
            И если семантический анализатор не обнаружил ошибок в структуре кода,
            то запускается кодогенератор [На основе дерева разбора парсера]
        
        */

        static void Main()
        {
            Stopwatch st = new Stopwatch(); // Создаём, и запускаем таймер
            st.Start();

            StartMassege();

            PrintStartSettings();

            // ----- Основной код лексера:

            // Лексер - принимает на вход изначальный файл, с кодом на языке Rust
            // и разбивает его на лексемы - ключевые слова

            MainLexer MainLexer = new MainLexer();

            MainLexer.OpenInputFiles("");   // Загружаем файл в массивы программы

            if(err_empty_input_file == true) { ExitFromProgramm(); return; }

            MainLexer.processingLine();     // Построково обрабатываем

            List<string> outpTable = MainLexer.outpTable; // В этом массиве будут хранится все разобранные лексемы

            // Если во входном файле распознаны неподдерживаемые лексемы, то мы указываем их, и прекращаем обработку фалйа
            if (MainLexer.isRecognisedUnsupportedLexems) { ExitFromProgramm(); return; }
            
            // Если во входном файле были распознаны посторонние символы (♥♦♣♠)
            if (MainLexer.isErrSymbol) { ExitFromProgramm(); return; }

            // ----- Основной код парсера и семантического анализатора:

            // Парсер - строит дерево разбора входного кода
            // Из этого дерева, мы потом будем собирать выходной код

            MainParser MainParser = new MainParser();

            MainParser.SimpleOutput_of_all_Lexems(outpTable);

            // Семантический анализатор проверяет, что бы в нашем входном разобранном коде не было ошибок в структуре кода
            // Если он обнаружил ошибку - то он останавливает построение дерево разбора, и указывает, где ошибка

            bool semanticAnalExeption = MainParser.semanticAnalExeption;
            string[] outpFileString = MainParser.outpFileString;

            if (MainParser.isRecError) { ExitFromProgramm(); return; } // Если обнаружена ошибка, в парсере

            if (semanticAnalExeption == false)
            {
                // ----- Основной код кодогенератора:

                // Кодогенератор - создаёт выходной код. Язык выходного кода - С++
                // Будет работать, только если семантический анализатор не обнаружил ошибок во входном коде

                isCodeGeneration = true;
                CodeGen CodeGen = new CodeGen();
                CodeGen.mainVoid(outpFileString);

                if (CodeGen.isCodeIncludeErrors) { ExitFromProgramm(); return; }
                isCodeGeneration = false;
            }

            PrintDeletedVars(); // Выводит удалённые переменные, если они есть
            PrintSeparator();
            st.Stop();
            println("\nВремя выполнения программы: " + (Math.Round(st.Elapsed.TotalSeconds, 2))); print(" секунд");

            ExitFromProgramm();
            writer.Close();
            writerCpp.Close();
        }

        //
        // -----
        //

        public static void PrintSeparator()
        {
            // Печатает разделитель, одинаковой длинны, для каждого вызова

            //print("-------------------------");
            //print("-----------------------------");
            print("---------------------------------------------------");
            //print("___________________________________________________");
        }

        public static string errorDescr = "Успешная трансляция программы"; // Описание ошибки
        public static string ExitCode = "OK"; // Код ошибки. По умолчанию 0 - без ошибок

        public static void ExitFromProgramm()
        {
            print("");
            print("_______________________________________");
            print("_______________________________________");
            print("_______________________________________");
            print("");
            print("Программа завершилась с кодом: " + ExitCode);
            print("");
            if (ExitCode != "OK")
            {
                print("Трансляция программы остановлена с исключением: \n" + errorDescr);                
            }
            else print(errorDescr);

            for (int i = 0; i < 4; i++) { print(""); }
        }

        public static void StartMassege()
        {
            print("Программа: Транслятор с языка Rust на язык C++");
            print("v5.0 Final");
            print("Автор: Gogik Ortey");
            print(" ");
        }

        public static void PrintDeletedVars()
        {
            if ((DeletedVar.Count > 0) && (isDeleteUnuseVar == true))
            {
                print("");
                PrintSeparator();
                print("Оптимизация кода:");
                print("Все удалённые неиспользуемые переменные:");
                print("");
                foreach (var item in DeletedVar)
                {
                    print("> " + item);
                }
                print("");
            }

            //foreach (KeyValuePair<string, bool> pair in allVars_isConst)
            //{
            //    // Вывожу все элементы списка allVarsDescr1
            //    print($"Key: {pair.Key}, Value: {pair.Value}");
            //}
        }







        /*/////////////////////////////////////////////////////////////////////////////////////////
        //                                                                                       //
        //                                                                                       //
        //                                      CODE GEN                                         //
        //                                                                                       //
        //                                                                                       //
        //////////////////////////////////////////////////////////////////////////////////////// */

        // Описание:
        /*  
            
            Кодогенератор включается в работу последним - он получает на вход массив 
            строк - Дерево разбора парсера. Такое-же, которое выводится в консоль

            Затем, он построково обрабатывает это дерево, и ищет ключевые слова
            используя метод .Contains()
        
            Например, outpFileString[i].Contains("FUNCTION") - сработает, если кодогенератор дойдёт до строчки,
            в которой будет ключевое слово FUNCTION
        
        */

        public static bool isCodeGeneration = false;  // Показывает, что сейчас работает процедура кодогенерации
        public static bool isStringUsage = false;     // Если в коде используются строки

        public static List<string> DeletedVar = new List<string>();  // Удалённые переменные

        // Кодогенератор - генерирует выходной код на языке C++
        // Используя распознанные ключевые слова
        public class CodeGen
        {
            public string[] outpFileString = new string[maxCountOfRowsInInputFile];

            public bool isProceddingConditionIf = false; // Мы сейчас обрабатываем условие?

            // Возвращает нужное количество пробелов перед выводом строки
            public string SpaseCr(int n)
            {
                string str = "";
                for (int ii = 0; ii < n; ii++) { str += "  "; }
                println(str);
                return str;
            }

            // Комбинированный метод: Сначала выводит пробелы, затем строку
            public void printLvl(string str)
            {
                string str2 = SpaseCr(lvl); print(str); 
                //outpFileString[indOnOutpMass] = str2 + str;
                //indOnOutpMass++;
            }

            public void printLvlln(string str)
            {
                SpaseCr(lvl); println(str); 
            }

            int lvl = 0;
            public bool isCodeIncludeErrors = false;
            int countStrOfCodeGen = 0; // Считает количество строк, которые выдаст кодогенератор

            /*//////////////////////////////////////////////////////////
            //                                                        //
            //           Основная процедура Кодогенератора            //
            //                                                        //
            ///////////////////////////////////////////////////////// */

            public void mainVoid(string[] outpFileS)
            {
                outpFileString = outpFileS;

                print("");
                PrintSeparator();
                print("");
                print("Кодогенератор С++");
                print("");

                bool isMainFuncUnicIntoOriginalCode = false; // Метод main() изначально был в оригинальном коде?

                if (outpFileString != null)
                {
                    for (int i = 0; i < outpFileString.Length - 1; i++)
                    {
                        try
                        {
                            // Если вы видите ошибку здесь - игнорируйте её
                            if ((outpFileString[i].Contains("FUNCTION")) && (outpFileString[i + 1].Contains("NAME : MAIN")))
                            {
                                isMainFuncUnicIntoOriginalCode = true;
                                break;
                            }
                        }
                        catch
                        {
                            break;
                            // Ошибка, уди
                        }
                    }
                }

                print("#include <iostream>");
                if (isStringUsage) print("#include <string>");
                print("using namespace std;");
                print("");

                if ((isMainInclude == false) && (isStrictAndCompleteOutput == true))
                {
                    if (isMainFuncUnicIntoOriginalCode == false)
                    {
                        print("int main()");
                        print("{");
                        lvl++;
                    }
                }

                int bufCurrLvl = 0;
                bool isBracet = false;
                bool isConstErr = false;
                bool isProcessingFuncMain = false;

                bool isDebugPrintCodeGen = false; // Вывод отладочной информации только в кодогенераторе
                bool isProcessedConditionBlock = false; // Если мы в данный момент обрабатываем условие (IF)

                /*///////////////////////////////////////////////////////////
                //                                                         //
                //            Основной цикл for Кодогенератора             //
                //                                                         //
                ////////////////////////////////////////////////////////// */

                for (int i = 0; i < outpFileString.Length; i++)
                {
                    //if (isDebugPrint) 
                    //print("-> Рассматриваем этот элемент в дереве разбора парсера: " + outpFileString[i]);
                    if (isDebugPrint || isDebugPrintCodeGen) print("-> РЭЭВДРП: " + outpFileString[i] + ", i = " + i);

                    //isProceddingConditionIf = false;

                    int www = 0; // Точка остановки для дебаггера


                    //print(outpFileString[i]);
                    if (outpFileString[i] == null)
                    {
                        if (isDebugPrint || isDebugPrintCodeGen) print("outpFileString[" + i + "] = " + outpFileString[i] + ", outpFileString[" + (i - 1) + "] = " + outpFileString[i-1] + ", outpFileString[" + (i + 1) + "] = " + outpFileString[i + 1]);
                        break;
                    }
                    if (isConstErr) break;

                    if ((outpFileString[i].Contains("FUNCTION")) && (outpFileString[i + 1].Contains("NAME : MAIN")))
                    {
                        isProcessingFuncMain = true; // Нужно для того, что бы корректно поставить return 0; в конец метода main()
                        if (isDebugPrint || isDebugPrintCodeGen) print("### Объявление основной функции main");

                        printLvlln("int main (");
                        isBracet = true;
                        countStrOfCodeGen++;
                        continue;
                    }
                    else if ((outpFileString[i].Contains("FUNCTION")) 
                        && (!(outpFileString[i].Contains("CALL_FUNCTION"))) 
                        && (!(outpFileString[i + 1].Contains("NAME : MAIN"))))
                    {
                        isProcessingFuncMain = false;                       

                        // void another_function(int x) 

                        i++;
                        string func_name = outpFileString[i].Split(':')[1].Trim(); // Получение значения справа от :
                        print("");
                        if (isDebugPrint || isDebugPrintCodeGen) print("### Объявление кастомной фунции");
                        printLvlln("void " + func_name + "(");

                        i += 2;
                        string arg_name = outpFileString[i].Split(':')[1].Trim(); // Получение значения справа от :

                        i++;
                        string arg_type = outpFileString[i].Split(':')[1].Trim(); // Получение значения справа от :

                        if (arg_type == "INT") arg_type = "int";

                        print(arg_type + " " + arg_name + ")");


                        //i++;
                        //lvl++;
                        continue;
                    }

                    if (outpFileString[i].Contains("FOR"))
                    {
                        // for (int i = 0; i < 5; ++i)

                        printLvlln("for (int ");

                        i += 2;
                        string var = outpFileString[i].Split(':')[1].Trim(); // Получение значения справа от :
                        println(var + " = ");

                        i += 1;
                        string from = outpFileString[i].Split(':')[1].Trim(); // Получение значения справа от :
                        println(from + "; " + var + " < ");

                        i += 1;
                        string to = outpFileString[i].Split(':')[1].Trim(); // Получение значения справа от :
                        print(to + "; " + var + "++)");

                        printLvl("{");
                        lvl++;

                        i += 1;
                        continue;
                    }

                    if (outpFileString[i].Contains("IF") || outpFileString[i].Contains("ELIF") || outpFileString[i].Contains("ELSE"))
                    {
                        isProcessedConditionBlock = true;

                        if (outpFileString[i].Contains("ELSE"))
                        {
                            // Тут без параметров блок
                            printLvl("else");
                        }
                        else
                        {
                            if (outpFileString[i].Contains("ELIF"))
                            {
                                printLvlln("else if (");
                            }
                            else if (outpFileString[i].Contains("IF"))
                            {
                                printLvlln("if (");
                            }
                        }
                    }

                    if ((outpFileString[i].Contains("ARGUMENT")) && (outpFileString[i].Contains("NO_ARGUMENTS")))
                    {
                        printLvl(")");
                        isBracet = false;
                        //printLvl("{");
                        //lvl++;
                        countStrOfCodeGen++;
                        continue;
                    }

                    if(outpFileString[i].Contains("CALL_FUNCTION"))
                    {
                        i++;
                        string func_name = outpFileString[i].Split(':')[1].Trim(); // Получение значения справа от :
                        printLvlln(func_name + "(");

                        i+=2;

                        if (!(outpFileString[i].Contains("NO_ARGUMENTS")))
                        {                           
                            do
                            {
                                string arg_name = outpFileString[i].Split(':')[1].Trim(); // Получение значения справа от :
                                println(arg_name); // + ", ");
                            } while (false); //(!(outpFileString[i].Contains("END_ARGUMENTS")))

                            i++;
                        }

                        print(");");
                        //print("Рассматриваем строку: " + outpFileString[i]);
                        i--;
                        continue;
                    }

                    if (outpFileString[i].Contains("INITIALIZATION_VAR"))
                    {
                        i++;
                        int indexOfColon = outpFileString[i].IndexOf(':');
                        string name = outpFileString[i].Substring(indexOfColon + 1).Trim();

                        bool isPrint = true;

                        KeyValuePair<string, bool> pair = allVarsDescr1.Find(p => p.Key == name);
                        if (pair.Value == false)
                        {
                            // Если у эта переменная нигде не используется, после инициализации
                            DeletedVar.Add(name);
                            if(isDeleteUnuseVar) isPrint = false; // Просто блокирую вывод, если эту переменную я убираю
                            else countStrOfCodeGen++;
                        }
                        else
                        {
                            countStrOfCodeGen++;
                        }

                        i++;
                        if (outpFileString[i].Contains("TYPE"))
                        {
                            int indexOfColon2 = outpFileString[i].IndexOf(':');
                            string type = outpFileString[i].Substring(indexOfColon2 + 1).Trim();

                            /*
                                if (recType == "int") currType = "INT";
                                else if (recType == "long long int") currType = "LLINT";
                                else if (recType == "float") currType = "FLOAT";
                                else if (recType == "double") currType = "DOUBLE";
                                else if (recType == "char") currType = "CHAR";
                            */

                            if (isPrint)
                            {
                                if (type == "INT")
                                {
                                    printLvlln("int ");
                                }
                                else if (type == "FLOAT")
                                {
                                    printLvlln("float ");
                                }
                                else if (type == "LLINT")
                                {
                                    printLvlln("long long int ");
                                }
                                else if (type == "DOUBLE")
                                {
                                    printLvlln("double ");
                                }
                                else if (type == "CHAR")
                                {
                                    printLvlln("char ");
                                }
                                else if (type == "BOOL")
                                {
                                    printLvlln("bool ");
                                }
                                else if (type == "STRING")
                                {
                                    printLvlln("std::string ");
                                }
                            }

                            i++;
                            if ((outpFileString[i] == null) || (!(outpFileString[i].Contains("VALUE"))))
                            {
                                if (isPrint) print(name + ";");
                            }
                            else
                            {
                                int indexOfColon3 = outpFileString[i].IndexOf(':');
                                string value = outpFileString[i].Substring(indexOfColon3 + 1).Trim();

                                if (isPrint) print(name + " = " + value + ";");
                            }
                        }                    
                        else
                        {
                            //print(" ---> У переменной " + name + " нет типа");
                            print("");
                            print("! Обнаружена ошибка: При инициализации, переменной " + name + " не присвоено начальное значение, и не указан её тип.");
                            print("");
                            //print("-----------------------");
                            //PrintSeparator();
                            ExitCode = "Init_Type";
                            errorDescr = "Отсутствие типа у инициализируемой переменной";
                            isCodeIncludeErrors = true;
                            return;
                        }

                        i -= 2;
                        continue;
                    }


                    if (outpFileString[i].Contains("WHILE"))
                    {
                        countStrOfCodeGen++;
                        i +=2;
                        if (!(outpFileString[i].Contains("NO_ARGUMENTS")))
                        {
                            if (outpFileString[i].Contains("OPERATION"))
                            {
                                int indexOfColon = outpFileString[i].IndexOf(':');
                                string oper = outpFileString[i].Substring(indexOfColon + 1).Trim();

                                i++;
                                int indexOfColon2 = outpFileString[i].IndexOf(':');
                                string leftOp = outpFileString[i].Substring(indexOfColon2 + 1).Trim();

                                i++;
                                int indexOfColon3 = outpFileString[i].IndexOf(':');
                                string rightOp = outpFileString[i].Substring(indexOfColon3 + 1).Trim();

                                printLvl("while(" + leftOp + " " + oper + " " + rightOp + ")");
                                continue;
                            }
                        }
                    }

                    if (outpFileString[i].Contains("BODY"))
                    {
                        isProcessedConditionBlock = false;
                        printLvl("{");
                        lvl++;
                        continue;
                    }

                    if (outpFileString[i].Contains("END_FUNC"))
                    {
                        if (isProcessingFuncMain == true)
                        {
                            print("");
                            printLvl("return 0;");
                        }

                        lvl--;
                        printLvl("}");                        
                        continue;
                    }

                    if (outpFileString[i].Contains("PRINT"))
                    {
                        countStrOfCodeGen++;
                        i += 2;

                        // Получаем текущий уровень отступа
                        int currentLvl_a = outpFileString[i].Length - outpFileString[i].TrimStart(' ').Length;
                        int currentLvl_b = currentLvl_a;

                        // Инициализируем начало строки вывода
                        string result = "std::cout";

                        // Разбиваем строку на части
                        int indexOfColon = outpFileString[i].IndexOf(':');
                        string oper = outpFileString[i].Substring(indexOfColon + 1).Trim();
                        string[] parts = oper.Split(new string[] { "{}" }, StringSplitOptions.None);
                        result += " << \"" + parts[0] + "\"";

                        int partIndex = 1;
                        while ((i < outpFileString.Length) && (currentLvl_a == currentLvl_b))
                        {
                            i++;
                            if ((i >= outpFileString.Length)) break;
                            if ((outpFileString[i] == null)) break;
                            currentLvl_b = outpFileString[i].Length - outpFileString[i].TrimStart(' ').Length;
                            if ((currentLvl_a != currentLvl_b)) break;
                            indexOfColon = outpFileString[i].IndexOf(':');
                            oper = outpFileString[i].Substring(indexOfColon + 1).Trim();
                            result += " << " + oper;
                            if (partIndex < parts.Length)
                            {
                                result += " << \"" + parts[partIndex] + "\"";
                                partIndex++;
                            }
                        }
                        result += " << std::endl;";
                        //lvl -= 2;

                        // Убираем лишние кавычки и пробелы
                        result = result.Replace("\"\"", "\"").Replace("\"\"", "\"");
                        result = result.Replace(" << \" << std::endl;", " << std::endl;");

                        printLvl(result);
                        i -= 2;
                        continue;
                    }

                    bool isOnce = true;
                    int recurs_i = 0;

                    string res1 = "";
                    res1 = ProcessNode(i);
                    if (isProcessedConditionBlock == true)
                    {
                        if (res1 != "") print(res1 + ")");
                    }
                    else
                    {
                        if (res1 != "") print(res1);
                    }

                    if (recurs_i != 0)
                    {
                        // Если мы глубоко ушли в разбор бинарок в дереве парсера, 
                        // вот здесь добавляем эти уже пройденные шаги к основному счётчику
                        i += recurs_i;
                        if (isDebugPrint || isDebugPrintCodeGen) print("Добавили " + recurs_i + " строк");
                        continue;
                    }


                    // Обрабатываю операцию, либо присвоение
                    string ProcessNode(int i)
                    {
                        string result = "";

                        if (isDebugPrint || isDebugPrintCodeGen) print(" ---> Рассматриваем бинарную операцию: " + outpFileString[i]);

                        if (isOnce == false)
                        {
                            recurs_i++;
                        }

                        // Обычно, строки здесь выглядят так: "ASSIGMENT : ="
                        // Тогда мы используем эти методы:
                        //
                        // string substring = str.Split(':')[0].Trim(); // Получение значения слева от :
                        // string substring = str.Split(':')[1].Trim(); // Получение значения справа от :

                        int indexOfColon = outpFileString[i].IndexOf(':');
                        if (indexOfColon == -1)
                        {
                            return "";
                        }
                        string nodeType = outpFileString[i].Substring(0, indexOfColon).Trim();

                        if (isOnce)
                        {
                            countStrOfCodeGen++;
                            if (!(nodeType == "ASSIGMENT" || nodeType == "OPERATION"))
                            {
                                if (isDebugPrint || isDebugPrintCodeGen) print(" ---> Операция: " + outpFileString[i] + " - не бинарная!");
                                return "";
                            }

                            if (isDebugPrint || isDebugPrintCodeGen) 
                                print("Операция применяется к переменной: " + outpFileString[i+1].Trim().Substring(5));
                            string currVar = outpFileString[i + 1].Trim().Substring(5);

                            if (isDebugPrint || isDebugPrintCodeGen) print("-------- Находимся внутри строки: " + outpFileString[i]);
                            if (isDebugPrint || isDebugPrintCodeGen) print("-------- Проверяем, является ли переменная " + currVar + " константной");

                            string keyToFind = currVar;
                            int index = allVars_isConst.FindIndex(pair => pair.Key == keyToFind);

                            bool res = true;

                            if (currVar.Contains("TION")) return "";

                            // Если мы внутри условия 
                            if (isProcessedConditionBlock == true)
                            {
                                if (isDebugPrint || isDebugPrintCodeGen) print("ХХХ Мы внутри условия, проверяем переменную");

                                if (CheckSubsetOper(outpFileString[i].Split(':')[1].Trim()) == true)
                                {
                                    print("Ошибка: Операция присвоения, внутри условия. Здесь должна быть операция условия");
                                    print("Возможно вы перепутали = и ==");
                                    if (index != -1) res = allVars_isConst[index].Value; // Является ли эта переменная коностантой?
                                    res = false;
                                    ExitCode = "Inv_Type_Op_Ins_Cond";
                                    errorDescr = "Неверный тип операции внутри условия";
                                    isCodeIncludeErrors = true;
                                    isConstErr = true;
                                    return "";
                                }
                                else
                                {
                                    res = false;
                                }
                            }
                            else // Если мы в теле блока
                            {
                                if (isDebugPrint || isDebugPrintCodeGen) print("UUU Мы в теле блока, проверяем переменную");

                                if (index != -1) res = allVars_isConst[index].Value; // Является ли эта переменная коностантой?
                            }

                            if (isDebugPrint || isDebugPrintCodeGen) print("Операция: " + outpFileString[i].Split(':')[1].Trim());

                            if (isDebugPrint || isDebugPrintCodeGen) 
                                print("::: Переменная " + currVar + " является константой? " + res);

                            if (res == true)
                            {
                                print("");
                                print("! Обнаружена ошибка: Переменная " + currVar + " была инициализирована как константная,");
                                print("но далее в коде ей присваивается новое значение");
                                print("");
                                //print("-----------------------");
                                //PrintSeparator();
                                ExitCode = "Const_Var_Cange";
                                errorDescr = "Изменение значения константной переменной";
                                isCodeIncludeErrors = true;
                                isConstErr = true;
                                return "";
                            }

                            //print(" ---> lvl = " + currLvl);

                            //if (currLvl > 0) // Зачем-то было здесь, удалил это
                            //{
                            //    result = "";
                            //    return;
                            //}
                        }

                        //lastLvl = currLvl;

                        isOnce = false;

                        // Она рекурсивно проходит по каждому узлу, если он будет обнаружен
                        if (nodeType == "ASSIGMENT" || nodeType == "OPERATION")
                        {
                            string oper = outpFileString[i].Substring(indexOfColon + 1).Trim();

                            i++;
                            
                            string leftOp = ProcessNode(i);

                            i++;
                            string rightOp = ProcessNode(i);

                            result = leftOp + " " + oper + " " + rightOp;
                            if (nodeType == "ASSIGMENT")
                            {
                                printLvl(result + ";");
                                result = "";
                            }
                        }
                        else // ID or NUM_INT
                        {
                            result = outpFileString[i].Substring(indexOfColon + 1).Trim();
                        }



                        return result;
                    }                    

                    //ProcessNode(0); // Нужна ли она здесь?

                    int currentLvl = outpFileString[i].Length - outpFileString[i].TrimStart(' ').Length;
                    //print("currentLvl = " + currentLvl);

                    if ((currentLvl - 2) == bufCurrLvl)
                    {
                        if (isBracet == false)
                        {
                            //lvl--;
                            //printLvl("}1");
                            
                        }
                    }

                    bufCurrLvl = currentLvl;
                }

                //
                // --- Конец цикла for
                //

                // Если операция изменяет переменную
                bool CheckSubsetOper(string str)
                {
                    if (
                        str == "=" ||
                        str == "+" ||
                        str == "-" ||
                        str == "*" ||
                        str == "/" ||
                        str == "%"
                       )
                    {
                        return true;
                    }
                    else return false;
                }

                if (countStrOfCodeGen == 0)
                {
                    printLvl("// [Программа пуста]");
                }

                if ((isMainInclude == false) && (isStrictAndCompleteOutput == true))
                {
                    if (isMainFuncUnicIntoOriginalCode == false)
                    {
                        print("");
                        printLvl("return 0;");
                        print("}");
                    }
                }
            }
        }







        /*/////////////////////////////////////////////////////////////////////////////////////////
        //                                                                                       //
        //                                                                                       //
        //                                     MAIN PARSER                                       //
        //                                                                                       //
        //                                                                                       //
        //////////////////////////////////////////////////////////////////////////////////////// */

        // Описание: 
        /*  
            
            Парсер получает на вход массив(лист) outpTable. Он выводится в консоли, после слов "Все распознанные лексемы:"
            Этот массив состоит из строк. Важно помнить, что нечётная ячейка - это лексема, а следующая за ней чётная - это её описание (большими буквами)
            Такой подход позволил хранить свсе лексемы и их описания в одном массиве, а не в 2х.

            В основной процедуре, в цикле for я иду по всем элементам этого массива (с шагом 2), и ищу нужные мне описания.
            Я отдельно выделил:
                • Блоковые
                • Строковые
                • Линейные
                • Операторы иницаилизации
                
                • А все остальные конструкции - я отправляю в разбор бинарных операций. Там строится простое дерево, ищется оператор, и если
                операторов (+ или - или = или другие) несколько - то процедура разбора бинарок вызывается рекурсивно, пока
                не обработает и не выведет в консоль все операторы

                • Также там в конце я задаю отступы (это переменная lvl), а собственно, печать с отступом, это процедура printLvl
                Если я вывожу элемент дерева разбора парсера, с помощью этой процедуры - он автоматически записывается, и в последствии
                передстся в Кодогенератор.

            Важно понимать, что когда я перехожу в разбор структуры, из основного цикла for (если найду блоковый, строковый, и др. оператор),
            то я забираю i - с собой, в эту процедуру. В этой процедуре я дальше двигаю значение i, и ижу по массиву outpTable, что-то выводя в консоль.
            И, когда я закончу разбор конструкции, я возвращаю моё текущее значение i в главный цикл for (новое значение, а не то, что было), и
            продолжаю разбора, оператором continue.

            Также, например, когда я обрабатываю блоковый оператор, я инициализирую (вывожу в консоль в дерево парсера) название конструкции
            и её аргументы, затем ключевое слово BODY, и дальше - я передаю управление обратно в основной цикл for.
            Это помогло мне сделать программу модульной. В принципе, наверное, так и было задумано.

            Также там есть обработка ошибок - если я встречаю ошибку, то я задаю для этой ошибки код (символьный), краткое описание, и кидаю 
            throw new Exception() - это останавливает разбор парсера, и останавливает программу.

            В принципе, из важной инфы - это всё. Построение дерева разбора парсера - самая большая часть моей программы, но не самая сложная.
        
        */

        public static bool isMainInclude = false; // Был ли во входном коде метод main()

        // Если переменная инициализируется, она добавляется в этот лист:
        public static List<KeyValuePair<string, bool>> allVarsDescr1 = new List<KeyValuePair<string, bool>>();
        
        // Описание, константа ли переменная
        public static List<KeyValuePair<string, bool>> allVars_isConst = new List<KeyValuePair<string, bool>>();

        public class MainParser
        {
            public int maxCountOrRecurs = 100; // Защита от бесконечной рекурсии, при построении или выводе дерева

            private int lvl; // Количество отступов перед текущей строкой, для корректного и красивого вывода структуры дерева

            #region printLvl
            // Возвращает нужное количество пробелов перед выводом строки
            public string SpaseCr(int n)
            {
                string str = "";
                for (int ii = 0; ii < n; ii++) { str += "  "; }
                println(str);
                return str;
            }

            // Комбинированный метод: Сначала выводит пробелы, затем строку
            public void printLvl(string str)
            {
                if (lvl < 0) lvl = 0;
                //print("lvl = " + lvl);
                string str2 = SpaseCr(lvl); print(str);
                outpFileString[indOnOutpMass] = str2 + str;
                indOnOutpMass++;
            }

            public void printLvlln(string str)
            {
                SpaseCr(lvl); println(str);
            }
            #endregion

            public class TreeNode
            {
                // Определяю класс вершины бинарного дерева, для определения операций

                public int ind = -1;
                public string[] data = new string[maxCountOfRowsInInputFile]; // Описание операции (заглавными буквами, из Лексера)
                public TreeNode left;
                public TreeNode right;

                public TreeNode()
                {
                    left = null;
                    right = null;
                }

                public TreeNode(int index, string data_x)
                {
                    ind = index;
                    data[0] = data_x;

                    left = null;
                    right = null;
                }
            }

            public string[] outpFileString = new string[maxCountOfRowsInInputFile];
            int indOnOutpMass = 0;            

            List<string> outpTable; // Я получаю на вход этот массив
            // Это массив всех разобранных лексем, вида ["Лексема" : "Её обозначение"]

            public string errorMassege = ""; // Сообщение о типе обнаруженной ошибки

            // Были ли найдены ошибки, в результате семантического аназиза входного кода?
            public bool semanticAnalExeption = false;


            /*//////////////////////////////////////////////////////////
            //                                                        //
            //  Основная прцедура построения дерева разбора парсера   //
            //                                                        //
            ///////////////////////////////////////////////////////// */


            public void SimpleOutput_of_all_Lexems(List<string> outpTableInp)
            {
                try
                {
                    // Изначально запускем эту процедуру
                    insideVoidParserTreeBuilding(outpTableInp);
                }
                catch (Exception ex)
                {
                    // Но если появились ошибки:

                    //print("inside_i = " + inside_i);

                    string ex1 = "\nСемантический анализатор:\n\n";
                    string ex2 = "————————————————————————————————————\n";
                    string ex3 = "В процессе построения дерева разбора \nпрограммы произошла ошибка!\n\n";
                    string ex4 = "Подробности ошибки:\n";
                    string ex5 = "";
                    string ex6 = "\n";

                    string sx = "";

                    for (int i = inside_i - 8; i < inside_i + 8; i += 2)
                    {
                        if (i > 0 && i < outpTable.Count)
                        {
                            if (i == inside_i)
                            {
                                string str = "";
                                for (int j = 0; j < ex6.Length; j++) { str += " "; }
                                str += "▼";
                                ex5 = str;
                            }

                            ex6 += outpTable[i] + " ";

                            //print("i = " + i + ", outpTable[i] = " + outpTable[i]);

                            if ((outpTable[i] == "while") || (outpTable[i] == "if") || (outpTable[i] == "else") 
                                || (outpTable[i] == "elif") || (outpTable[i] == "function"))
                            {
                                sx = "Возможно, вы не обернули аргумент в скобки";
                            }
                        }
                    }

                    if (sx != "")
                    {
                        if (errorMassege == "") errorMassege = "Error: Синтаксическая ошибка\n" + sx;
                    }
                    else
                    {
                        if (errorMassege == "") errorMassege = "Error: Синтаксическая ошибка."; // По умолчанию выводится такая ошибка
                    }

                    print(ex1 + ex2 + ex3 + ex4 + ex5 + ex6);
                    print("");
                    if (errorMassege != "") print(errorMassege);     // Если я указывал ранее в коде тип обнаруженной ошибки, то вывожу его
                    semanticAnalExeption = true;

                    ExitCode = "Pars_Err";
                    errorDescr = errorMassege;
                }
            }

            public int inside_i;
            public int indStartNewStr = 0;

            // Основная процедура построения дерева разбора парсера
            public void insideVoidParserTreeBuilding(List<string> outpTableInp)
            {
                outpTable = outpTableInp;

                if (isDebugPrint) print("-----------------------------------------------");

                print(" ");
                print("Дерево разбора парсера: ");
                print(" ");

                /*//////////////////////////////////////////////////////////
                //                                                        //
                //               Основной цикл for Парсера                //
                //                                                        //
                ///////////////////////////////////////////////////////// */

                // Тут я иду по лексемам, и передаю управление, в нужные процедуры
                for (int i = 0; i < outpTable.Count; i += 2)
                {
                    inside_i = i;
                    string lett;
                    string f = outpTable[i + 1];

                    if (isDebugPrint) print("В данный момент обрабатываем лексему: " + outpTable[i] + " : " + outpTable[i + 1] + ", i = " + i / 2);
                    if (isDebugPrint) print("f = " + f);

                    if (f == "ID")
                    {
                        // Если мы дошли до имени объявленной в коде функции
                        if (ListFunctionName.Contains(outpTable[i]))
                        {
                            // Идём в разбор функций
                            i = FunctionProcessing(i);
                            continue;
                        }
                    }

                    // Вывожу значения операторов, только если они - числа, строки или названия
                    if (f == "ID" || f == "NUM_INT" || f == "STRING" || f == "NUM_FLOAT")
                    {
                        lett = (outpTable[i + 1] + " : " + outpTable[i]);
                    }
                    else
                    {
                        // Во всех остальных случаях, просто вывожу название этой лексемы
                        lett = (outpTable[i + 1]);
                    }

                    // Если это блоковый оператор
                    string[] massBlockConstr = new string[] { "FUNCTION", "IF", "WHILE", "ELSE", "ELIF", "FOR"};       

                    if (massBlockConstr.Contains(f)) 
                    {
                        // Передаём управление в функцию обработки блоковых операторов
                        i = BlockConstrProcessing(i);
                        i -= 2; // Переходим на предыдущую лексему, т.к. дальше мы продолжим цикл for, и он сам сделает i += 2
                        continue; 
                    }

                    // Если строковый оператор
                    string[] massStrConstr = new string[] { "PRINT" };                    

                    if (massStrConstr.Contains(f)) 
                    {
                        i = StringConstrProcessing(i);
                        i -= 2;
                        continue;
                    }

                    // Если это оператор инициализации
                    string[] massInitConstr = new string[] { "LET" };

                    if (massInitConstr.Contains(f)) 
                    {
                        i = initiVarProcessing(i);                        

                        // Фикс, если initiVarProcessing возвращает некорректный индекс конца строки
                        if (outpTable[(i + 1) - 4] == "END_LINE")
                        {
                            i -= 4;
                            if (isDebugPrint) print("outpTable[(" + i + " + 1) - 4] == END_LINE");
                            if (isDebugPrint) print("Фикс конца строки в initiVarProcessing (- 2 ключевых слова назад)");
                        }
                        else if (outpTable[(i + 1) - 2] == "END_LINE")
                        {
                            i -= 2;
                            if (isDebugPrint) print("outpTable[(" + i + " + 1) - 2] == END_LINE");
                            if (isDebugPrint) print("Фикс конца строки в initiVarProcessing (- 1 ключевое слово назад)");
                        }

                        i -= 2;

                        continue;
                    }

                    if (f == "END_VOID")                 // }
                    {
                        if (isDebugPrint) print("f = " + f);
                        if ((i + 2) >= outpTable.Count())
                        {
                            lvl -= 2;
                            printLvl("END_FUNC");
                            break; // Если это последняя закрывающая кавычка в программе, то завершаем построение дерева разбора
                        }
                        else 
                        {
                            if (isDebugPrint) print("i + 2 = " + (i + 2) + ", outpTable.Count() = " + outpTable.Count());
                        }
                    }

                    // Если это не блоковый, не строковый и не оператор объявления, то мы
                    // загоняем всю строку в разбор бинарных операций

                    if (f != "END_LINE")
                    {
                        int w_i = indStartNewStr;
                        if (isDebugPrint) print("indStartNewStr = " + indStartNewStr);

                        while (w_i < outpTable.Count)
                        {
                            w_i += 2;
                            //int indStart = indStartNewStr;
                            if (isDebugPrint) print("Ищем конец строки, w_i = " + w_i/2);

                            if (outpTable[w_i + 1] == "END_LINE")
                            {
                                // Если цикл обнаружил конец строки, то мы загоняем всю строку в разбор бинарок
                                BinarOperation(indStartNewStr, w_i);

                                if (isLastBinarOper_isReturnNullResult == false)
                                {
                                    // Если разбор бинарных операций нашёл операции в этой строке

                                    i = i_forBinarOperReturn + 4; // +2, т.к. он останавливается на последней лексеме
                                                                  // и ещё +2, до символа конца строки

                                    //print("i = " + i/2);
                                    f = outpTable[i + 1];
                                }

                                break;
                            }
                        }
                    }

                    //print("outpTable[i] = " + outpTable[i]);
                    //print("outpTable[i - 1] = " + outpTable[i - 1]);

                    // Разбор доп. символов:

                    if (f == "END_VOID")                 // }
                    {
                        lvl--;
                        printLvl("END_FUNC");
                    }

                    if (f == "OPEN_BRACKET") lvl++;      // (
                    if (f == "CLOSED_BRACKET")           // )
                    {
                        lvl--;    
                        //if (printArgumentProcessed == true)
                        //{
                        //    printArgumentProcessed = false;
                        //    lvl--;
                        //}
                    }

                    if (f == "END_LINE")                 // ;
                    {
                        lvl--;          
                        indStartNewStr = i + 2;
                    }

                    if (f != "END_LINE" && f != "END_VOID" && f != "OPEN_BRACKET" && f != "CLOSED_BRACKET")
                    {
                        // В любом друго случае - просто печатаем текущую лексему
                        printLvl(lett);
                    }                    
                }

                if (semanticAnalExeption == false)
                {
                    print(" ");
                    PrintSeparator();
                    print("Семантический анализатор:");
                    print(" ");
                    print("Проблем не обнаружено.");
                }
            }

            // 
            // --- Отдельно выведенные процедуры Парсера: 
            //

            bool isLastBinarOper_isReturnNullResult = false;

            public int FunctionProcessing(int i)
            {
                printLvl("CALL_FUNCTION");
                lvl++;
                printLvl("NAME : " + outpTable[i]);
                printLvl("ARGUMENT");
                lvl++;
                i += 4;
                if (outpTable[i + 1] == "CLOSED_BRACKET")
                {
                    printLvl("NO_ARGUMENTS");
                }
                else
                {
                    i = DecodeVarDescr(i);
                }

                lvl--;

                return i;
            }

            // Отдельно вынес код, в котором создаётся дерево разбора бинарных операций, и затем печатается
            public int BinarOperation(int oldI, int i)
            {
                // oldI = index start
                // i = index finish (включительно)

                TreeNode root = mainBinarOperationProcessing(oldI, i);
                int subLvl = 0;

                if (root != null)
                {
                    // Обходим и выводим дерево как КЛП, где каждый К - с новым lvl++

                    int oldLvl = lvl;
                    subLvl = PrintPreorderTraversal(root, oldLvl);
                    //lvl = oldLvl;
                    lvl = subLvl;
                    isLastBinarOper_isReturnNullResult = false;
                }
                else
                {
                    // Когда не смогли найти ни одной бинарной операции в заданном диапазоне лексем

                    //print("Дерево = null");
                    isLastBinarOper_isReturnNullResult = true;
                }

                lvl--;

                if (i_forBinarOperReturn != 0) i = i_forBinarOperReturn;
                else i += 2; // ! Ошибка - Если идут 2 подряд разбора дерева переменных, он пролетает на 1 символ вперёд

                return i;
            }

            // Печать всего дерева
            public int PrintPreorderTraversal(TreeNode node, int subLvl)
            {
                //node = node.left;
                if (node == null)
                {
                    return subLvl;
                }

                coll++;

                for (int i = 0; i < node.data.Length; i++)
                {
                    if (node.data[i] != null)
                    {
                        printLvl(node.data[i]);
                    }
                }
                //print("");

                if (node.ind != -1) lvl++;
                else subLvl++;

                if (subLvl >= 2)
                {
                    subLvl = 0;
                    lvl--;
                }

                subLvl = PrintPreorderTraversal(node.left, subLvl);
                subLvl = PrintPreorderTraversal(node.right, subLvl);

                return subLvl;
            }

            int coll = 0;

            // --- Дальше идут процедуры, в которые я передаю управление, при построении дерева разбора Парсера

            // Массивы, по которым мы проверяем принадлежность лексемы:
            //public string[] massBlockConstr = new string[] { "FUNCTION", "IF", "WHILE" };       // К "Блоковым" структурам
            //public string[] massStrConstr = new string[] { "PRINT" };                           // К "Строковым" структурам
            //public string[] massInitConstr = new string[] { "LET" };                            // К структурам "Инициализации"

            // Отдельная обработка оператора FOR
            public int ForProcessedOperations(int i)
            {
                printLvl("FOR");
                
                i += 2;
                lvl++;

                printLvl("CONDITION");
                lvl++;

                printLvl("VAR : " + outpTable[i]);

                i += 2;
                if (!(outpTable[i] == "in"))
                {
                    print("Ошибка, на этом месте должно было стоять ключевое слово in");
                    throw new Exception();
                }

                i += 2;
                printLvl("FROM : " + outpTable[i]);

                i += 2;
                if (!(outpTable[i + 1] == "DOT"))
                {
                    print("Ошибка, на этом месте должне была стоять точка");
                    throw new Exception(); 
                }
                i += 2;
                if (!(outpTable[i + 1] == "DOT"))
                {
                    print("Ошибка, на этом месте должне была стоять точка");
                    throw new Exception();
                }
                i += 2;

                printLvl("TO : " + outpTable[i]);

                lvl--;
                printLvl("BODY");
                lvl++;

                i += 2;

                return i;
            }

            // Обработка блоковых операторов
            public int BlockConstrProcessing(int i)
            {
                if(isDebugPrint) print("Pars > Обрабатываем блоковый оператор");

                if (outpTable[i + 1] == "FOR")
                {
                    if (isDebugPrint) print("Обрабатываем цикл for");
                    return(ForProcessedOperations(i));
                }

                int new_i = i;
                inside_i = i;

                //lvl++;

                printLvl(outpTable[new_i + 1]);

                new_i += 2;

                lvl++;

                // Следующее ID - это имя, но только если это функция

                if (outpTable[new_i - 1] == "FUNCTION")
                {
                    new_i += 2;
                    if (outpTable[new_i - 1] == "MAIN")
                    {
                        printLvl("NAME : " + outpTable[new_i - 1]);
                    }
                    else
                    {
                        printLvl("NAME : " + outpTable[new_i - 2]);
                    }
                    printLvl("ARGUMENT");

                    // Если у функции всё-же есть аргументы:
                    if (!(outpTable[new_i + 2 + 1] == "CLOSED_BRACKET"))
                    {
                        new_i += 2;
                        lvl++;

                        new_i = DecodeVarDescr(new_i);

                        lvl--;

                        // Переходим чуть ниже - пропускаем инициализацию аргументов, и переходим сразу к телу блока
                        goto argument_is_valid;
                    }
                }
                else
                {
                    printLvl("CONDITION");
                }            

                // Внутри () - это аргумент

                int subLvl = 0;
                int stInd = new_i;

                // У else нет аргументов
                if (!(outpTable[new_i - 1] == "ELSE"))
                {
                    while (outpTable[new_i - 1] != "OPEN_BRACKET") { new_i += 2; if (new_i > maxCountOrRecurs) break; } // Идём до ()
                    inside_i = i;

                    coll = 0;
                    lvl += 1;
                    stInd = new_i;
                }

                while (outpTable[new_i + 1] != "START_VOID")
                {
                    new_i += 2;
                    if (new_i > maxCountOrRecurs) break;
                }
                inside_i = i;

                int finInd = new_i;

                TreeNode root = mainBinarOperationProcessing(stInd, finInd);

                if (root != null)
                {
                    // Обходим и выводим дерево как КЛП, где каждый К - с новым lvl++

                    int oldLvl = lvl;
                    subLvl = PrintPreorderTraversal(root, subLvl);
                    lvl = oldLvl;
                }

                if (coll == 0) 
                {
                    printLvl("NO_ARGUMENTS");
                }

                lvl -= 1;

                // Внутри {} - это тело

                argument_is_valid:

                while (outpTable[new_i - 1] != "START_VOID") { new_i += 2; if (new_i > maxCountOrRecurs) break; } // Идём до {}

                printLvl("BODY");
                lvl += 1;

                return new_i;
            }

            public bool printArgumentProcessed = false; // Идёт ли сейчас обработка аргумента у функции print?

            // Обработка строковых операторов
            public int StringConstrProcessing(int ui)
            {
                if (isDebugPrint) print("Pars > Обрабатываем строковый оператор");

                inside_i = ui;
                int i = ui;

                printLvl(outpTable[i + 1]);

                if (outpTable[i + 1] == "PRINT")
                {
                    printArgumentProcessed = true;
                    i += 2;
                    lvl++;
                    printLvl("ARGUMENTS");
                    lvl++;
                    //lvl++;
                    while (outpTable[i + 1] != "END_LINE") {
                        if ((outpTable[i + 1] != "END_VOID")
                        && (outpTable[i + 1] != "OPEN_BRACKET")
                        && (outpTable[i + 1] != "CLOSED_BRACKET")
                        && (outpTable[i + 1] != "END_LINE")
                        && (outpTable[i + 1] != "COMMA"))
                        {
                            printLvl(outpTable[i + 1] + " : " + outpTable[i]);
                            inside_i = i;
                        }
                        
                        i += 2; 
                        if (i > maxCountOrRecurs) break; 
                    }
                    lvl--;
                    return i;
                }

                lvl++;
                i += 2;

                while (outpTable[i - 1] != "OPEN_BRACKET") { i += 2; if (i > maxCountOrRecurs) break; } // Идём до (

                return i;
            }

            // Проверяем, является ли входная строка логической или арифметической операцией
            bool CheckEqOper(string str)
            {
                if (
                    str == "+" ||
                    str == "-" ||
                    str == "*" ||
                    str == "/" ||
                    str == "%" ||
                    str == "==" ||
                    str == "!=" ||
                    str == "<" ||
                    str == ">" ||
                    str == "<=" ||
                    str == ">=" ||
                    str == "&&" ||
                    str == "||" ||
                    str == "!")
                {
                    return true;
                }
                else return false;
            }

            public void CheckInitVar(int ins_i) // Проверяет, была ли ранее инициализирована используемая переменная
            {
                //ins_i += 2;
                string str = outpTable[ins_i];

                if (!(outpTable[ins_i + 1] == "ID"))
                {
                    //print(str + " - не переменная");
                    return; // Если это не id переменной, то это мы не рассматриваем
                }

                bool res = allVarsDescr1.Any(pair => pair.Key.Contains(str));
                //print("Прверяю, была ли инициализирована переменная: " + str);

                if (res == false)
                {
                    print("! В результате разбора исходной программы возникла ошибка!");
                    print("Переменная '" + str + "' не была объявлена.");
                    inside_i = ins_i;

                    errorMassege = "Семантическая ошибка: " + "Переменная '" + str + "' не была объявлена.";

                    //Exception();
                    throw new Exception();
                }
            }

            public int i_forBinarOperReturn = 0; // Позиция i, которую может вернуть разбор линарных операций

            // Рекурсивная обработка бинарных операторов (у аргументов функций)
            public TreeNode mainBinarOperationProcessing(int indStart, int indFinish) // Индексы включительно
            {
                if (isDebugPrint) print("Pars > Обрабатываем бинарную операцию");                

                inside_i = indStart;
                TreeNode root = null;

                indFinish -= 4;
                //printLvl("--- Все лексемы у аргумента:");

                //print("Запускаю дерево разбора, начало - " + indStart/2 + ", конец - " + indFinish/2);

                int lvlPol = 0;

                if (isDebugPrint) print("Создаём новую бинарную операцию, индекс начала = " + indStart + ", индекс конца = " + indFinish);
                if (isDebugPrint) print("символ начала: " + outpTable[indStart] + " , символ конца: " + outpTable[indFinish]);

                //SpaseCr(lvl);
                for (int i = indStart; i <= indFinish; i+=2)
                {
                    if (i >= outpTable.Count()) // Если дальше этой бинарной операции конец файла
                    {
                        return(root);
                    }

                    if (outpTable[i + 1] == "END_LINE")
                    {
                        i_forBinarOperReturn = i;
                        return (root);
                    }

                    //println(outpTable[i] + " ");

                    if (outpTable[i] == "(") lvlPol++;

                    if (outpTable[i] == ")") lvlPol--;

                    int end_i_l = 0;
                    int end_i_r = 0;
                    int res_end_i = 0;

                    if ((CheckEqOper(outpTable[i]) == true) && (outpTable[i + 2] == "="))
                    {
                        root = new TreeNode(i, "ASSIGMENT : " + outpTable[i] + outpTable[i + 2]);
                        CheckInitVar(i);

                        end_i_l = recurseBinOp(indStart, i - 2, root, 0);
                        end_i_r = recurseBinOp(i + 4, indFinish, root, 1);
                    }
                    else if ((outpTable[i] == "=") && (CheckEqOper(outpTable[i - 2]) == false))
                    {
                        root = new TreeNode(i, "ASSIGMENT : " + outpTable[i]);
                        CheckInitVar(i);

                        end_i_l = recurseBinOp(indStart, i - 2, root, 0);
                        end_i_r = recurseBinOp(i + 2, indFinish, root, 1);

                    }
                    else if (CheckEqOper(outpTable[i]) == true)
                    {
                        if (lvlPol == 0)
                        {
                            root = new TreeNode(i, "OPERATION : " + outpTable[i]);
                            CheckInitVar(i);

                            end_i_l = recurseBinOp(indStart, i - 2, root, 0);
                            end_i_r = recurseBinOp(i + 2, indFinish, root, 1);
                        }
                    }
                    else
                    {
                        if (isDebugPrint) print("Операнд " + outpTable[i] + " не операция.");
                    }

                    if (end_i_l > end_i_r) res_end_i = end_i_l;
                    else res_end_i = end_i_r;

                    if (res_end_i / 2 > 0)
                    {
                        //print("Закончили разбор бинарного поддерева на этом индексе: " + res_end_i / 2);

                        i = res_end_i;
                    }
                }
                //print("");

                i_forBinarOperReturn = indFinish;
                return (root);
            }

            public int recurseBinOp(int indStart, int indFinish, TreeNode inp_root, int sideMode) 
            {
                // sideMode = 0 - левая сторона
                // sideMode = 1 - правая сторона

                //if ((indFinish - indStart) <= 0) return;

                TreeNode new_node = new TreeNode();

                if (sideMode == 0)
                {
                    inp_root.left = new_node;
                }
                else
                {
                    inp_root.right = new_node;
                }

                if (indStart > indFinish)
                {
                    indFinish = indStart;
                }

                int lvlPol = 0;
                bool isOperationNode = false;
                int ind = 0;

                for (int i = indStart; i <= indFinish; i += 2)
                {
                    if (i >= outpTable.Count()) // Если дальше этой бинарной операции конец файла
                    {
                        //return;
                        break;
                    }

                    if (outpTable[i + 1] == "END_LINE") // Если в листе дерева мы встретили конец строки, то прекращаем разбор этого листа
                    {
                        break;
                    }

                    //println(outpTable[i] + " ");
                    if (isDebugPrint) print("Рассматриваем операнд как бинарную операцию: " + outpTable[i]);

                    //if (outpTable[i] == "(") lvlPol++;
                    //if (outpTable[i] == ")") lvlPol--;

                    if ((CheckEqOper(outpTable[i]) == true) && (outpTable[i + 2] == "="))
                    {
                        new_node.data[0] = "ASSIGMENT : " + outpTable[i] + outpTable[i + 2];
                        CheckInitVar(i);
                        new_node.ind = i;

                        recurseBinOp(indStart, i - 2, new_node, 0);
                        recurseBinOp(i + 4, indFinish, new_node, 1);

                        isOperationNode = true;
                    }
                    else if ((outpTable[i] == "=") && (CheckEqOper(outpTable[i - 2]) == false))
                    {
                        new_node.data[0] = "ASSIGMENT : " + outpTable[i];
                        CheckInitVar(i);
                        new_node.ind = i;

                        recurseBinOp(indStart, i - 2, new_node, 0);
                        recurseBinOp(i + 2, indFinish, new_node, 1);

                        isOperationNode = true;
                    }
                    else if (CheckEqOper(outpTable[i]) == true)
                    {
                        if (lvlPol == 0)
                        {
                            new_node.data[0] = "OPERATION : " + outpTable[i];
                            CheckInitVar(i);
                            new_node.ind = i;

                            recurseBinOp(indStart, i - 2, new_node, 0);
                            recurseBinOp(i + 2, indFinish, new_node, 1);
                        }
                        isOperationNode = true;
                    }
                }

                //print("indStart = " + indStart + " indFinish = " + indFinish);

                for (int i = indStart; i <= indFinish; i += 2)
                {
                    if (i >= outpTable.Count()) // Если дальше этой бинарной операции конец файла
                    {
                        //return;
                        break;
                    }

                    if (outpTable[i + 1] == "END_LINE") // Если в листе дерева мы встретили конец строки, то прекращаем разбор этого листа
                    {
                        break;
                    }

                    if (!isOperationNode)
                    {
                        if (outpTable[i] != "(" && outpTable[i] != ")")
                        {
                            CheckInitVar(i);
                            SetUsegVar(outpTable[i]);
                            new_node.data[ind] = outpTable[i + 1] + " : " + outpTable[i];
                        }

                        //print("Добавляем в лист операнд: " + outpTable[i+1]);

                        ind++;
                    }
                }

                return indFinish;
            }

            public static void SetUsegVar(string nameVar)
            {
                string keyToFind = nameVar;
                int index = allVarsDescr1.FindIndex(pair => pair.Key == keyToFind);
                if (index != -1)
                {
                    allVarsDescr1.RemoveAt(index);
                    allVarsDescr1.Insert(index, new KeyValuePair<string, bool>(keyToFind, true));
                }

                // Изменяю для этой переменно поле, что она была использована с false на true
            }

            Dictionary<string, string> dickAllTypesFromInitVars = new Dictionary<string, string>
            {
                // Все типы данных, которые могут быть инициализированы, и соответствующие им типы данных на C++
                {"i8", "int"},
                {"u8", "int"},
                {"i16", "int"},
                {"u16", "int"},
                {"i32", "int"},
                {"u32", "int"},
                {"i64", "long long int"},
                {"u64", "long long int"},
                {"f32", "float"},
                {"f64", "double"},
                {"bool", "bool"},
                {"char", "char"},
                {"string", "string"},
                {"String", "string"},

                //// Неподдерживаемые типы:
                //{"i128", "none"},
                //{"u128", "none"},
                //{"isize", "none"},
                //{"usize", "none"}
            };

            // Неподдерживаемые типы данных:
            Dictionary<string, string> dickOfUnsupportedTypesFromInitVars = new Dictionary<string, string>
            {
                {"i128", "none"},
                {"u128", "none"},
                {"isize", "none"},
                {"usize", "none"}
            };

            public bool isRecError = false; // Если обнаружена ошибка несоответствия типов инициализации, и присвоения этой переменной

            // initialization
            public int initiVarProcessing(int inp_i)
            {
                //if (isDebugPrint) print("Pars > Обрабатываем инициализацию");                

                inside_i = inp_i;
                int i = inp_i;                

                //if(i > 2) i -= 2;

                if (isDebugPrint) print("Pars > Обрабатываем инициализацию: " + outpTable[i] + " : " + outpTable[i + 1] + ", i = " + i / 2);

                //print("Вошли в процедуру обработки LET. i = " + i + ", simv = " + outpTable[i]);

                if (outpTable[i + 1] == "LET")
                {
                    i += 2;
                    printLvl("INITIALIZATION_VAR");
                    lvl++;

                    //printLvl(+outpTable[i - 1]);

                    bool isConstVar = true;

                    if (outpTable[i + 1] == "MUT") // Игнорируем это ключевое слово
                    {
                        i += 2;
                        isConstVar = false;
                    }

                    if (outpTable[i + 1] == "ID")
                    {
                        i = DecodeVarDescr(i, isConstVar);
                    }

                    lvl--;
                }

                return i;
            }

            // Разбирает типы и начальное значение переменной при инициализации 
            // (или задании типа переменной в описании аргументов функции)
            public int DecodeVarDescr(int i, bool isConstVar=false)
            {

                printLvl("NAME : " + outpTable[i]);
                string localVarName = outpTable[i];
                //allVarsDescr.Add(outpTable[i]);

                int index = allVarsDescr1.FindIndex(pair => pair.Key == localVarName);
                if (index != -1)
                {
                    // Если такая переменная уже была инициализирована выше

                    isRecError = true;
                    PrintSeparator();
                    errorMassege = "Повторная инициплизация переменной";
                    //print(errorMassege);
                    print("");
                    print("Ошибка! Переменная " + localVarName + " была инициализирована второй раз");
                    //return;
                    throw new Exception();
                }

                allVarsDescr1.Add(new KeyValuePair<string, bool>(localVarName, false));
                allVars_isConst.Add(new KeyValuePair<string, bool>(localVarName, isConstVar));

                i += 2;
                inside_i = i;

                if (outpTable[i] == "=")
                {
                    i += 2;
                    if ((outpTable[i + 1] == "NUM_INT") || (outpTable[i + 1] == "NUM_FLOAT"))
                    {
                        if (outpTable[i + 1] == "NUM_INT")
                        {
                            printLvl("TYPE : INT");
                            printLvl("VALUE : " + outpTable[i]);
                            i += 2;
                        }
                        else if (outpTable[i + 1] == "NUM_FLOAT")
                        {
                            printLvl("TYPE : FLOAT");
                            printLvl("VALUE : " + outpTable[i]);
                            i += 2;
                        }
                    }
                    else if ((outpTable[i + 1] == "TRUE") || (outpTable[i + 1] == "FALSE"))
                    {
                        printLvl("TYPE : BOOL");
                        printLvl("VALUE : " + outpTable[i]);
                        i += 2;
                    }
                    else if (outpTable[i + 1] == "ONCE_QUOTAT")
                    {
                        printLvl("TYPE : CHAR");
                        i += 2;
                        printLvl("VALUE : '" + outpTable[i] + "'");
                        i += 4;
                    }
                    else if (outpTable[i + 1] == "STRING")
                    {
                        isStringUsage = true;
                        printLvl("TYPE : STRING");
                        printLvl("VALUE : " + outpTable[i]);
                        i += 2;
                    }
                    else // Отправляем в разбор бинарок. Это эксперементальная ветка кода (!)
                    {
                        i = BinarOperation(i - 6, i + 2);
                    }
                }
                else if (outpTable[i] == ":")
                {
                    // Если мы явно указываем тип нашей переменной

                    i += 2;
                    if (dickAllTypesFromInitVars.ContainsKey(outpTable[i]))
                    {
                        string recType = dickAllTypesFromInitVars[outpTable[i]];

                        // Если это один из поддерживаемых типов
                        //print(outpTable[i] + " - поддерживаемый тип данных. Мы объявим его как " + recType);

                        string currType = "";
                        if (recType == "int") currType = "INT";
                        else if (recType == "long long int") currType = "LLINT";
                        else if (recType == "float") currType = "FLOAT";
                        else if (recType == "double") currType = "DOUBLE";
                        else if (recType == "char") currType = "CHAR";
                        else if (recType == "bool") currType = "BOOL";
                        else if (recType == "string") currType = "STRING";

                        printLvl("TYPE : " + currType);

                        void ErrorRec()
                        {
                            isRecError = true;
                            PrintSeparator();
                            errorMassege = "Ошибка! Несоответствие типов объявляемой переменной, и присваевомого ей значения!";
                            print(errorMassege);
                            print("");
                            print("Трансляция программы остановлена с исключением: Несоответствие типов объявляемой переменной, и присваевомого ей значения");
                            //return;
                            throw new Exception();
                        }

                        i += 2;
                        if (outpTable[i] == "=")
                        {
                            i += 2;
                            //print("outpTable[i + 1] = " + outpTable[i + 1]); -------------------------------------------------------< Не закончил
                            if ((outpTable[i + 1] == "NUM_INT") || (outpTable[i + 1] == "NUM_FLOAT"))
                            {
                                if (outpTable[i + 1] == "NUM_INT")
                                {
                                    //printLvl("TYPE : INT");
                                    if ((currType == "INT") || (currType == "LLINT"))
                                    {
                                        printLvl("VALUE : " + outpTable[i]);
                                        i += 2;
                                    }
                                    else
                                    {
                                        ErrorRec();
                                    }
                                }
                                else if (outpTable[i + 1] == "NUM_FLOAT")
                                {
                                    print("currType = " + currType);
                                    if ((currType == "FLOAT") || (currType == "DOUBLE"))
                                    {
                                        printLvl("VALUE : " + outpTable[i]);
                                        i += 2;
                                    }
                                    else
                                    {
                                        ErrorRec();
                                    }
                                }
                            }
                            else if (currType == "BOOL")
                            {
                                printLvl("VALUE : " + outpTable[i]);
                                i += 2;
                            }
                            else if (currType == "CHAR")
                            {
                                i += 2;
                                printLvl("VALUE : '" + outpTable[i] + "'");
                                i += 4;
                            }
                            else if (currType == "STRING")
                            {
                                i += 2;
                                printLvl("VALUE : " + outpTable[i - 2]);
                                i += 4;
                            }
                            else // Отправляем в разбор бинарок. Это эксперементальная ветка кода (!)
                            {
                                i = BinarOperation(i - 6, i + 2);
                            }
                        }
                        else if (outpTable[i] == ")")
                        {
                            //print("### Объявлен только тип переменной");
                            return i;
                        }
                    }
                    else if (dickOfUnsupportedTypesFromInitVars.ContainsKey(outpTable[i]))
                    {
                        // Если это один из неподдерживаемых типов
                        //print(outpTable[i] + " - НЕподдерживаемый тип данных");
                        isRecError = true;
                        print("");
                        errorMassege = "Ошибка! В объявлении переменной " + localVarName + " обнаружен неподдерживаемый тип данных: " + outpTable[i];
                        print(errorMassege);
                        print("");
                        PrintSeparator();
                        print("Трансляция программы остановлена с исключением: Переменная была инициализирована неподдерживаемым типом данных");
                        //return;
                        throw new Exception();
                    }
                    else
                    {
                        // Если это синтаксическая ошибка
                        throw new Exception();
                    }
                }

                return i;
            }
        }







        /*/////////////////////////////////////////////////////////////////////////////////////////
        //                                                                                       //
        //                                                                                       //
        //                                     MAIN LEXER                                        //
        //                                                                                       //
        //                                                                                       //
        //////////////////////////////////////////////////////////////////////////////////////// */

        // Описание: 
        /*              
            ~ 
        */

        public static bool err_empty_input_file = false;

        // В этом листе будут хранится инициализированные имена функций, из всего входного кода
        public static List<string> ListFunctionName = new List<string>();

        // Лексер - распознаёт лексемы из входного файла, и одну за другой добавляет их в выходной массив
        // Будем считать, что он полностью отлажен, и работает верно.
        public class MainLexer
        {
            public string InputNameFile = "Input.txt";                      // Название входного файла
            public List<string> InputLine = new List<string>();             // Хранит все полученные строки входного файла

            public List<string> outpTable = new List<string>();
            // Массив, хранящий таблицу, которую мы выводим в конце работы программы

            /*
                Тут три словаря
                
                В первом - храняться whitespace слова. Это такие лексемы, которые должны начинаться с пробела, и заканчиваться пробелом, иначе их смысл теряется
                Например: while true
                
                Во втором - лексемы, которые могут находиться внутри строки, не огражадаемые пробелами, и это не утрачивает их смысл
                Например: a+=10^2;

                В третьем - символы, между которыми процедура подготовки расставит пробелы, для лучшего распознавания
                Например, строка из файла "let n=5;" превращается в "let n = 5 ; "
            */

            /*
                Это алгоритм работы всех процедур. Я расписывал его для себя, но наверно не буду удалять

                Идём посимвольно вперёд, пока не встретим проблел или таб
                Все символы сохраняем в буфер, и потом отправляем на опознание

                Если встретился символ:
                    " "
                    "   "
                то прекращаем ввод, и отправляем на опознание, если мы сохранили болше 0 символов

                ---- В опознании лексем:

                Если лексема начинается с цифры, то:
                    Если он состоит только из цифр, то это целое число (NUM_INT)
                    Иначе если у него встречается одна точка, то это не целое число (NUM_FLOAT)
                    Иначе - это ошибка распознавания (ERROR)
                Иначе - отправляем в словарь, на распознавание
                    Если в словаре распозналось - оставляем, то что пришло от него
                    Иначе - это название переменной или метода (ID)

                ---- В распознании по словарю:

                Если не можем распознать строку, то:
                    создаём новую пустую, и начинаем итерационно добавлять к неё символы из входной строки
                        Если мы добавили всю строку, и ничего не распознали, то выкидываем ""
                        Но, если мы хоть что-то распознали, то выкидываем это как ответ, но перед этим:
                            Запускаем рекурсивный вызов распознавания в словаре, оставшегося кусочка
            */

            // Словарь для хранения используемых whitespace лексем, и их поиска
            Dictionary<string, string> dickOfLex_01 = new Dictionary<string, string>
            {             
                // Ключевые слова, которые должны встречаться между пробелами

                {"as", "AS"},               // Используется для явного приведения типов
                {"break", "BREAK"},         // Прерывает выполнение цикла
                {"const", "CONST"},         // Определяет константу
                {"continue", "CONTINUE"},   // Переходит к следующей итерации цикла
                {"crate", "CRATE"},         // Используется для обозначения текущего крейта (пакета)
                {"else", "ELSE"},           // Определяет блок кода, который должен выполниться в случае, если условие не истинно
                {"enum", "ENUM"},           // Определяет перечисление
                {"extern", "EXTERN"},       // Указывает на то, что функция или переменная определены в другом модуле
                {"false", "FALSE"},         // Булевое значение, которое обозначает ложь
                {"fn", "FUNCTION"},         // Определяет функцию
                {"for", "FOR"},             // Начинает цикл по итератору
                {"if", "IF"},               // Определяет блок кода, который должен выполниться в случае, если условие истинно
                {"impl", "IMPL"},           // Определяет реализацию типажа (trait) для типа
                {"in", "IN"},               // Используется в циклах для обхода итераторов
                {"let", "LET"},             // Определяет переменную
                {"loop", "LOOP"},           // Начинает бесконечный цикл
                {"match", "MATCH"},         // Используется для сопоставления значения с шаблоном
                {"mod", "MOD"},             // Определяет модуль
                {"move", "MOVE"},           // Используется для перемещения владения значения
                {"mut", "MUT"},             // Позволяет изменять значение переменной
                {"pub", "PUB"},             // Делает элемент публичным
                {"ref", "REF"},             // Создает ссылку на значение
                {"return", "RETURN"},       // Возвращает значение из функции
                {"self", "SELF"},           // Ссылка на текущий тип или модуль
                {"static", "STATIC"},       // Определяет статическую переменную
                {"struct", "STRUCT"},       // Определяет структуру
                {"super", "SUPER"},         // Ссылка на родительский модуль
                {"trait", "TRAIT"},         // Определяет типаж
                {"true", "TRUE"},           // Булевое значение, которое обозначает истину
                {"type", "TYPE"},           // Определяет новый тип
                {"unsafe", "UNSAFE"},       // Используется для написания небезопасного кода
                {"use", "USE"},             // Импортирует элементы из модуля
                {"where", "WHERE"},         // Используется для ограничения типов
                {"while", "WHILE"},         // Начинает цикл, который выполняется, пока условие истинно

                {"main", "MAIN"},
                {"println!", "PRINT"},

                // Все типы данных, которые могут быть инициализированы, и соответствующие им типы данных на C++
                {"i8", "int"},
                {"u8", "int"},
                {"i16", "int"},
                {"u16", "int"},
                {"i32", "int"},
                {"u32", "int"},
                {"i64", "long long int"},
                {"u64", "long long int"},
                {"f32", "float"},
                {"f64", "double"},
                {"bool", "bool"},
                {"char", "char"},

                // Неподдерживаемые типы:
                {"i128", "none"},
                {"u128", "none"},
                {"isize", "none"},
                {"usize", "none"}
            };
            
            //// Словарь для хранения НЕ используемых whitespace лексем, и их поиска
            //Dictionary<string, string> dickOfLex_01 = new Dictionary<string, string>
            //{             
            //    // Ключевые слова, которые должны встречаться между пробелами

            //    {"as", "AS"},               // Используется для явного приведения типов
            //    {"break", "BREAK"},         // Прерывает выполнение цикла
            //    //{"const", "CONST"},         // Определяет константу
            //    {"continue", "CONTINUE"},   // Переходит к следующей итерации цикла
            //    //{"crate", "CRATE"},         // Используется для обозначения текущего крейта (пакета)
            //    {"else", "ELSE"},           // Определяет блок кода, который должен выполниться в случае, если условие не истинно
            //    //{"enum", "ENUM"},           // Определяет перечисление
            //    //{"extern", "EXTERN"},       // Указывает на то, что функция или переменная определены в другом модуле
            //    {"false", "FALSE"},         // Булевое значение, которое обозначает ложь
            //    {"fn", "FUNCTION"},         // Определяет функцию
            //    {"for", "FOR"},             // Начинает цикл по итератору
            //    {"if", "IF"},               // Определяет блок кода, который должен выполниться в случае, если условие истинно
            //    //{"impl", "IMPL"},           // Определяет реализацию типажа (trait) для типа
            //    {"in", "IN"},               // Используется в циклах для обхода итераторов
            //    {"let", "LET"},             // Определяет переменную
            //    {"loop", "LOOP"},           // Начинает бесконечный цикл
            //    //{"match", "MATCH"},         // Используется для сопоставления значения с шаблоном
            //    //{"mod", "MOD"},             // Определяет модуль
            //    //{"move", "MOVE"},           // Используется для перемещения владения значения
            //    {"mut", "MUT"},             // Позволяет изменять значение переменной
            //    //{"pub", "PUB"},             // Делает элемент публичным
            //    //{"ref", "REF"},             // Создает ссылку на значение
            //    {"return", "RETURN"},       // Возвращает значение из функции
            //    //{"self", "SELF"},           // Ссылка на текущий тип или модуль
            //    //{"static", "STATIC"},       // Определяет статическую переменную
            //    //{"struct", "STRUCT"},       // Определяет структуру
            //    //{"super", "SUPER"},         // Ссылка на родительский модуль
            //    //{"trait", "TRAIT"},         // Определяет типаж
            //    {"true", "TRUE"},           // Булевое значение, которое обозначает истину
            //    {"type", "TYPE"},           // Определяет новый тип
            //    //{"unsafe", "UNSAFE"},       // Используется для написания небезопасного кода
            //    //{"use", "USE"},             // Импортирует элементы из модуля
            //    //{"where", "WHERE"},         // Используется для ограничения типов
            //    {"while", "WHILE"},         // Начинает цикл, который выполняется, пока условие истинно

            //    {"main", "MAIN"},
            //    {"println!", "PRINT"},
            //};
            
            // Словарь для хранения НЕ используемых whitespace лексем, и их поиска
            Dictionary<string, string> dickOfLex_01_dontUseKeyWord = new Dictionary<string, string>
            {             
                // Ключевые слова, которые должны встречаться между пробелами

                //{"as", "AS"},               // Используется для явного приведения типов
                //{"break", "BREAK"},         // Прерывает выполнение цикла
                {"const", "CONST"},         // Определяет константу
                //{"continue", "CONTINUE"},   // Переходит к следующей итерации цикла
                {"crate", "CRATE"},         // Используется для обозначения текущего крейта (пакета)
                //{"else", "ELSE"},           // Определяет блок кода, который должен выполниться в случае, если условие не истинно
                {"enum", "ENUM"},           // Определяет перечисление
                {"extern", "EXTERN"},       // Указывает на то, что функция или переменная определены в другом модуле
                //{"false", "FALSE"},         // Булевое значение, которое обозначает ложь
                //{"fn", "FUNCTION"},         // Определяет функцию
                //{"for", "FOR"},             // Начинает цикл по итератору
                //{"if", "IF"},               // Определяет блок кода, который должен выполниться в случае, если условие истинно
                {"impl", "IMPL"},           // Определяет реализацию типажа (trait) для типа
                //{"in", "IN"},               // Используется в циклах для обхода итераторов
                //{"let", "LET"},             // Определяет переменную
                //{"loop", "LOOP"},           // Начинает бесконечный цикл
                {"match", "MATCH"},         // Используется для сопоставления значения с шаблоном
                {"mod", "MOD"},             // Определяет модуль
                {"move", "MOVE"},           // Используется для перемещения владения значения
                //{"mut", "MUT"},             // Позволяет изменять значение переменной
                {"pub", "PUB"},             // Делает элемент публичным
                {"ref", "REF"},             // Создает ссылку на значение
                //{"return", "RETURN"},       // Возвращает значение из функции
                {"self", "SELF"},           // Ссылка на текущий тип или модуль
                {"static", "STATIC"},       // Определяет статическую переменную
                {"struct", "STRUCT"},       // Определяет структуру
                {"super", "SUPER"},         // Ссылка на родительский модуль
                {"trait", "TRAIT"},         // Определяет типаж
                //{"true", "TRUE"},           // Булевое значение, которое обозначает истину
                //{"type", "TYPE"},           // Определяет новый тип
                {"unsafe", "UNSAFE"},       // Используется для написания небезопасного кода
                {"use", "USE"},             // Импортирует элементы из модуля
                {"where", "WHERE"},         // Используется для ограничения типов
                //{"while", "WHILE"},         // Начинает цикл, который выполняется, пока условие истинно
                //
                //{"main", "MAIN"},
                //{"println!", "PRINT"},
                {"->", "RETURN_TYPE"},
                {"=>", "PATTERM_MATCH"},
                {"::", "INSIDE_LINK"},
                {"_", "NEW_PATTERM"},
                {"<<", "BITWISE_OPERATION__SHL"},
                {">>", "BITWISE_OPERATION__SHR"},
            };
            

            // Словарь для хранения литерных лексем
            Dictionary<string, string> dickOfCont = new Dictionary<string, string>
            {
                {"{", "START_VOID"},
                {"}", "END_VOID"},

                {"()", "NULL_ARGUMENT"},
                {"(", "OPEN_BRACKET"},
                {")", "CLOSED_BRACKET"},

                {"[", "OPEN_SQUEA_BRACKET"},
                {"]", "CLOSED_SQUEA_BRACKET"},

                {"\"", "DOUBLE_QUOTAT"},
                {"'", "ONCE_QUOTAT"},

                {"->", "RETURN_TYPE"},//
                {"=>", "PATTERM_MATCH"},//

                {".", "DOT"},
                {",", "COMMA"},
                {":", "COLON"},
                {";", "END_LINE"},

                {"_", "NEW_PATTERM"},//

                {"::", "INSIDE_LINK"},//
                //{"//", "Удалили какой-то наверно очень важный комментарий :)"},

                {"+", "ARITHMETIC_OPERATION__ADD"},
                {"-", "ARITHMETIC_OPERATION__SUB"},
                {"*", "ARITHMETIC_OPERATION__MULT"},
                {"/", "ARITHMETIC_OPERATION__DIV"},
                {"%", "ARITHMETIC_OPERATION__REM_DIV"},

                {"==", "COMPARISON_OPERATION__EQUAL"},
                {"!=", "COMPARISON_OPERATION__INEQUAL"},
                {"<", "COMPARISON_OPERATION__LESS"},
                {">", "COMPARISON_OPERATION__GREAT"},
                {"<=", "COMPARISON_OPERATION__LESS_EQ"},
                {">=", "COMPARISON_OPERATION__GREAT_EQ"},

                {"&&", "LOGICAL_OPERATION__AND"},
                {"||", "LOGICAL_OPERATION__OR"},
                {"!", "LOGICAL_OPERATION__NOT"},

                {"+=", "ASSIGNMENT_OPERATION__ADD_ASS"},
                {"-=", "ASSIGNMENT_OPERATION__SUB_ASS"},
                {"*=", "ASSIGNMENT_OPERATION__MUL_ASS"},
                {"/=", "ASSIGNMENT_OPERATION__DIV_ASS"},
                {"%=", "ASSIGNMENT_OPERATION__REM_ASS"},
                {"=", "ASSIGNMENT_OPERATION__SET"},

                {"&", "BITWISE_OPERATION__AND"},
                {"|", "BITWISE_OPERATION__OR"},
                {"^", "BITWISE_OPERATION__XOR"},
                {"<<", "BITWISE_OPERATION__SHL"},//
                {">>", "BITWISE_OPERATION__SHR"},//

                //{"u64", "NUM_TYPE_LONG_INT"}
            };

            // Словарь символов, между которыми мы ставим пробелы
            Dictionary<string, string> dickOfCont_2 = new Dictionary<string, string>
            {
                // Важное условие: тут не должно быть символов, которые являются частью других ключей
                // Например, тут не должно быть символа :, если мы хотим распознавать лексему ::

                {"{", "START_VOID"},
                {"}", "END_VOID"},

                {"(", "OPEN_BRACKET"},
                {")", "CLOSED_BRACKET"},

                {"[", "OPEN_SQUEA_BRACKET"},
                {"]", "CLOSED_SQUEA_BRACKET"},

                //{"\"", "DOUBLE_QUOTAT"},
                //{"'", "ONCE_QUOTAT"},

                {".", "DOT"},
                {",", "COMMA"},
                {";", "END_LINE"},
                {"->", "RETURN_TYPE"},
                {"=>", "PATTERM_MATCH"},

                {"::", "INSIDE_LINK"},

                {"==", "COMPARISON_OPERATION__EQUAL"},
                {"!=", "COMPARISON_OPERATION__INEQUAL"},
                //{"<", "COMPARISON_OPERATION__LESS"},
                //{">", "COMPARISON_OPERATION__GREAT"},
                {"<=", "COMPARISON_OPERATION__LESS_EQ"},
                {">=", "COMPARISON_OPERATION__GREAT_EQ"},

                {"&&", "LOGICAL_OPERATION__AND"},
                {"||", "LOGICAL_OPERATION__OR"},

                {"+", "ARITHMETIC_OPERATION__ADD"},
                {"-", "ARITHMETIC_OPERATION__SUB"}, //// Почему то была закомментирована
                {"*", "ARITHMETIC_OPERATION__MULT"},
                {"%", "ARITHMETIC_OPERATION__REM_DIV"},

                // {"+=", "ASSIGNMENT_OPERATION__ADD_ASS"}, // Эти операци следует исключить из возможностей парсера, для стаибльности
                // {"-=", "ASSIGNMENT_OPERATION__SUB_ASS"},
                // {"*=", "ASSIGNMENT_OPERATION__MUL_ASS"},
                // {"/=", "ASSIGNMENT_OPERATION__DIV_ASS"},
                // {"%=", "ASSIGNMENT_OPERATION__REM_ASS"},

                //{"=", "ASSIGNMENT_OPERATION__SET"},

                //{"&", "BITWISE_OPERATION__AND"},
                //{"|", "BITWISE_OPERATION__OR"},
                {"^", "BITWISE_OPERATION__XOR"},
                {"<<", "BITWISE_OPERATION__SHL"},
                {">>", "BITWISE_OPERATION__SHR"},
            };

            // Метод чтения строк из входного файла
            public void OpenInputFiles(string nameInputFile)
            {
                if (nameInputFile == "") nameInputFile = InputNameFile; // Имя файла можно не указывать, при вызове этой процедуры

                try
                {
                    using (StreamReader fs = new StreamReader(nameInputFile))
                    {
                        string currentLine = "";
                        while ((currentLine = fs.ReadLine()) != null) // Построчно считывает
                        {
                            //Console.WriteLine(currentLine);
                            InputLine.Add(currentLine); // И построчно добавляет в двумерный лист
                        }
                    }
                    //return true;
                }
                catch (Exception e)
                {
                    print("Файл не может быть прочитан:");
                    print(e.Message);
                    //return false;

                    ExitCode = "Err_Read_Inp_File";
                    errorDescr = "Произошла ошибка, при чтении входного файла Input.txt";
                }

                if (InputLine.Count() == 0)
                {
                    err_empty_input_file = true;
                    print("@@ Входной файл пуст! ");
                    ExitCode = "Err_Read_Inp_File";
                    errorDescr = "Входной файл не был прочитан, или массив строк пуст.";
                    return;
                }

                //OutOfInputFile(); // Выводит весь файл в консоль
                OutOfInputFile_newColoringComments();
                improvingFileReadability_v2(); // Улучшает входной файл                        
                //OutOfInputFile(); // Выводит изменённый файл в консоль // Раскомментируйте эту строку, что бы посмотреть промежуточный результат

            }

            public List<int> indForQuoat = new List<int>();

            public List<string> ListRecognisedUnsupportedLexems = new List<string>();
            public bool isRecognisedUnsupportedLexems = false; // Были ли распознаны неподдерживаемые лексемы?

            public void CheckUnsupporterKeyWordsInclude()
            {             
                for (int i = 0; i < outpTable.Count(); i+=2)
                {
                    if (dickOfLex_01_dontUseKeyWord.ContainsValue(outpTable[i + 1]))
                    {
                        // Если одна из распознанных лексем, является неподдерживаемой

                        isRecognisedUnsupportedLexems = true;
                        ListRecognisedUnsupportedLexems.Add(outpTable[i + 1]);
                    }
                }

                //InputLine
                // dickOfLex_01_dontUseKeyWord
                // outpTable

                if (isRecognisedUnsupportedLexems)
                {
                    print("");
                    print("! Во входной программе были распознаны неподдерживаемые ключевые слова, которые не могу быть обработаны данным транслятором.");
                    print("К сожалению, этот транслятор - учебный проект, и реализовать функционал переноса всех ключевых слов, с их возможностями - была бы непосильная задача");
                    print("");
                    print("Конкретно эти лексемы в вашем коде мы не можем обработать: ");

                    string str1 = "";

                    foreach (var item in ListRecognisedUnsupportedLexems)
                    {
                        str1 += item + ", ";
                    }

                    str1 = str1.Substring(0, str1.Length - 2);
                    print(str1);

                    print(" ");
                    print("Пожалуйста, введите программу, без таких ключевых слов.");
                    
                    print(" ");
                    //print("-----------------------");
                    PrintSeparator();

                    ExitCode = "Unsp_KyWords";
                    errorDescr = "Обнаружены неподдерживаемые ключевые слова.";
                }
            }

            // В этой процедуре мы ставим пробелы до и после всех служебных символов. Это улучшает распознавание токенов
            // Все служебные символы описаны в словаре dickOfCont_2
            // Например, строка из файла "let n=5;" превращается в "let n = 5 ; "
            public void improvingFileReadability_v2()
            {
                /*
                    Короче, тут мы действуем так:
                    
                    1) Проходим по каждой строке входного файла
                    2) Цикл while работает до тех пор, пока мы не внесли все требуемые изменения
                    3) В foreach мы проходим по каждому элементу словаря dickOfCont_2
                    4) Для каждого такого элемента мы выполняем поиск в строке
                        • Если поиск успешен - мы ставим пробелы между найденными символами
                        • А также запоминаем индекс этого символа, и дальше продолжаем поиск не с начала строки, а с этого индекса

                */

                for (int i = 0; i < InputLine.Count; i++)
                {
                    // Флаг, указывающий, были ли выполнены изменения в строке
                    bool changesMade = true;
                    int countWh = 0;

                    // Пока производятся изменения в строке, продолжаем поиск
                    while (changesMade)
                    {
                        // Сброс флага изменений
                        changesMade = false;
                        countWh++;

                        // Проход по каждой подстроке в словаре и вставка пробелов
                        foreach (KeyValuePair<string, string> entry in dickOfCont_2)
                        {
                            int indd = -1;

                            // Тут мы игнорируем распознавание символов и вставку пробелов между ними, в строках
                            // Например, строка из фходного файла:  let my_string = "Привет, мир!".to_string();
                            // Будет преобразована в строку:        let my_string = "Привет, мир!" . to_string ( ) ;
                            // Игнорируя символы , и ! в строке.
                            // Также этот кусок кода работает с произвольным количеством строк в исходной строке
                            {
                                bool searchQuoatIsEneble = true;
                                indForQuoat = new List<int>();

                                while (searchQuoatIsEneble)
                                {
                                    int firstIndex = -1;
                                    int secondIndex = -1;
                                    int trithIndex = -1;

                                    if (indForQuoat.Count == 0) firstIndex = InputLine[i].IndexOf("\"");
                                    else firstIndex = InputLine[i].IndexOf("\"", indForQuoat[indForQuoat.Count - 1]);

                                    if (firstIndex != -1)
                                    {
                                        secondIndex = InputLine[i].IndexOf("\"", firstIndex + 1);
                                    }

                                    if (secondIndex != -1)
                                    {
                                        trithIndex = InputLine[i].IndexOf("\"", secondIndex + 1);

                                        indForQuoat.Add(firstIndex);
                                        indForQuoat.Add(secondIndex);
                                    }

                                    if (trithIndex == -1)
                                    {
                                        searchQuoatIsEneble = false;
                                    }
                                }
                            }


                            for (int u = 0; u < 1; u++)
                            {
                                string substring = entry.Key;
                                int index = InputLine[i].IndexOf(substring, indd+1);

                                if (index != -1)
                                {
                                    /*
                                        Примеры, как окружение может располагаться, если мы распознаём лексему ::

                                        __::__
                                        ::____
                                        ____::
                                        _ ::__
                                        __:: _
                                    */

                                    bool isOk = true;

                                    for (int p = 0; p < indForQuoat.Count; p++)
                                    {
                                        if ((index >= indForQuoat[p]) && (index <= indForQuoat[p+1]))
                                        {
                                            isOk = false;
                                            //print(InputLine[i] + "\nindex " + index + " >= " + indForQuoat[p] + " или index <= " +
                                            // + indForQuoat[p + 1] + ", блокируем проверку токена " + entry.Key);
                                        }

                                        p++;
                                    }

                                    if (isOk == true)
                                    {
                                        // Проверка, что перед и после подстроки нет пробелов
                                        if ((index == 0 || InputLine[i][index - 1] != ' ') &&
                                            (index + substring.Length == InputLine[i].Length || InputLine[i][index + substring.Length] != ' '))
                                        {
                                            // Вставка пробелов перед и после подстроки
                                            InputLine[i] = InputLine[i].Insert(index, " ");
                                            InputLine[i] = InputLine[i].Insert(index + substring.Length + 1, " ");
                                            // Установка флага изменений
                                            changesMade = true;
                                        }
                                        else if ((index != 0) && (InputLine[i][index - 1] != ' ') &&
                                            (index + substring.Length < InputLine[i].Length && InputLine[i][index + substring.Length] == ' ')) // __:: _
                                        {
                                            InputLine[i] = InputLine[i].Insert(index, " ");
                                            changesMade = true;
                                        }
                                        else if ((index != 0) && (InputLine[i][index - 1] == ' ') &&
                                            (index + substring.Length < InputLine[i].Length && InputLine[i][index + substring.Length] != ' ')) // _ ::__
                                        {
                                            InputLine[i] = InputLine[i].Insert(index + substring.Length, " ");
                                            changesMade = true;
                                        }

                                        int buf = index;
                                        if (indd != index)
                                        {
                                            index = indd;
                                            indd = buf;
                                            u--;
                                        }

                                        //print(InputLine[i]);   
                                    }
                                }
                            }
                        }
                    }

                    //print("countWh = " + countWh);
                    //print("");
                }                
            }

            // Выводим весь массив строк, считанный из входного файла
            public void OutOfInputFile()
            {
                print("\nВходной файл:");

                if (InputLine.Count != 0)
                    InputLine.ForEach(print);
                else
                {
                    print("Входной файл не был прочитан, или массив строк пуст.");
                    ExitCode = "Err_Read_Inp_File";
                    errorDescr = "Входной файл не был прочитан, или массив строк пуст.";
                }
            }

            public void OutOfInputFile_newColoringComments()
            {
                print("\nВходной файл:");

                if (InputLine.Count != 0)
                {
                    foreach (var line in InputLine)
                    {
                        if (line.StartsWith("//"))
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            print(line);
                            Console.ResetColor();
                        }
                        else
                        {
                            print(line);
                        }
                    }
                }
                else
                {
                    print("Входной файл не был прочитан, или массив строк пуст.");
                    ExitCode = "Err_Read_Inp_File";
                    errorDescr = "Входной файл не был прочитан, или массив строк пуст.";
                }
            }

            // Реализация простейшей процедуры поиска в словаре
            /*
            public void SearchOfDick()
            {
                string res = null;
                string inp = "if";
                dickOfLex.TryGetValue(inp, out res);
                if (res == null)
                {
                    print("Не нашли элемент " + inp);
                }
                else
                {
                    print("Код элемента " + inp + " = " + res);
                }
            }
            */

            // Нерекурсивная процедура поиска в массиве
            public string SearchOfDick(string inp) // Dictionary - словарь, сокр. - dick
            {
                string res = null;
                //dickOfLex.TryGetValue(inp, out res); // Ищем элемент по ключу inp. В случае успеха, найденное значение положится в переменную res
                dickOfCont.TryGetValue(inp, out res);
                return res;
            }

            bool insertToEnd = false; // Флаг. Нужен для корректной вставки распознанных лексем в буферную очередь

            // Рекурсивная процедура поиска в массиве (основная)
            public string SearchOfDickOnRecurs(string inp) 
            {
                string res = null;
                dickOfLex_01.TryGetValue(inp, out res); // Ищем в массиве по ключу
                if (res == null) dickOfCont.TryGetValue(inp, out res);

                if (res == null) // Если не нашли
                {
                    string newInp = "";

                    // Обяснение, что тут за сложный код:
                    {
                        /*
                            Тут я реализовал алгоритм уменьшения строки. Т.е., если мы не смогли найти лексему в массиве при поиске,
                            это не означает, что в строке её нет. Их там может быть, кстати, много

                            Например, нас попалась такая строка: "(int("

                            Ниже, в первом for я посимвольно создаю новую строку, с левого конца
                            Вот так это выглядит:

                            i = 1: "("
                            i = 2: "(i"
                            i = 3: "(in"
                            ...

                            Далее, я на каждом шаге запускаю процедуру нерекурсивного поиска. Например, мы нашли лексему "(", при i = 1.
                            Тогда, я отправляю её в начало очереди буферных лексем

                            Это сделано потомучто по другому не сделать

                            Далее, я создаю новую строку, без тех символов, которые я опознал. В этом примере она будет выглядеть так: "int("
                            И запускаю эту рекурсивную процедуру ещё раз, уже с этой новой строкой

                            ---

                            Понятно, что строку "int(" не получится идентефицировать, если брать символы слева
                            И логично будет пойти с другой строрны - справа

                            Этим как раз и занимается второй for ниже
                            Вот так будет выглядеть его работа:

                            i = 1: "("
                            i = 2: "t("
                            i = 3: "nt("
                            i = 4: "int("
                            ...

                            Он опознает скобку, уже на первом шаге, и дальше строка "int" снова уйдёт в рекурсию, и верно идентефицируется.

                            В принципе это всё, что нужно знать, для понимания
                        */
                    }
                    
                    for (int i = 1; i < inp.Length; i++)
                    {
                        newInp = new String(inp.ToCharArray(), 0, i);
                        string searchNewInp = SearchOfDick(newInp);

                        if (searchNewInp != null)
                        { 
                            string finalInp = new String(inp.ToCharArray(), i, (inp.Length - i));
                            //print("finalInp = " + finalInp);

                            buferOfLexemsDetectionForDick.Add(newInp);
                            buferOfLexemsDetectionForDick.Add(searchNewInp);

                            insertToEnd = true;
                            SearchOfDickOnRecurs(finalInp);

                            return null;
                        }
                    }

                    for (int i = 1; i < inp.Length; i++)
                    {
                        newInp = new String(inp.ToCharArray(), i, inp.Length-i);
                        string searchNewInp = SearchOfDick(newInp);

                        if (searchNewInp != null)
                        {
                            string finalInp = new String(inp.ToCharArray(), 0, i);

                            buferOfLexemsDetectionForDick.Add(newInp);
                            buferOfLexemsDetectionForDick.Add(searchNewInp);

                            insertToEnd = false;
                            SearchOfDickOnRecurs(finalInp);

                            return null;
                        }
                    }
                }
                else // Если нашли лексему при поиске в словаре
                {
                    if (buferOfLexemsDetectionForDick.Count != 0)
                    {
                        buferOfLexemsDetectionForDick.Add(inp);
                        buferOfLexemsDetectionForDick.Add(res);

                        return res;
                    }
                    else
                        return res;
                }

                if (buferOfLexemsDetectionForDick.Count != 0) 
                {
                    int adrInp;
                    if (int.TryParse(inp, out adrInp) == true) // Тут, если наша новая лексема - число
                    {
                        tokenDetection(inp);
                        return res;
                    }

                    if (insertToEnd == false) // Тут, либо добавление в начало очереди (если мы шли с правого конца строки, вторым for)
                    {
                        buferOfLexemsDetectionForDick.Insert(0, inp);
                        buferOfLexemsDetectionForDick.Insert(1, "ID");
                    }
                    else // Либо добавление в конец очереди (если мы шли с левого конца строки, первым фором)
                    {
                        buferOfLexemsDetectionForDick.Add(inp);
                        buferOfLexemsDetectionForDick.Add("ID");
                    }
                }
                else
                    return res;

                return res;
            }

            public List<string> buferOfLexemsDetectionForDick = new List<string>();
            // Буферный массив для распознанных лексем, которых было больше одной в строке, полученной процедурой SearchOfDickOnRecurs()

            public static bool isErrSymbol = false;

            // Это основная процедура построковой и посимвольной обработки файла
            // Она разделяет все входящие лексемы, которые отделены пробелами, и без разбора отправляет в tokenDetection
            public void processingLine()
            {
                for (int i = 0; i < InputLine.Count; i++)
                {
                    for (int j = 0; j < InputLine[i].Length; j++)
                    {
                        //(!(char.IsLetterOrDigit(InputLine[i][j]) || char.IsPunctuation(InputLine[i][j])))
                        // Фильтрует лишние символы, которые не попадают под русскую или английскую раскладку
                        if (!(((InputLine[i][j] >= 32) && (InputLine[i][j] <= 126)) || (((InputLine[i][j] >= 1039) && (InputLine[i][j] <= 1105)))))
                        {
                            //print("\nВстречен недопустимый символ: " + InputLine[i][j] + "\nРазбор документа остановлен");
                            ExitCode = "Def_Symv";
                            string s1 = "";

                            if (InputLine[i][j] == '	')
                            {
                                s1 = "Табы не поддержываюца ☺";
                            }

                            errorDescr = "\nВстречен недопустимый символ: " + InputLine[i][j] + "\nРазбор документа остановлен\n" + s1;

                            isErrSymbol = true;
                            return;
                        }
                    }

                    if (InputLine[i] != "")
                    {
                        char currentChar = InputLine[i][0];
                        string bufer = "";

                        bool isString = false;

                        for (int j = 0; j < InputLine[i].Length; j++)
                        {
                            int index = InputLine[i].IndexOf("//"); // Находим индекс первого вхождения "//" в строке
                            if (index >= 0) // Если нашлось вхождение "//" в строке
                            {
                                InputLine[i] = InputLine[i].Substring(0, index); // Удаляем все символы, которые следуют за "//", вместе с этим символом
                                continue;
                            }

                            if (InputLine[i][j] == '"')
                            {
                                if (isString == false)
                                {
                                    if (bufer.Length > 0)
                                    {
                                        tokenDetection(bufer);
                                        //print(bufer);
                                        bufer = "";
                                    }

                                    isString = true;
                                }
                                else
                                {
                                    isString = false;

                                    outpTable.Add(bufer + "\"");
                                    outpTable.Add("STRING");
                                    //print("123");

                                    bufer = "";
                                    continue;
                                }
                            }

                            if (isString == false)
                            {
                                if ((InputLine[i][j] != ' ') && (InputLine[i][j] != '	'))
                                {
                                    bufer += InputLine[i][j];
                                }
                                else
                                {
                                    if (bufer.Length > 0)
                                    {
                                        tokenDetection(bufer);
                                        //print(bufer);
                                        bufer = "";
                                    }
                                }
                            }
                            else
                            {
                                bufer += InputLine[i][j];
                            }
                        }
                        if(bufer.Length>0) tokenDetection(bufer);
                    }
                }

                CurrentedFloatNumbers(); // Собирает все числа
                OnceQuotatSwap(); // Исправляет небольшую неточность, при распознавании одинарных кавычек
                CorrectedInput();
                CorrectedInput2();
                CorrectedInput3();
                ExtrudeInitFunctionName();

                printFinalTable(); // А в конце печатает финальную таблицу с распознанными лексемами
                CheckUnsupporterKeyWordsInclude(); // Проверяет, не содержится ли в распознанных лексемах неподдерживаемых ключевых слов                
            }

            public bool isTrue2 = true;

            // Заполняет массив всех инициализированных названий функций
            public void ExtrudeInitFunctionName()
            {
                // ListFunctionName

                for (int i = 0; i < outpTable.Count - 2; i += 2)
                {
                    if (outpTable[i+1] == "FUNCTION")
                    {
                        if (outpTable[i + 1 + 2] == "ID")
                        {
                            ListFunctionName.Add(outpTable[i + 1 + 2 - 1]);
                            if (isTrue2 == true)
                            {
                                isTrue2 = false;
                                print("");
                            }
                            print("Добавили имя функции в объявленную область: " + outpTable[i + 1 + 2 - 1]);
                        }
                    }
                }
            }

            // Корректриовка обозначения цикла for
            public void CorrectedInput3()
            {
                for (int i = 0; i < outpTable.Count; i += 2)
                {
                    if (outpTable[i] == "or")
                    {
                        outpTable[i] = "for";
                        outpTable[i + 1] = "FOR";
                    }
                }
            }

            // Заменяю Else If на ElIf
            public void CorrectedInput2()
            {
                for (int i = 2; i < outpTable.Count - 2; i += 2)
                {
                    if (outpTable[i + 1] == "ELSE")
                    {
                        if (outpTable[i + 1 + 2] == "IF")
                        {
                            outpTable[i] = "elif";
                            outpTable[i + 1] = "ELIF";

                            // Удаляю следующие 2 ячейки, т.к. они больше не нужны
                            for (int j = 0; j < 2; j++)
                            {
                                outpTable.RemoveAt(i + 2);
                            }
                        }
                    }
                }
            }

            public void CorrectedInput()
            {
                for (int i = 2; i < outpTable.Count; i += 2)
                {
                    if (outpTable[i] == "et")
                    {
                        outpTable[i] = "let";
                        outpTable[i+1] = "LET";
                    }
                }
            }

            public void CurrentedFloatNumbers()
            {
                for (int i = 2; i < outpTable.Count - 6; i += 2)
                {
                    if (outpTable[i + 1] == "NUM_INT")
                    {
                        if (outpTable[i + 3] == "DOT")
                        {
                            if (outpTable[i + 5] == "NUM_INT")
                            {
                                outpTable[i + 1] = "NUM_FLOAT";

                                if (outpTable[i + 3] == "COMMA")
                                {
                                    // Если разделитель - это запятая, а не точка, то кидаю ошибку, т.к. это не соответствует стандартам кода

                                    ExitCode = "Err_Def_Float_Number";
                                    errorDescr = "Неверная инициализация значения float - для разеления целой и десятичной части, нужно использовать точку, а не запятую. Ошибка на месте: " + (i/2) + 2;

                                    isErrSymbol = true;
                                    return;
                                }

                                // Кобинирую новое значение переменной - float
                                outpTable[i] = outpTable[i] + outpTable[i + 2] + outpTable[i + 4];

                                // Удаляю следующие 4 ячейки, т.к. они больше не нужны
                                for (int j = 0; j < 4; j++)
                                {
                                    outpTable.RemoveAt(i + 2); 
                                }
                            }
                        }
                    }    
                }
            }

            // Исправляет небольшую неточность, при распознавании одинарных кавычек
            public void OnceQuotatSwap()
            {
                for (int i = 2; i < outpTable.Count - 2; i += 2)
                {
                    if (outpTable[i] == "'")
                    {
                        if (outpTable[i + 2] == "'")
                        {
                            if (outpTable[i - 1] == "ID")
                            {
                                //print("Нашли несоответствие кавычек на " + i/2 + " месте");
                                string id_name = outpTable[i - 2];
                                string id_obozn = outpTable[i - 1];

                                outpTable[i - 2] = "'";
                                outpTable[i - 1] = "ONCE_QUOTAT";

                                outpTable[i] = id_name;
                                outpTable[i + 1] = id_obozn;
                            }
                        }
                    }
                }
            }

            public void tokenDetection(string inp) // Принимает либо лексему, либо несколько лексем в одной строке, подряд
            {
                //print("Распознаём: " + inp);

                int adrInp;
                if (int.TryParse(inp, out adrInp) == true) // Если полученная строка является числом
                {
                    // То это число
                    int indexOfDott = inp.IndexOf('.');
                    if (indexOfDott != -1)
                    {
                        string h = inp.Remove(indexOfDott);
                        if (h.IndexOf('.') != -1)
                        {

                            outpTable.Add(inp);
                            outpTable.Add("ERROR_INTERPR_NUM"); // Если в строке больше одной точки - то это не число, это ошибка

                            // 1.1
                        }
                        else
                        {
                            outpTable.Add(inp);
                            outpTable.Add("NUM_FLOAT"); // Это не тестировал
                        }
                    }
                    else
                    {
                        outpTable.Add(inp);
                        outpTable.Add("NUM_INT");
                    }
                }
                else // Если строка не число
                {
                    string res = SearchOfDickOnRecurs(inp); // Распознаём лексему по словарю (рекурсивно)

                    // Если лексема была в строке одна, и её не нашлось в словаре - то это ID
                    if ((res == null) && (buferOfLexemsDetectionForDick.Count == 0)) 
                    {
                        outpTable.Add(inp);
                        outpTable.Add("ID");

                        /*
                        // Если в строке сначала цифры, потом буквы
                        if (isStrOnNoInt(inp) == true)
                        {
                            outpTable.Add(inp);
                            outpTable.Add("ID");
                        }
                        else
                        {
                            outpTable.Add(inp);
                            outpTable.Add("ERROR");
                        }
                        */

                        //print("Строка не число");
                    }
                    else if ((res != null) && (buferOfLexemsDetectionForDick.Count == 0)) // Если лексема нашлась в словаре, и была одна
                    {
                        outpTable.Add(inp);
                        outpTable.Add(res);
                    }
                    else
                    {
                        detectingOfBuferLexems(); // Если лексем в строке было несколько
                    }
                }

                //printFinalTable();
            }

            public void detectingOfBuferLexems()
            {
                // Наверняка вы заметили, что я не создал 2 массива, или не создал вложенные массивы, для хранения значений [NAME, TOKEN]
                // Всё проще. Чётные значения в листе - это NAME, а нечётные - это TOKEN

                for (int i = 0; i < buferOfLexemsDetectionForDick.Count; i+=2)
                {
                    outpTable.Add(buferOfLexemsDetectionForDick[i]);
                    outpTable.Add(buferOfLexemsDetectionForDick[i + 1]);
                }

                buferOfLexemsDetectionForDick.Clear(); // После каждой итерации распознавания, не забываю чистить буферную очередь
            }

            public int maxLengthFexem = 0;   // Максимальная длинна распознанных лексем 

            // Находит самое длинное название лексемы
            /*
            public int get_maxLengthFexem()
            {
                if (maxLengthFexem != 0) return maxLengthFexem;
                else
                {
                    for (int i = 0; i < outpTable.Count; i+=2)
                    {
                        if (outpTable[i].Length > maxLengthFexem)
                        {
                            //print(">>> " + outpTable[i]);
                            maxLengthFexem = outpTable[i].Length;
                        }
                    }
                }

                return maxLengthFexem;
            }
            */

            // Печатает финальную таблицу с распознанными лексемами
            public void printFinalTable()
            {
                print("\nВсе распознанные лексемы: \n");

                //get_maxLengthFexem();

                maxLengthFexem = 10;

                for (int i = 0; i < outpTable.Count; i += 2)
                {
                    // Тут опять немного сложного кода, для красивого вывода)

                    println(i / 2);

                    if (outpTable.Count < 100)
                    {
                        if ((i / 2 < 10)) println("  ");
                        else println(" ");
                    }
                    else
                    {
                        if ((i / 2 < 10)) println("  ");
                        else if ((i / 2 < 100)) println(" ");
                    }

                    println("  " + outpTable[i]);

                    int a_size = maxLengthFexem - outpTable[i].Length;

                    if (a_size <= 0) a_size = 1;

                    string newSpase = new String(' ', a_size);
                    println(newSpase);

                    print(" -  " + outpTable[i + 1]);
                }
            }

            public bool isStrOnNoInt(string str)
            {
                bool isDigitsFirst = true; // флаг, указывающий, что сначала должны быть цифры
                for (int i = 0; i < str.Length; i++)
                {
                    if (char.IsDigit(str[i])) // если текущий символ - цифра
                    {
                        if (!isDigitsFirst) // если мы уже встречали буквы в строке
                        {
                            //Console.WriteLine("Строка не соответствует условию: сначала цифры, потом буквы");
                            return true;
                        }
                    }
                    else // иначе - это буква
                    {
                        isDigitsFirst = false; // сбрасываем флаг, так как мы уже встретили буквы
                    }
                }

                //Console.WriteLine("Строка соответствует условию: сначала цифры, потом буквы");
                return (false);
            }
        }
    }
}
