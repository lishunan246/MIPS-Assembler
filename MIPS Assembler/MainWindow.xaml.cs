using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace MIPS_Assembler
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>

    enum InsFormat { R, J, I }
    public partial class MainWindow
    {
        private readonly OpenFileDialog _openFileDialog;
        private readonly SaveFileDialog _saveFileDialog;
        private readonly SaveFileDialog _saveFileDialog2;

        private int _baseAddress;

        private string[] _resultToPrint;
        public MainWindow()
        {
            InitializeComponent();
            _openFileDialog = new OpenFileDialog();
            _openFileDialog.FileOk += OpenFileDialogFileOk;

            _saveFileDialog = new SaveFileDialog();

            _saveFileDialog.Filter = Properties.Resources.saveFileDialogfilter;
            _saveFileDialog.FilterIndex = 3;
            _saveFileDialog.RestoreDirectory = true;

            _saveFileDialog.FileOk+=saveFileDialog_FileOk;

            _saveFileDialog2 = new SaveFileDialog();

            _saveFileDialog2.Filter = Properties.Resources.saveFileDialog2fliter;
            _saveFileDialog2.FilterIndex = 3;
            _saveFileDialog2.RestoreDirectory = true;

            _saveFileDialog2.FileOk += saveFileDialog2_FileOk;
        }

        private void saveFileDialog2_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string fullPathname = _saveFileDialog2.FileName;
            FileInfo src = new FileInfo(fullPathname);
            FileStream fs = src.Create();
            fs.Close();
            StreamWriter sw = src.CreateText();

                  

            sw.Write(Result.Text);
            sw.Close();
        }

        private void saveFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string fullPathname = _saveFileDialog.FileName;
            FileInfo src = new FileInfo(fullPathname);
            FileStream fs = src.Create();
            fs.Close();
            StreamWriter sw = src.CreateText();
            
            sw.Write(Code.Text);
            sw.Close();
        }

        private void OpenClick(object sender, RoutedEventArgs e)
        {
            _openFileDialog.ShowDialog();
        }

        private void OpenFileDialogFileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string fullPathname = _openFileDialog.FileName;
            FileInfo src = new FileInfo(fullPathname);
            TextReader reader = src.OpenText();
            DisplayData(reader);
        }
        private void DisplayData(TextReader reader)
        {
            Code.Text = "";
            string line = reader.ReadLine();
            while (line != null)
            {
                Code.Text += line + "\n";
                line = reader.ReadLine();
            }
            reader.Dispose();
        }

        private void SaveFileClick(object sender, RoutedEventArgs e)
        {
            _saveFileDialog.ShowDialog();
        }

 
        private void SaveResultClick(object sender, RoutedEventArgs e)
        {
            _saveFileDialog2.ShowDialog();
        }

        private void code_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void AssembleClick(object sender, RoutedEventArgs e)
        {
            string[] codes = Code.Text.Split('\n');
            _resultToPrint = DoAssemble(codes);
            PrintResult(_resultToPrint,"address");
        }

        private string[] Removesharp(string[] codes)
        {
            int i = 0;
            string[] asm = codes;
            foreach (string line in codes)
            {
                if (line.Contains("#"))
                {
                    var temp = line.Split('#');
                    asm[i] = temp[0];
                }
                else
                {
                    asm[i] = line;
                }
                i++;
            }
            return asm;
        }

        private static string[] RemoveComment(string[] codes)
        {
            int i = 0;
            string[] asm = codes;
            foreach (string line in codes)
            {
                if (line.Contains("//"))
                {
                    var temp = line.Split('/');
                    asm[i] = temp[0];
                }
                else
                {
                    asm[i] = line;
                }
                i++;
            }
            return asm;
        }

        private string[] DoAssemble(string[] codes)//汇编
        {
            string[] machineCode;
            string[] asm = DoStyle(codes);
            machineCode = DealPseudo(asm);
            machineCode = DoStyle(machineCode);
            machineCode = DealBranch(machineCode);

            machineCode = DealInstruction(machineCode);
           /* try
            {
                machineCode = dealInstruction(machineCode);
            }
            catch (FormatException f)
            {
                System.Windows.MessageBox.Show("在dealInstruction方法中"+f.Message);
            }
            */
            return machineCode;
        }

        private string[] DealInstruction(string[] machineCodes)//处理真正的指令
        {
            for (int i = 0; i < machineCodes.Length; i++)
            {
                string[] instruction = machineCodes[i].Split(new[] { ' ' }, 2);
                try
                {
                    instruction[1] = instruction[1].Remove(instruction[1].Length - 1);//除去最后一个字符：分号 
                    
                }
                catch (IndexOutOfRangeException f)
                {
                    System.Windows.MessageBox.Show("dealInstruction()方法中" + f.Message);
                
                }
                if(instruction.Length==1)
                {
                    System.Windows.MessageBox.Show("dealInstructon failed!");
                    return machineCodes;
                }
                string[] operant = instruction[1].Split(',');
                
               
                string[] temp;
                switch (instruction[0])
                {
                    case "add":
                        machineCodes[i] = "000000" + RegN(operant[1]) + RegN(operant[2]) + RegN(operant[0]) + "00000"+"100000";
                        break;
                    case "addu":
                        machineCodes[i] = "000000" + RegN(operant[1]) + RegN(operant[2]) + RegN(operant[0]) + "00000100001";
                        break;
                    case "sub":
                        machineCodes[i] = "000000" + RegN(operant[1]) + RegN(operant[2]) + RegN(operant[0]) + "00000100010";
                        break;
                    case "subu":
                        machineCodes[i] = "000000" + RegN(operant[1]) + RegN(operant[2]) + RegN(operant[0]) + "00000100011";
                        break;
                    case "addi":
                        machineCodes[i] = "001000" + RegN(operant[1]) + RegN(operant[0])+ImmToB16(operant[2]);
                        break;
                    case "addiu":
                        machineCodes[i] = "001001" + RegN(operant[1]) + RegN(operant[0]) + ImmToB16(operant[2]);
                        break;
                    case "mult":
                        machineCodes[i] = "000000" + RegN(operant[0]) + RegN(operant[1]) + "00000" + "00000"+HexTob6("18");
                        break;
                    case "multu":
                        machineCodes[i] = "000000" + RegN(operant[0]) + RegN(operant[1]) + "00000" + "00000" + HexTob6("19");
                        break;
                    case "div":
                        machineCodes[i] = "000000" + RegN(operant[0]) + RegN(operant[1]) + "00000" + "00000" + HexTob6("1a");
                        break;
                    case "divu":
                        machineCodes[i] = "000000" + RegN(operant[0]) + RegN(operant[1]) + "00000" + "00000" + HexTob6("1b");
                        break;
                    case "lw":
                        temp = operant[1].Split(new char[] {'(',')'});
                        machineCodes[i] = HexTob6("23") + RegN(temp[1]) + RegN(operant[0]) + ImmToB16(temp[0]);
                        break;
                    case "lh":
                        temp = operant[1].Split(new char[] { '(', ')' });
                        machineCodes[i] = HexTob6("21") + RegN(temp[1]) + RegN(operant[0]) + ImmToB16(temp[0]);
                        break;
                    case "lhu":
                        temp = operant[1].Split(new char[] { '(', ')' });
                        machineCodes[i] = HexTob6("25") + RegN(temp[1]) + RegN(operant[0]) + ImmToB16(temp[0]);
                        break;
                    case "lb":
                        temp = operant[1].Split(new char[] { '(', ')' });
                        machineCodes[i] = HexTob6("20") + RegN(temp[1]) + RegN(operant[0]) + ImmToB16(temp[0]);
                        break;
                    case "lbu":
                        temp = operant[1].Split(new char[] { '(', ')' });
                        machineCodes[i] = HexTob6("24") + RegN(temp[1]) + RegN(operant[0]) + ImmToB16(temp[0]);
                        break;
                    case "sw":
                        temp = operant[1].Split(new char[] { '(', ')' });
                        machineCodes[i] = HexTob6("2b") + RegN(temp[1]) + RegN(operant[0]) + ImmToB16(temp[0]);
                        break;
                    case "sh":
                        temp = operant[1].Split(new char[] { '(', ')' });
                        machineCodes[i] = HexTob6("29") + RegN(temp[1]) + RegN(operant[0]) + ImmToB16(temp[0]);
                        break;
                    case "sb":
                        temp = operant[1].Split(new char[] { '(', ')' });
                        machineCodes[i] = HexTob6("28") + RegN(temp[1]) + RegN(operant[0]) + ImmToB16(temp[0]);
                        break;

                    case "lui":

                        machineCodes[i] = HexTob6("f") + "00000"+ RegN(operant[0]) + ImmToB16(operant[1]);
                        break;

                    case "mfhi":

                        machineCodes[i] = HexTob6("0") + "00000" + "00000" + RegN(operant[0]) +"00000" +HexTob6("10");
                        break;
                    case "mflo":

                        machineCodes[i] = HexTob6("0") + "00000" + "00000" + RegN(operant[0])  +"00000"+ HexTob6("12");
                        break;

                    case "and":
                        machineCodes[i] = "000000" + RegN(operant[1]) + RegN(operant[2]) + RegN(operant[0]) +"00000" + HexTob6("24");
                        break;
                    case "andi":
                        machineCodes[i] = HexTob6("c") + RegN(operant[1]) + RegN(operant[0]) + ImmToB16(operant[2]);
                        break;
                    case "or":
                        machineCodes[i] = "000000" + RegN(operant[1]) + RegN(operant[2]) + RegN(operant[0])  +"00000"+ HexTob6("25");
                        break;
                    case "ori":
                        machineCodes[i] = HexTob6("d") + RegN(operant[1]) + RegN(operant[0]) + ImmToB16(operant[2]);
                        break;
                    case "xori":
                        machineCodes[i] = HexTob6("e") + RegN(operant[1]) + RegN(operant[0]) + ImmToB16(operant[2]);
                        break;
                    case "xor":
                        machineCodes[i] = "000000" + RegN(operant[1]) + RegN(operant[2]) + RegN(operant[0]) +"00000" + HexTob6("26");
                        break;
                    case "nor":
                        machineCodes[i] = "000000" + RegN(operant[1]) + RegN(operant[2]) + RegN(operant[0]) +"00000" + HexTob6("27");
                        break;
                    case "slt":
                        machineCodes[i] = "000000" + RegN(operant[1]) + RegN(operant[2]) + RegN(operant[0]) +"00000" + HexTob6("2a");
                        break;
                    case "slti":
                        machineCodes[i] = HexTob6("a") + RegN(operant[1]) + RegN(operant[0]) + ImmToB16(operant[2]);
                        break;
                    case "sll":
                        machineCodes[i] = "000000" + "00000" + RegN(operant[1]) + RegN(operant[0]) + HexTob5(operant[2]) + HexTob6("0");
                        break;
                    case "srl":
                        machineCodes[i] = "000000" + "00000" + RegN(operant[1]) + RegN(operant[0]) + HexTob5(operant[2]) + HexTob6("2");
                        break;
                    case "sra":
                        machineCodes[i] = "000000" + "00000" + RegN(operant[1]) + RegN(operant[0]) + HexTob5(operant[2]) + HexTob6("3");
                        break;
                    case "sllv":
                        machineCodes[i] = "000000" + RegN(operant[1]) + RegN(operant[2]) + RegN(operant[0]) + "00000" + HexTob6("4");
                        break;
                    case "srlv":
                        machineCodes[i] = "000000" + RegN(operant[1]) + RegN(operant[2]) + RegN(operant[0]) + "00000" + HexTob6("6");
                        break;
                    case "srav":
                        machineCodes[i] = "000000" + RegN(operant[1]) + RegN(operant[2]) + RegN(operant[0]) + "00000" + HexTob6("7");
                        break;
                    case "beq":
                        machineCodes[i] = HexTob6("4") + RegN(operant[0]) + RegN(operant[1]) + CalculateAddress(operant[2],i+2);
                        break;
                    case "bne":
                        machineCodes[i] = HexTob6("5") + RegN(operant[0]) + RegN(operant[1]) + CalculateAddress(operant[2], i + 2);
                        break;
                    case "j":
                        machineCodes[i] = HexTob6("2") + CalculateAddress26(operant[0]);
                        break;
                    case "jal":
                        machineCodes[i] = HexTob6("3") + CalculateAddress26(operant[0]);
                        break;
                    case "jr":
                        machineCodes[i] = HexTob6("0") + RegN(operant[0]) + "00000" + "00000" + "00000" + HexTob6("8");
                        break;
                }
            }
            return machineCodes;
        }

        private string CalculateAddress26(string p)
        {
            p = p.Trim();
            int c = Convert.ToInt32(p, 10);
            c = _baseAddress + c-1;
            return Convert.ToString(c, 2).PadLeft(26, '0');
        }

        private string CalculateAddress(string p,int a)
        {
            p = p.Trim();
            int c = Convert.ToInt32(p, 10)-a;
            c = _baseAddress + 4 * (c - 1);
            return Convert.ToString(c, 2).PadLeft(16, '0');
        }

        private string HexTob5(string p)
        {
            int i = Convert.ToInt32(p, 16);
            return Convert.ToString(i, 2).PadLeft(5, '0');
        }

        private string HexTob6(string p)
        {
            int i = Convert.ToInt32(p, 16);
            return Convert.ToString(i, 2).PadLeft(6, '0');
        }

        private string ImmToB16(string p)
        {
            int i;
            i = Convert.ToInt32(p, 16);
            return Convert.ToString(i, 2).PadLeft(16, '0');
        }

        private string RegN(string p)
        {
            int reg;
            p=p.Trim().ToLower();
            if (p[0] != '$')
            {
                p = p.TrimStart(new char[] { 'r' });
                reg = Convert.ToInt32(p, 10);
                return Convert.ToString(reg, 2).PadLeft(5, '0');
            }

            
            switch(p[1])
            {
                case 'z': reg = 0;
                    break;
                case 'v': reg = p[2] - '0' + 2;
                    break;
                case 'k': reg = p[2] - '0' + 26;
                    break;
                case 'a':
                    if (p[2] != 't')
                    {
                        reg = p[2] - '0' + 4;
                    }
                    else
                        reg = 1;
                    break;
                case 's':
                    if (p[2] == 'p')
                        reg = 29;
                    else
                        reg = p[2] - '0' + 16;
                    break;
                case 't':
                    if ((p[2] - '0') >= 8)
                        reg = p[2] - '0' + 24-8;
                    else
                        reg = p[2] - '0' + 8;
                    break;
                case 'g':
                    reg = 28;
                    break;
                case 'f':
                    reg = 30;
                    break;
                case 'r':
                    reg = 31;
                    break;
                default:
                    reg = -1;
                    break;
             }
            return Convert.ToString(reg,2).PadLeft(5,'0');
            
        }

        private string[] DealPseudo(string[] asm)//处理所遇到的伪指令
        {
            bool hasLabel = false;
            string label="";
            for (int i = 0; i < asm.Length; i++)
            {
                try
                {
                    if (asm[i].Contains(':'))
                    {
                        hasLabel = true;
                        var temp = asm[i].Split(new[] { ':' }, 2);
                        label = temp[0];
                        asm[i] = temp[1];
                    }

                    var instruction = asm[i].Split(new[] { ' ' }, 2);
                    instruction[1] = instruction[1].Remove(instruction[1].Length - 1);//除去最后一个字符：分号
                    var operant = instruction[1].Split(',');
                    string[] im;
                    switch (instruction[0])
                    {
                        case "baseaddr":
                            asm[i] = "";
                            _baseAddress = Convert.ToInt32(operant[0], 16);
                            break;
                        case "move":
                            asm[i] = "add " + operant[0] + "," + operant[1] + "," + "$zero;";
                            break;
                        case "clear":
                            asm[i] = "add " + operant[0] + ",$zero,$zero;";
                            break;
                        case "not":
                            asm[i] = "nor " + operant[0] + "," + operant[1] + "," + "$zero;";
                            break;
                        case "la":
                            im = ImmSplit(operant[1]);
                            asm[i] = "lui " + operant[0] + "," + im[0] + ";\nori " + operant[0] + "," + operant[0] + "," + im[1] + ";";
                            break;
                        case "li":
                            im = ImmSplit(operant[1]);
                            asm[i] = "lui " + operant[0] + "," + im[0] + ";\nori " + operant[0] + "," + operant[0] + "," + im[1] + ";";
                            break;
                        case "b":
                            asm[i] = "beq $zero,$zero," + operant[0] + ";";
                            break;
                        case "bal":
                            asm[i] = "bgezal $zero," + operant[0] + ";";
                            break;
                        case "bgt":
                            asm[i] = "slt $at," + operant[1] + "," + operant[0] + ";\nbne $at,$zero," + operant[2] + ";";
                            break;
                        case "blt":
                            asm[i] = "slt $at," + operant[0] + "," + operant[1] + ";\nbne $at,$zero," + operant[2] + ";";
                            break;
                        case "bge":
                            asm[i] = "slt $at," + operant[0] + "," + operant[1] + ";\nbeq $at,$zero," + operant[2] + ";";
                            break;
                        case "ble":
                            asm[i] = "slt $at," + operant[1] + "," + operant[0] + ";\nbeq $at,$zero," + operant[2] + ";";
                            break;
                        case "bgtu":
                            asm[i] = "sltu $at," + operant[1] + "," + operant[0] + ";\nbne $at,$zero," + operant[2] + ";";
                            break;
                        case "bgtz":
                            asm[i] = "slt $at," + "$zero" + "," + operant[0] + ";\nbne $at,$zero," + operant[1] + ";";
                            break;
                        case "beqz":
                            asm[i] = "beq " + operant[0] + ",$zero," + operant[1] + ";";
                            break;
                        case "mul":
                            asm[i] = "mult " + operant[1] + "," + operant[2] + ";\nmflo " + operant[0] + ";";
                            break;
                        case "div":
                            if (operant.Length == 3)
                                asm[i] = "div " + operant[1] + "," + operant[2] + ";\nmflo " + operant[0] + ";";
                            break;
                        case "rem":
                            asm[i] = "div " + operant[1] + "," + operant[2] + ";\nmfli " + operant[0] + ";";
                            break;
                    }
                    if (!hasLabel) continue;
                    asm[i] = label + ":" + asm[i];
                    hasLabel = false;
                }
                catch (IndexOutOfRangeException f)
                {
                    Info.Text ="第"+(i+1).ToString()+"行："+ f.Message;
                }
            }
            return asm;
        }

        private static string[] ImmSplit(string p)
        {
            p = p.PadLeft( 8,'0');
            var r = new string[2];
            r[0] = "";
            for(var i=0;i<4;i++)
            {
                r[0] += p[i].ToString();
            }
            for (int i = 4; i < 8; i++)
            {
                r[1] += p[i].ToString();
            }
            return r;
        }

        private string[] DealBranch(string[] asm)
        {
           for(int i=0;i<asm.Length;i++)
           {
               if(asm[i].Contains(":"))
               {
                   string[] temp=asm[i].Split(':');
                   asm[i]=temp[1];
                   string pattern =temp[0];
                   string replacement = (i+1).ToString();

                   try
                   {
                        Regex rgx= new Regex(pattern);
                   
                   
                       for(int j=0;j<asm.Length;j++)
                       {
                           asm[j]=rgx.Replace(asm[j],replacement);
                       }
                   }
                   catch (SystemException)
                   {
                       Info.Text = "格式错误：第" + (i + 1).ToString() + "行\n";
                   }
               }
           }
            return asm;
        }

        private static string[] ReFormat(IEnumerable<string> asm)
        {
            char[] t1 = { ';' };
            string temp= asm.Select(line => line.Trim()).Aggregate("", (current, t) => current + t);

            var t2 = temp.Split(t1,StringSplitOptions.RemoveEmptyEntries);
            for(var i=0;i<t2.Length;i++)
            {
                t2[i] = t2[i].Trim();
                t2[i] = t2[i].ToLower();
                t2[i] += ';';
            }
            return t2;
        }

        private static string[] RemoveEmptyline(IEnumerable<string> asm)
        {
            IEnumerable<string> re =
                from line in asm
                where line.Trim() != ""
                select line;

            return re.ToArray();
        }

        private void PrintResult(string[] a)
        {
            Result.Text = "";
            foreach (string line in a)
            {
               Result.Text += line+'\n';
            }
        }
        private void PrintResult(string[] a,string m)
        {
            if (m == "address")
            {
                int i = 0;
                Result.Text = "";
                foreach (string line in a)
                {
                    i++;
                    int j = _baseAddress + 4 * (i - 1);
                    Result.Text += Convert.ToString(j, 2).PadLeft(32, '0') + ":";
                    Result.Text += line + '\n';
                }
            }
            if (m == "null")
            {
                
                Result.Text = "";
                foreach (string line in a)
                {
                   
                    Result.Text += line + '\n';
                }
            }
        }

        private void StyleCodeClick(object sender, RoutedEventArgs e)
        {
            string[] codes = Code.Text.Split('\n');
            _resultToPrint = DoStyle(codes);
            PrintResult(_resultToPrint);
        }

        private string[] DoStyle(string[] codes)
        {
            
            string[] asm = Removesharp(codes);
            asm = RemoveComment(asm);
            asm = RemoveEmptyline(asm);
            asm = ReFormat(asm);
            return asm;
        }

        private void DisassembleClick(object sender, RoutedEventArgs e)
        {
            string[] codes = Code.Text.Split('\n');
            codes = RemoveEmptyline(codes);
            string[] rLocal = DoDisassem(codes);
            PrintResult(rLocal);
        }

        private string[] DoDisassem(string[] codes)
        {
            for (int i = 0; i < codes.Length; i++)
            {
                try
                {
                    if (codes[i].Contains(':'))
                    {
                        codes[i] = codes[i].Split(':')[1];
                    }
                    var opcode = GetOpcode(codes[i]);
                    var address = GetAddress(codes[i]);
                    var rs = RegNam(GetRs(codes[i]));
                    var rt = RegNam(GetRt(codes[i]));
                    var immdediate = GetImmediate(codes[i]);
                    int j=Convert.ToInt32(opcode, 2);
                    switch (j)
                    {
                        case 0:

                            var rd = RegNam(GetRd(codes[i]));
                            var shamt = GetShamt(codes[i]);
                            var funct = GetFunct(codes[i]);
                            switch (Convert.ToInt32(funct, 2))
                            {
                                case 0:
                                    codes[i] = "sll " + rd + "," + rt + "," + shamt;
                                    break;
                                case 2:
                                    codes[i] = "srl " + rd + "," + rt + "," + shamt;
                                    break;
                                case 3:
                                    codes[i] = "srl " + rd + "," + rt + "," + shamt;
                                    break;
                                case 4:
                                    codes[i] = "sllv " + rd + "," + rt + "," + rs;
                                    break;
                                case 6:
                                    codes[i] = "srlv " + rd + "," + rt + "," + rs;
                                    break;
                                case 7:
                                    codes[i] = "srav " + rd + "," + rt + "," + rs;
                                    break;
                                case 8:
                                    codes[i] = "jr " + rs;
                                    break;
                                case 9:
                                    codes[i] = "jalr " + rs + "," + rd;
                                    break;
                                case 16:
                                    codes[i] = "mfhi " + rd;
                                    break;
                                case 17:
                                    codes[i] = "mthi " + rd;
                                    break;
                                case 18:
                                    codes[i] = "mflo " + rd;
                                    break;
                                case 19:
                                    codes[i] = "mtlo " + rd;
                                    break;
                                case 24:
                                    codes[i] = "mult " + rs + "," + rt;
                                    break;
                                case 25:
                                    codes[i] = "multu " + rs + "," + rt;
                                    break;
                                case 26:
                                    codes[i] = "div " + rs + "," + rt;
                                    break;
                                case 27:
                                    codes[i] = "divu " + rs + "," + rt;
                                    break;
                                case 32:
                                    codes[i] = "add " + rd + "," + rs + "," + rt;
                                    break;
                                case 33:
                                    codes[i] = "addu " + rd + "," + rs + "," + rt;
                                    break;
                                case 34:
                                    codes[i] = "sub " + rd + "," + rs + "," + rt;
                                    break;
                                case 35:
                                    codes[i] = "subu " + rd + "," + rs + "," + rt;
                                    break;
                                case 36:
                                    codes[i] = "and " + rd + "," + rs + "," + rt;
                                    break;
                                case 37:
                                    codes[i] = "or " + rd + "," + rs + "," + rt;
                                    break;
                                case 38:
                                    codes[i] = "xor " + rd + "," + rs + "," + rt;
                                    break;
                                case 39:
                                    codes[i] = "nor " + rd + "," + rs + "," + rt;
                                    break;
                                case 42:
                                    codes[i] = "slt " + rd + "," + rs + "," + rt;
                                    break;
                                case 43:
                                    codes[i] = "sltu " + rd + "," + rs + "," + rt;
                                    break;
                            }
                            break;
                        case 2:
                            codes[i] = "j " + address;
                            break;
                        case 3:
                            codes[i] = "jal " + address;
                            break;
                        case 4:
                            codes[i] = "beq " + rs + "," + rt + "," +(Convert.ToInt32(immdediate,10)+i+2).ToString();
                            break;
                        case 5:
                            codes[i] = "bne " + rs + "," + rt + "," + (Convert.ToInt32(immdediate, 10) + i + 2).ToString();
                            break;
                        case 8:
                            codes[i] = "addi " + rt + "," + rs +","+ GetC(codes[i]);
                            break;
                        case 9:
                            codes[i] = "addiu " + rt + "," + rs + "," + GetC(codes[i]);
                            break;
                        case 10:
                            codes[i] = "slti " + rt + "," + rs + "," + GetC(codes[i]);
                            break;
                        case 11:
                            codes[i] = "sltiu " + rt + "," + rs + "," + GetC(codes[i]);
                            break;
                        case 12:
                            codes[i] = "andi " + rt + "," + rs + "," + GetC(codes[i]);
                            break;
                        case 13:
                            codes[i] = "ori " + rt + "," + rs + "," + GetC(codes[i]);
                            break;
                        case 14:
                            codes[i] = "xori " + rt + "," + rs + "," + GetC(codes[i]);
                            break;
                        case 15:
                            codes[i] = "lui " + rt +  "," + GetC(codes[i]);
                            break;
                        case 32:
                            codes[i] = "lb " + rt + "," + GetC(codes[i]) + "(" + rs + ")";
                            break;
                        case 33:
                            codes[i] = "lh " + rt + "," + GetC(codes[i]) + "(" + rs + ")";
                            break;
                        case 34:
                            codes[i] = "lwl " + rt + "," + GetC(codes[i]) + "(" + rs + ")";
                            break;
                        case 35:
                            codes[i] = "lw " + rt + "," + GetC(codes[i]) + "(" + rs + ")";
                            break;
                        case 36:
                            codes[i] = "lbu " + rt + "," + GetC(codes[i]) + "(" + rs + ")";
                            break;
                        case 37:
                            codes[i] = "lhu " + rt + "," + GetC(codes[i]) + "(" + rs + ")";
                            break;
                        case 38:
                            codes[i] = "lwr " + rt + "," + GetC(codes[i]) + "(" + rs + ")";
                            break;
                        case 40:
                            codes[i] = "sb " + rt + "," + GetC(codes[i]) + "(" + rs + ")";
                            break;
                        case 41:
                            codes[i] = "sh " + rt + "," + GetC(codes[i]) + "(" + rs + ")";
                            break;
                        case 42:
                            codes[i] = "swl " + rt + "," + GetC(codes[i]) + "(" + rs + ")";
                            break;
                        case 43:
                            codes[i] = "sw " + rt + "," + GetC(codes[i]) + "(" + rs + ")";
                            break;
                        case 46:
                            codes[i] = "swr " + rt + "," + GetC(codes[i]) + "(" + rs + ")";
                            break;
                    }
                    codes[i] += ";";
                }
                catch(FormatException f)
                {
                    string t="格式错误，第"+(i+1).ToString()+"行:"+f.Message;
                    System.Windows.MessageBox.Show(t);
                }
                catch(IndexOutOfRangeException f2)
                {
                    System.Windows.MessageBox.Show(f2.Message);
                }
                
            }
            
        
            return codes;
           
        }

        private string GetC(string p)
        {
            int a;
            string t = "";
            for (int i = 16; i < 32; i++)
                t += p[i].ToString();
            a = Convert.ToInt32(t, 2);
            return Convert.ToString(a,16) ;
        }

        private string GetImmediate(string p)
        {
            int a;
            string t = "";
            for (int i = 16; i < 32; i++)
                t += p[i].ToString();
            a = Convert.ToInt32(t, 2) / 4 + 1;
            return a.ToString();
        }

        private string GetAddress(string p)
        {
            int a;
            string t="";
            for (int i = 6; i < 32; i++)
                t += p[i].ToString();
            a = Convert.ToInt32(t, 2)+1;
            return a.ToString();
        }

        private static string RegNam(string p,bool x=false)
        {
            int r = Convert.ToInt32(p, 2);
            if (x)
            {
                if (r >= 8 && r <= 15)
                    return "$t" + (r - 8).ToString();
                else if (r >= 16 && r <= 23)
                    return "$s" + (r - 16).ToString();
                else
                    switch (r)
                    {
                        case 0:
                            return "$zero";
                        case 1:
                            return "$at";
                        case 2:
                            return "$v0";
                        case 3:
                            return "$v1";
                        case 4:
                            return "$a0";
                        case 5:
                            return "$a1";
                        case 6:
                            return "$a2";
                        case 7:
                            return "$a3";
                        case 24:
                            return "$t8";
                        case 25:
                            return "$t9";
                        case 26:
                            return "$k0";
                        case 27:
                            return "$k1";
                        case 28:
                            return "$gp";
                        case 29:
                            return "$sp";
                        case 30:
                            return "$fp";
                        case 31:
                            return "$ra";

                        default:
                            return "Unknown_Register";
                    }
            }
            else
            {
                return "r" + r.ToString();
            }
        }

        private string GetOpcode(string p)
        {
            string t="";
            for (int i = 0; i < 6; i++)
                t += p[i].ToString();
            return t;
        }
        private string GetRs(string p)
        {
            string t = "";
            for (int i = 6; i < 11; i++)
                t += p[i].ToString();
            return t;
        }
        private string GetRt(string p)
        {
            string t = "";
            for (int i = 11; i < 16; i++)
                t += p[i].ToString();
            return t;
        }
        private string GetRd(string p)
        {
            string t = "";
            for (int i = 16; i < 21; i++)
                t += p[i].ToString();
            return t;
        }

        private string GetShamt(string p)
        {
            string t = "";
            for (int i = 21; i < 26; i++)
                t += p[i].ToString();
            int m = Convert.ToInt32(t, 2);
            return Convert.ToString(m, 16);
        }
        private string GetFunct(string p)
        {
            string t = "";
            for (int i = 26; i < 32; i++)
                t += p[i].ToString();
            return t;
        }

        private void ExchangeClick(object sender, RoutedEventArgs e)
        {
            string temp = Code.Text;
            Code.Text = Result.Text;
            Result.Text = temp;
            
        }

        private void ReadCoeClick(object sender, RoutedEventArgs e)
        {
            string[] codes = Code.Text.Split(';');
           
            string[] t=codes[0].Split('=');

            if(t.Length==1)
            {
                System.Windows.MessageBox.Show("readCOE Failed!");
                return;
            }
            t[1] = t[1].Trim(); 
            int radix = Convert.ToInt32(t[1], 10);
            string[] t2 = codes[1].Split('=');
            string[] t3 = t2[1].Split(',');
            
            for(int i=0;i<t3.Length;i++)
            {
                t3[i] = t3[i].Trim();
                t3[i] = t3[i].Trim(new char[] {'\n','\r'});
                int temp = Convert.ToInt32(t3[i], radix);
                t3[i] = Convert.ToString(temp, 2).PadLeft(32, '0');
            }
            PrintResult(t3,"null");
        }

        private void GenCoeClick(object sender, RoutedEventArgs e)
        {
            string[] codes = Code.Text.Split('\n');
            codes = RemoveEmptyline(codes);
            string[] r2 = new string[2];
            r2[0] = "memory_initialization_radix=16;";
            r2[1] = "memory_initialization_vector=";

            for (int j = 0; j < codes.Length;j++ )
            {
                try
                {
                   
                    string s = codes[j].Trim();
                    if (s.Contains(':'))
                        s = s.Split(':')[1].Trim();
                    int i = Convert.ToInt32(s, 2);
                    s = Convert.ToString(i, 16).PadLeft(8, '0');
                    r2[1] += s;
                    if (j == codes.Length - 1)
                        r2[1] += ";";
                    else
                        r2[1] += ",";
                }
                catch (FormatException f)
                {
                    System.Windows.MessageBox.Show("在genCOEClick方法中 " + f.Message);
                    
                }
                catch (ArgumentOutOfRangeException f)
                {
                    System.Windows.MessageBox.Show("在genCOEClick方法中 "+f.Message);

                }
                
            }
            PrintResult(r2,"null");
        }
    }
}
