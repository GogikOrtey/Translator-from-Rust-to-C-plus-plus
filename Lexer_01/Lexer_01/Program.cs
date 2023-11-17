using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace Lexer_01
{
    // Программа: Лексер для языка Rust
    // Назначение: Разбивает входящий файл на языке Rust, на лексемы
    // Версия: 2.1 от 01.04.2023
    // Автор: Gogik Ortey

    // Знаком ! отмечены ещё не исправленные ошибки

    class Program
    {
        #region MyPrint
        // Реализую свои методы вывода в консоль (для удобства)
        public static void print<Type>(Type Input)
        {
            Console.WriteLine(Input);
        }

        public static void println<Type>(Type Input)
        {
            // Печать строки без переноса каретки
            Console.Write(Input);
        }
        #endregion

        static void Main()
        {
            Stopwatch st = new Stopwatch(); // Таймер
            st.Start();            
            
            // ----- Основной код лексера:
            
            MainLexer MainLexer = new MainLexer();

            MainLexer.OpenInputFiles("");   // Загружаем файл в массивы программы
            MainLexer.processingLine();     // Построково обрабатываем

            List<string> outpTable = MainLexer.outpTable; // В этом массиве будут хранится все разобранные лексемы


            // ----- Основной код парсера и семантического анализатора:

            MainParser MainParser = new MainParser();

            MainParser.SimpleOutput_of_all_Lexems(outpTable);

            bool semanticAnalExeption = MainParser.semanticAnalExeption;
            string[] outpFileString = MainParser.outpFileString;

            if (semanticAnalExeption == false) 
            {
                // ----- Основной код кодогенератора:

                CodeGen CodeGen = new CodeGen();
                CodeGen.mainVoid(outpFileString);
            }

            // -----

            print("-----");
            st.Stop();
            Console.Write("\nВремя выполнения программы: {0}", st.Elapsed.TotalSeconds); print(" секунд");
        }

        public class CodeGen
        {
            public string[] outpFileString = new string[200];

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

            public void mainVoid(string[] outpFileS)
            {
                outpFileString = outpFileS;

                print("");
                print("-----");
                print("");
                print("Кодогенератор С++");
                print("");

                int bufCurrLvl = 0;
                bool isBracet = false;

                for (int i = 0; i < outpFileString.Length; i++)
                {
                    //print(outpFileString[i]);
                    if (outpFileString[i] == null) break;

                    if ((outpFileString[i].Contains("FUNCTION")) && (outpFileString[i+1].Contains("NAME : MAIN")))
                    {
                        // Ваш код, который нужно выполнить, когда встретится слово "FUNCTION"
                        printLvlln("int main (");
                        isBracet = true;
                        continue;
                    }

                    if ((outpFileString[i].Contains("ARGUMENT")) && (outpFileString[i].Contains("NO_ARGUMENTS")))
                    {
                        printLvl(")");
                        isBracet = false;
                        //printLvl("{");
                        //lvl++;
                        continue;
                    }

                    if (outpFileString[i].Contains("INITIALIZATION_VAR"))
                    {
                        i++;
                        int indexOfColon = outpFileString[i].IndexOf(':');
                        string name = outpFileString[i].Substring(indexOfColon + 1).Trim();

                        i++;
                        int indexOfColon2 = outpFileString[i].IndexOf(':');
                        string type = outpFileString[i].Substring(indexOfColon2 + 1).Trim();

                        i++;
                        int indexOfColon3 = outpFileString[i].IndexOf(':');
                        string value = outpFileString[i].Substring(indexOfColon3 + 1).Trim();

                        if (type == "INT")
                        {
                            printLvlln("int ");
                        }
                        else if (type == "FLOAT")
                        {
                            printLvlln("float ");
                        }

                        print(name + " = " + value + ";");
                        continue;
                    }

                    if (outpFileString[i].Contains("WHILE"))
                    {
                        i+=2;
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
                        printLvl("{");
                        lvl++;
                        continue;
                    }

                    if (outpFileString[i].Contains("PRINT"))
                    {
                        i += 2;

                        int currentLvl_a = outpFileString[i].Length - outpFileString[i].TrimStart(' ').Length;
                        int currentLvl_b = currentLvl_a;

                        printLvlln("std::cout");

                        int indexOfColon = outpFileString[i].IndexOf(':');
                        string oper = outpFileString[i].Substring(indexOfColon + 1).Trim();
                        println(" << " + oper);

                        while ((i < outpFileString.Length) && (currentLvl_a == currentLvl_b))
                        {                            

                            i++;
                            if ((i >= outpFileString.Length)) break;
                            if ((outpFileString[i] == null)) break;
                            currentLvl_b = outpFileString[i].Length - outpFileString[i].TrimStart(' ').Length;
                            if ((currentLvl_a != currentLvl_b)) break;
                            indexOfColon = outpFileString[i].IndexOf(':');
                            oper = outpFileString[i].Substring(indexOfColon + 1).Trim();
                            println(" << " + oper);
                            //printLvl("outpFileString[i] = " + outpFileString[i]);
                        }
                        print(" << std::endl;");
                        lvl--;
                        printLvl("}");
                        continue;
                    }

                    if (outpFileString[i].Contains("ASSIGMENT"))
                    {
                        int indexOfColon = outpFileString[i].IndexOf(':');
                        string oper = outpFileString[i].Substring(indexOfColon + 1).Trim();

                        i++;
                        int indexOfColon2 = outpFileString[i].IndexOf(':');
                        string leftOp = outpFileString[i].Substring(indexOfColon2 + 1).Trim();

                        i++;
                        int indexOfColon3 = outpFileString[i].IndexOf(':');
                        string rightOp = outpFileString[i].Substring(indexOfColon3 + 1).Trim();

                        printLvl(leftOp + " " + oper + " " + rightOp + ";");
                        continue;
                    }

                    int currentLvl = outpFileString[i].Length - outpFileString[i].TrimStart(' ').Length;
                    //print("currentLvl = " + currentLvl);

                    if ((currentLvl - 2) == bufCurrLvl)
                    {
                        if (isBracet == false)
                        {
                            lvl--;
                            printLvl("}");
                            
                        }
                    }

                    bufCurrLvl = currentLvl;
                }

                if(lvl != 0) print("}");
            }
        }

        public class MainParser
        {
            public string[] outpFileString = new string[200];
            int indOnOutpMass = 0;

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
                outpFileString[indOnOutpMass] = str2 + str;
                indOnOutpMass++;
            }

            public void printLvlln(string str)
            {
                SpaseCr(lvl); println(str);
            }

            // ! Без обработки ошибок
            // ! Сделал только для функций

            
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
            // Обработка блоковых операторов
            public int BlockConstrProcessing(int i)
            {
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
                    printLvl("NAME : " + outpTable[new_i - 1]);
                    printLvl("ARGUMENT");
                }
                else
                {
                    printLvl("CONDITION");
                }

                // ! Вот сюда воткнуть обработку операций после аргумента, но до тела функции                

                // Внутри () - это аргумент

                while (outpTable[new_i - 1] != "OPEN_BRACKET") { new_i += 2; if (new_i > 100) break; } // Идём до ()
                inside_i = i;

                int subLvl = 0;
                coll = 0;
                lvl += 1;

                int stInd = new_i;

                while (outpTable[new_i + 1] != "START_VOID")
                {
                    new_i += 2;
                    if (new_i > 100) break;
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

                /*
                while (outpTable[new_i + 1] != "START_VOID") 
                {
                    string f = outpTable[new_i + 1];
                    if (f != "END_LINE" && f != "END_VOID" && f != "OPEN_BRACKET" && f != "CLOSED_BRACKET")
                    {
                        printLvl(outpTable[new_i + 1] + " : " + outpTable[new_i]);
                    }
                    coll++;

                    new_i += 2; 
                    if (new_i > 100) break; 
                }
                */

                if (coll == 0) 
                {
                    printLvl("NO_ARGUMENTS");
                }

                lvl -= 1;

                // Внутри {} - это тело

                while (outpTable[new_i - 1] != "START_VOID") { new_i += 2; if (new_i > 100) break; } // Идём до {}

                printLvl("BODY");
                lvl += 1;

                return new_i;
            }

            // Обработка строковых операторов
            public int StringConstrProcessing(int ui)
            {
                inside_i = ui;
                int i = ui;

                printLvl(outpTable[i + 1]);

                if (outpTable[i + 1] == "PRINT")
                {
                    i += 2;
                    printLvl("ARGUMENTS");
                    lvl++;
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
                        if (i > 100) break; 
                    }
                    return i;
                }

                lvl++;
                i += 2;

                while (outpTable[i - 1] != "OPEN_BRACKET") { i += 2; if (i > 100) break; } // Идём до (

                return i;
            }

            public class TreeNode
            {
                // Определяю класс вершины бинарного дерева, для определения операций

                public int ind = -1;
                public string[] data = new string[20]; 
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

            // Рекурсивная обработка бинарных операторов (у аргументов функций)
            public TreeNode mainBinarOperationProcessing(int indStart, int indFinish) // Индексы включительно
            {
                inside_i = indStart;
                TreeNode root = null;

                indFinish -= 4;
                //printLvl("--- Все лексемы у аргумента:");

                //print("Запускаю дерево разбора, начало - " + indStart/2 + ", конец - " + indFinish/2);

                int lvlPol = 0;

                //SpaseCr(lvl);
                for (int i = indStart; i <= indFinish; i+=2)
                {
                    //println(outpTable[i] + " ");

                    if (outpTable[i] == "(") lvlPol++;

                    if (outpTable[i] == ")") lvlPol--;

                    if ((CheckEqOper(outpTable[i]) == true) && (outpTable[i + 2] == "="))
                    {
                        root = new TreeNode(i, "ASSIGMENT : " + outpTable[i] + outpTable[i + 2]);
                        recurseBinOp(indStart, i - 2, root, 0);
                        recurseBinOp(i + 4, indFinish, root, 1);
                    }
                    else if ((outpTable[i] == "=") && (CheckEqOper(outpTable[i - 2]) == false))
                    {
                        root = new TreeNode(i, "ASSIGMENT : " + outpTable[i]);
                        recurseBinOp(indStart, i - 2, root, 0);
                        recurseBinOp(i + 2, indFinish, root, 1);
                    }
                    else if (CheckEqOper(outpTable[i]) == true)
                    {
                        if (lvlPol == 0)
                        {
                            root = new TreeNode(i, "OPERATION : " + outpTable[i]);
                            recurseBinOp(indStart, i - 2, root, 0);
                            recurseBinOp(i + 2, indFinish, root, 1);
                        }
                    }
                    else
                    {
                        //print("Операнд " + outpTable[i] + " не операция.");
                    }
                }
                //print("");
                return (root);
            }

            public void recurseBinOp(int indStart, int indFinish, TreeNode inp_root, int sideMode) 
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

                int lvlPol = 0;
                bool isOperationNode = false;
                int ind = 0;

                for (int i = indStart; i <= indFinish; i += 2)
                {
                    //println(outpTable[i] + " ");

                    //if (outpTable[i] == "(") lvlPol++;
                    //if (outpTable[i] == ")") lvlPol--;

                    if ((CheckEqOper(outpTable[i]) == true) && (outpTable[i + 2] == "="))
                    {
                        new_node.data[0] = "ASSIGMENT : " + outpTable[i] + outpTable[i + 2];
                        new_node.ind = i;

                        recurseBinOp(indStart, i - 2, new_node, 0);
                        recurseBinOp(i + 4, indFinish, new_node, 1);

                        isOperationNode = true;
                    }
                    else if ((outpTable[i] == "=") && (CheckEqOper(outpTable[i - 2]) == false))
                    {
                        new_node.data[0] = "ASSIGMENT : " + outpTable[i];
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
                            new_node.ind = i;

                            recurseBinOp(indStart, i - 2, new_node, 0);
                            recurseBinOp(i + 2, indFinish, new_node, 1);
                        }
                        isOperationNode = true;
                    }
                }

                for (int i = indStart; i <= indFinish; i += 2)
                {
                    if (!isOperationNode)
                    {
                        if (outpTable[i] != "(" && outpTable[i] != ")")
                            new_node.data[ind] = outpTable[i + 1] + " : " + outpTable[i];

                        ind++;
                    }
                }
            }

            // initialization
            public int initiVarProcessing(int inp_i)
            {
                inside_i = inp_i;
                int i = inp_i;                

                i -= 2;

                //print("Вошли в процедуру обработки LET. i = " + i + ", simv = " + outpTable[i]);

                if (outpTable[i + 1] == "LET")
                {
                    i += 2;
                    printLvl("INITIALIZATION_VAR");
                    lvl++;
                    //printLvl(+outpTable[i - 1]);

                    if (outpTable[i + 1] == "MUT") i += 2; // Игнорируем это ключевое слово

                    if (outpTable[i + 1] == "ID")
                    {
                        printLvl("NAME : " + outpTable[i]);
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
                                }
                            }
                        }
                    }

                    lvl--;
                }

                return i;
            }

            private int lvl;

            public string[] massBlockConstr = new string[] { "FUNCTION", "IF", "WHILE" };
            public string[] massStrConstr = new string[] { "PRINT" };

            List<string> outpTable;

            public void SimpleOutput_of_all_Lexems(List<string> outpTableInp)
            {
                try
                {
                    insideVoid(outpTableInp);
                }
                catch
                {
                    if (inside_i != 94)
                    {
                        //print("inside_i = " + inside_i);

                        string ex1 = "\nСемантический анализатор:\n";
                        string ex2 = "___________________\n";
                        string ex3 = "В процессе построения дерева разбора \nпрограммы произошла ошибка!\n";
                        string ex4 = "Подробности ошибки:\n";
                        string ex5 = "";
                        string ex6 = "\n";

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
                            }
                        }
                        //throw new Exception(ex1 + ex2 + ex3 + ex4 + ex5 + ex6);

                        print(ex1 + ex2 + ex3 + ex4 + ex5 + ex6);
                        semanticAnalExeption = true;
                    }
                }
            }

            /*
                Создать массив объявлённых переменных
                Сделать так, что бы во 2м примере всё работало
                Проверить, что бы во всех остальных примерах тоже всё работало

                Написать кодогенератор
                    Принимает на вход набор строк из парсера
                    По простому генерирует код, для каждого из 3х примеров
            */

            public bool semanticAnalExeption = false;

            public int inside_i;

            public void insideVoid(List<string> outpTableInp)
            {
                outpTable = outpTableInp;

                print(" ");
                print("Дерево разбора парсера: ");
                print(" ");

                for (int i = 0; i < outpTable.Count; i += 2)
                {
                    inside_i = i;
                    string lett;                    
                    string f = outpTable[i + 1];

                    // Вывожу значения операторов, только если они - числа, строки или названия
                    if (f == "ID" || f == "NUM_INT" || f == "STRING" || f == "NUM_FLOAT") 
                    {
                        lett = (outpTable[i + 1] + " : " + outpTable[i]);
                    } 
                    else 
                    {
                        lett = (outpTable[i + 1]);
                    }                    

                    if (massBlockConstr.Contains(f)) // Если это блоковый оператор
                    {
                        // Передаём управление в функцию обработки блоковых операторов
                        i = BlockConstrProcessing(i);
                        i -= 2;
                        continue;
                    }
                    
                    if (massStrConstr.Contains(f)) // Если строковый оператор
                    {
                        // Передаём управление в функцию обработки строковых операторов
                        i = StringConstrProcessing(i);
                        i -= 2;
                        continue;
                    }

                    if (massStrConstr.Contains(outpTable[i - 1])) // Если строковый оператор
                    {
                        i -= 2;
                        // Передаём управление в функцию обработки строковых операторов
                        i = StringConstrProcessing(i);
                        i -= 2;
                        continue;
                    }

                    /*
                        Вот тут он ловит строки, до знака ; и проверяет их на наличие арифм. операций
                        Если найдёт операцию - всю строку загоняет в дерево разбора бинарок
                    */

                    int oldI = i;
                    bool isUnvCont = false;

                    while (outpTable[i - 1] != "END_LINE") 
                    {
                        //print("i = " + i + ", simv = " + outpTable[i]);
                        if (outpTable[i - 1] == "LET")
                        {
                            i = initiVarProcessing(i);
                            //i += 4;
                            isUnvCont = true;
                            break;
                        }
                        else
                        {
                            //print(outpTable[i - 1] + " != LET");
                        }
                        i += 2;

                        if (outpTable[i - 1] == "OPEN_BRACKET") break;
                        if (outpTable[i - 1] == "START_VOID") break;

                        if (i > 100) break;
                    }

                    if (isUnvCont == true) 
                    {
                        isUnvCont = false;
                        continue;
                    }

                    f = outpTable[i + 1];

                    if (massBlockConstr.Contains(f)) // Если это блоковый оператор
                    {
                        // Передаём управление в функцию обработки блоковых операторов
                        i = BlockConstrProcessing(i);
                        i -= 2;
                        continue;
                    }

                    if (massStrConstr.Contains(f)) // Если строковый оператор
                    {
                        lvl--;
                        // Передаём управление в функцию обработки строковых операторов
                        i = StringConstrProcessing(i);
                        i -= 2;
                        continue;
                    }

                    // Если нашёл символ ; то он обрабатывает его как выражение
                    // Если там нет операций - он просто выводит его как строку
                    if (outpTable[i - 1] == "END_LINE")
                    {
                        //print("i_ = " + i + ", simv = " + outpTable[i]);

                        if (outpTable[i] == "let")
                        {
                            i = initiVarProcessing(i);
                            i += 2;
                            //isUnvCont = true;
                            continue;
                        }

                        int subLvl = 0;
                        coll = 0;
                        //lvl += 1;

                        //print("_oldI = " + oldI + " i = " + i);
                        if ((oldI == i-4) || (oldI == i - 6))
                        {
                            oldI -= 4;
                            //print("new - oldI = " + oldI + " i = " + i);
                        }
                        if (oldI == i - 2)
                        {
                            oldI += 2;
                            i += 12;
                            //print("-new - oldI = " + oldI + " i = " + i);
                        }

                        TreeNode root = mainBinarOperationProcessing(oldI, i);

                        if (root != null)
                        {
                            // Обходим и выводим дерево как КЛП, где каждый К - с новым lvl++

                            int oldLvl = lvl;
                            subLvl = PrintPreorderTraversal(root, subLvl);
                            lvl = oldLvl;
                        }
                        else
                        {
                            //print("Дерево = null");
                        }

                        lvl--;

                        i += 2; // ! Ошибка - Если идут 2 подряд разбора дерева переменных, он пролетает на 1 символ вперёд
                        continue;
                    }
                    else
                    {
                        i = oldI;
                    }

                    {
                        if (f == "END_VOID") lvl--;          // }

                        if (f == "OPEN_BRACKET") lvl++;      // (
                        if (f == "CLOSED_BRACKET") lvl--;    // )

                        if (f == "END_LINE") lvl--;          // ;

                        if (f != "END_LINE" && f != "END_VOID" && f != "OPEN_BRACKET" && f != "CLOSED_BRACKET")
                        {
                            printLvl(lett);
                        }
                    }
                }

                if (semanticAnalExeption == false)
                {
                    print(" ");
                    print("-----");
                    print("Семантический анализатор:");
                    print(" ");
                    print("Проблем не обнаружено.");
                }
            }
        }

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

            // Словарь для хранения whitespace лексем, и их поиска
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

                {"->", "RETURN_TYPE"},
                {"=>", "PATTERM_MATCH"},

                {".", "DOT"},
                {",", "COMMA"},
                {":", "COLON"},
                {";", "END_LINE"},

                {"_", "NEW_PATTERM"},

                {"::", "INSIDE_LINK"},
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
                {"<<", "BITWISE_OPERATION__SHL"},
                {">>", "BITWISE_OPERATION__SHR"},

                {"u64", "NUM_TYPE_LONG_INT"},
            };

            // Словарь символов, между которыми мы ставим пробелы
            Dictionary<string, string> dickOfCont_2 = new Dictionary<string, string>
            {
                // Важное условие: тут не должно быть символов, которые являются частью других ключей
                // Например, тут не должно быть символа :, если мы хотим разделить лексему ::

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
                    Console.WriteLine("The file could not be read:");
                    Console.WriteLine(e.Message);
                    //return false;
                }

                OutOfInputFile(); // Выводит весь файл в консоль
                improvingFileReadability_v2(); // Улучшает входной файл                        
                //OutOfInputFile(); // Выводит изменённый файл в консоль // Раскомментируйте эту строку, что бы посмотреть промежуточный результат
            }

            public List<int> indForQuoat = new List<int>();

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
                    InputLine.ForEach(Console.WriteLine);
                else
                    print("Входной файл не был прочитан, или массив строк пуст.");
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

            // Это основная процедура построковой и посимвольной обработки файла
            // Она разделяет все входящие лексемы, которые отделены пробелами, и без разбора отправляет в tokenDetection
            public void processingLine()
            {
                for (int i = 0; i < InputLine.Count; i++)
                {
                    for (int j = 0; j < InputLine[i].Length; j++)
                    {
                        //(!(char.IsLetterOrDigit(InputLine[i][j]) || char.IsPunctuation(InputLine[i][j])))
                        if (!(((InputLine[i][j] >= 32) && (InputLine[i][j] <= 126)) || (((InputLine[i][j] >= 1039) && (InputLine[i][j] <= 1105)))))
                        {
                            print("\nВстречен недопустимый символ: " + InputLine[i][j] + "\nРазбор документа остановлен");
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

                printFinalTable(); // А в конце печатает финальную таблицу с распознанными лексемами
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

                    if ((res == null) && (buferOfLexemsDetectionForDick.Count == 0)) // Если лексема была в строке одна, и её не нашлось в словаре - то это ID
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