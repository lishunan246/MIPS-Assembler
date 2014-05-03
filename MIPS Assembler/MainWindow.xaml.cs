using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace MIPS_Assembler
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>

    enum insFormat { R, J, I }
    public partial class MainWindow : Window
    {
        private OpenFileDialog openFileDialog = null;
        private SaveFileDialog saveFileDialog = null;
        private SaveFileDialog saveFileDialog2 = null;

        private int baseAddress = 0;

        private string[] resultToPrint;
        public MainWindow()
        {
            InitializeComponent();
            openFileDialog = new OpenFileDialog();
            openFileDialog.FileOk += openFileDialogFileOk;

            saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "COE files (*.coe)|*.coe|txt files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog.FilterIndex = 3;
            saveFileDialog.RestoreDirectory = true;

            saveFileDialog.FileOk+=saveFileDialog_FileOk;

            saveFileDialog2 = new SaveFileDialog();

            saveFileDialog2.Filter = "COE files (*.coe)|*.coe|txt files (*.txt)|*.txt|汇编程序(*.asm)|*.asm|All files (*.*)|*.*";
            saveFileDialog2.FilterIndex = 3;
            saveFileDialog2.RestoreDirectory = true;

            saveFileDialog2.FileOk += saveFileDialog2_FileOk;
        }

        private void saveFileDialog2_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string fullPathname = saveFileDialog2.FileName;
            FileInfo src = new FileInfo(fullPathname);
            FileStream fs = src.Create();
            fs.Close();
            StreamWriter sw = src.CreateText();

            string temp = "";
            foreach (string line in resultToPrint)
            {
                temp += line + '\n';
            }            

            sw.Write(temp);
            sw.Close();
        }

        private void saveFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string fullPathname = saveFileDialog.FileName;
            FileInfo src = new FileInfo(fullPathname);
            FileStream fs = src.Create();
            fs.Close();
            StreamWriter sw = src.CreateText();
            
            sw.Write(code.Text);
            sw.Close();
        }

        private void openClick(object sender, RoutedEventArgs e)
        {
            openFileDialog.ShowDialog();
        }

        private void openFileDialogFileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string fullPathname = openFileDialog.FileName;
            FileInfo src = new FileInfo(fullPathname);
            TextReader reader = src.OpenText();
            displayData(reader);
        }
        private void displayData(TextReader reader)
        {
            code.Text = "";
            string line = reader.ReadLine();
            while (line != null)
            {
                code.Text += line + "\n";
                line = reader.ReadLine();
            }
            reader.Dispose();
        }

        private void saveFileClick(object sender, RoutedEventArgs e)
        {
            saveFileDialog.ShowDialog();
        }

 
        private void saveResultClick(object sender, RoutedEventArgs e)
        {
            saveFileDialog2.ShowDialog();
        }

        private void code_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void assembleClick(object sender, RoutedEventArgs e)
        {
            string[] codes = code.Text.Split('\n');
            resultToPrint = doAssemble(codes);
            printResult(resultToPrint,"address");
        }

        private string[] removesharp(string[] codes)
        {
            int i = 0;
            string[] asm = codes;
            string[] temp = null;
            foreach (string line in codes)
            {
                if (line.Contains("#"))
                {
                    temp = line.Split('#');
                    asm[i] = temp[0];
                }
                else
                {
                    if (line != null)
                    {
                        asm[i] = line;
                    }
                }
                i++;
            }
            return asm;
        }

        private string[] removeComment(string[] codes)
        {
            int i = 0;
            string[] asm = codes;
            string[] temp = null;
            foreach (string line in codes)
            {
                if (line.Contains("//"))
                {
                    temp = line.Split('/');
                    asm[i] = temp[0];
                }
                else
                {
                    if (line != null)
                    {
                        asm[i] = line;
                    }
                }
                i++;
            }
            return asm;
        }

        private string[] doAssemble(string[] codes)//汇编
        {
            string[] machineCode;
            string[] asm = doStyle(codes);
            machineCode = dealPseudo(asm);
            machineCode = doStyle(machineCode);
            machineCode = dealBranch(machineCode);
            machineCode = dealInstruction(machineCode);
            return machineCode;
        }

        private string[] dealInstruction(string[] machineCodes)//处理真正的指令
        {
            insFormat insType;   
            for (int i = 0; i < machineCodes.Length; i++)
            {
                string[] instruction = machineCodes[i].Split(new char[] { ' ' }, 2);
                instruction[1] = instruction[1].Remove(instruction[1].Length - 1);//除去最后一个字符：分号
                string[] operant = instruction[1].Split(',');
                string[] temp;
                switch (instruction[0])
                {
                    case "add":
                        insType = insFormat.R;
                        machineCodes[i] = "000000" + regN(operant[1]) + regN(operant[2]) + regN(operant[0]) + "00000"+"100000";
                        break;
                    case "addu":
                        insType = insFormat.R;
                        machineCodes[i] = "000000" + regN(operant[1]) + regN(operant[2]) + regN(operant[0]) + "00000100001";
                        break;
                    case "sub":
                        insType = insFormat.R;
                        machineCodes[i] = "000000" + regN(operant[1]) + regN(operant[2]) + regN(operant[0]) + "00000100010";
                        break;
                    case "subu":
                        insType = insFormat.R;
                        machineCodes[i] = "000000" + regN(operant[1]) + regN(operant[2]) + regN(operant[0]) + "00000100011";
                        break;
                    case "addi":
                        insType = insFormat.I;
                        machineCodes[i] = "001000" + regN(operant[1]) + regN(operant[0])+immToB16(operant[2]);
                        break;
                    case "addiu":
                        insType = insFormat.I;
                        machineCodes[i] = "001001" + regN(operant[1]) + regN(operant[0]) + immToB16(operant[2]);
                        break;
                    case "mult":
                        insType = insFormat.R;
                        machineCodes[i] = "000000" + regN(operant[0]) + regN(operant[1]) + "00000" + "00000"+hexTob6("18");
                        break;
                    case "multu":
                        insType = insFormat.R;
                        machineCodes[i] = "000000" + regN(operant[0]) + regN(operant[1]) + "00000" + "00000" + hexTob6("19");
                        break;
                    case "div":
                        insType = insFormat.R;
                        machineCodes[i] = "000000" + regN(operant[0]) + regN(operant[1]) + "00000" + "00000" + hexTob6("1a");
                        break;
                    case "divu":
                        insType = insFormat.R;
                        machineCodes[i] = "000000" + regN(operant[0]) + regN(operant[1]) + "00000" + "00000" + hexTob6("1b");
                        break;
                    case "lw":
                        insType = insFormat.I;
                        temp = operant[1].Split(new char[] {'(',')'});
                        machineCodes[i] = hexTob6("23") + regN(temp[1]) + regN(operant[0]) + immToB16(temp[0]);
                        break;
                    case "lh":
                        insType = insFormat.I;
                        temp = operant[1].Split(new char[] { '(', ')' });
                        machineCodes[i] = hexTob6("21") + regN(temp[1]) + regN(operant[0]) + immToB16(temp[0]);
                        break;
                    case "lhu":
                        insType = insFormat.I;
                        temp = operant[1].Split(new char[] { '(', ')' });
                        machineCodes[i] = hexTob6("25") + regN(temp[1]) + regN(operant[0]) + immToB16(temp[0]);
                        break;
                    case "lb":
                        insType = insFormat.I;
                        temp = operant[1].Split(new char[] { '(', ')' });
                        machineCodes[i] = hexTob6("20") + regN(temp[1]) + regN(operant[0]) + immToB16(temp[0]);
                        break;
                    case "lbu":
                        insType = insFormat.I;
                        temp = operant[1].Split(new char[] { '(', ')' });
                        machineCodes[i] = hexTob6("24") + regN(temp[1]) + regN(operant[0]) + immToB16(temp[0]);
                        break;
                    case "sw":
                        insType = insFormat.I;
                        temp = operant[1].Split(new char[] { '(', ')' });
                        machineCodes[i] = hexTob6("2b") + regN(temp[1]) + regN(operant[0]) + immToB16(temp[0]);
                        break;
                    case "sh":
                        insType = insFormat.I;
                        temp = operant[1].Split(new char[] { '(', ')' });
                        machineCodes[i] = hexTob6("29") + regN(temp[1]) + regN(operant[0]) + immToB16(temp[0]);
                        break;
                    case "sb":
                        insType = insFormat.I;
                        temp = operant[1].Split(new char[] { '(', ')' });
                        machineCodes[i] = hexTob6("28") + regN(temp[1]) + regN(operant[0]) + immToB16(temp[0]);
                        break;

                    case "lui":
                        insType = insFormat.I;
                        
                        machineCodes[i] = hexTob6("f") + "00000"+ regN(operant[0]) + immToB16(operant[1]);
                        break;

                    case "mfhi":
                        insType = insFormat.R;

                        machineCodes[i] = hexTob6("0") + "00000" + "00000" + regN(operant[0]) +"00000" +hexTob6("10");
                        break;
                    case "mflo":
                        insType = insFormat.R;

                        machineCodes[i] = hexTob6("0") + "00000" + "00000" + regN(operant[0])  +"00000"+ hexTob6("12");
                        break;

                    case "and":
                        insType = insFormat.R;
                         machineCodes[i] = "000000" + regN(operant[1]) + regN(operant[2]) + regN(operant[0]) +"00000" + hexTob6("24");
                        break;
                    case "andi":
                        insType = insFormat.I;
                        machineCodes[i] = hexTob6("c") + regN(operant[1]) + regN(operant[0]) + immToB16(operant[2]);
                        break;
                    case "or":
                        insType = insFormat.R;
                        machineCodes[i] = "000000" + regN(operant[1]) + regN(operant[2]) + regN(operant[0])  +"00000"+ hexTob6("25");
                        break;
                    case "ori":
                        insType = insFormat.I;
                        machineCodes[i] = hexTob6("d") + regN(operant[1]) + regN(operant[0]) + immToB16(operant[2]);
                        break;
                    case "xor":
                        insType = insFormat.R;
                        machineCodes[i] = "000000" + regN(operant[1]) + regN(operant[2]) + regN(operant[0]) +"00000" + hexTob6("26");
                        break;
                    case "nor":
                        insType = insFormat.R;
                        machineCodes[i] = "000000" + regN(operant[1]) + regN(operant[2]) + regN(operant[0]) +"00000" + hexTob6("27");
                        break;
                    case "slt":
                        insType = insFormat.R;
                        machineCodes[i] = "000000" + regN(operant[1]) + regN(operant[2]) + regN(operant[0]) +"00000" + hexTob6("2a");
                        break;
                    case "slti":
                        insType = insFormat.I;
                        machineCodes[i] = hexTob6("a") + regN(operant[1]) + regN(operant[0]) + immToB16(operant[2]);
                        break;
                    case "sll":
                        insType = insFormat.R;
                        machineCodes[i] = "000000" + "00000" + regN(operant[1]) + regN(operant[0]) + hexTob5(operant[2]) + hexTob6("0");
                        break;
                    case "srl":
                        insType = insFormat.R;
                        machineCodes[i] = "000000" + "00000" + regN(operant[1]) + regN(operant[0]) + hexTob5(operant[2]) + hexTob6("2");
                        break;
                    case "sra":
                        insType = insFormat.R;
                        machineCodes[i] = "000000" + "00000" + regN(operant[1]) + regN(operant[0]) + hexTob5(operant[2]) + hexTob6("3");
                        break;
                    case "sllv":
                        insType = insFormat.R;
                        machineCodes[i] = "000000" + regN(operant[1]) + regN(operant[2]) + regN(operant[0]) + "00000" + hexTob6("4");
                        break;
                    case "srlv":
                        insType = insFormat.R;
                        machineCodes[i] = "000000" + regN(operant[1]) + regN(operant[2]) + regN(operant[0]) + "00000" + hexTob6("6");
                        break;
                    case "srav":
                        insType = insFormat.R;
                        machineCodes[i] = "000000" + regN(operant[1]) + regN(operant[2]) + regN(operant[0]) + "00000" + hexTob6("7");
                        break;
                    case "beq":
                        insType = insFormat.I;
                        machineCodes[i] = hexTob6("4") + regN(operant[0]) + regN(operant[1]) + calculateAddress(operant[2]);
                        break;
                    case "bne":
                        insType = insFormat.I;
                        machineCodes[i] = hexTob6("5") + regN(operant[0]) + regN(operant[1]) + calculateAddress(operant[2]);
                        break;
                    case "j":
                        insType = insFormat.J;
                        machineCodes[i] = hexTob6("2") + calculateAddress26(operant[0]);
                        break;
                    case "jal":
                        insType = insFormat.J;
                        machineCodes[i] = hexTob6("3") + calculateAddress26(operant[0]);
                        break;
                    case "jr":
                        insType = insFormat.R;
                        machineCodes[i] = hexTob6("0") + regN(operant[0]) + "00000" + "00000" + "00000" + hexTob6("8");
                        break;
                    default:
                        break;
                }
            }
            return machineCodes;
        }

        private string calculateAddress26(string p)
        {
            p = p.Trim();
            int c = Convert.ToInt32(p, 10);
            c = baseAddress + 4 * (c - 1);
            return Convert.ToString(c, 2).PadLeft(26, '0');
        }

        private string calculateAddress(string p)
        {
            p = p.Trim();
            int c = Convert.ToInt32(p, 10);
            c = baseAddress + 4 * (c - 1);
            return Convert.ToString(c, 2).PadLeft(16, '0');
        }

        private string hexTob5(string p)
        {
            int i = Convert.ToInt32(p, 16);
            return Convert.ToString(i, 2).PadLeft(5, '0');
        }

        private string hexTob6(string p)
        {
            int i = Convert.ToInt32(p, 16);
            return Convert.ToString(i, 2).PadLeft(6, '0');
        }

        private string immToB16(string p)
        {
            int i;
            i = Convert.ToInt32(p, 16);
            return Convert.ToString(i, 2).PadLeft(16, '0');
        }

        private string regN(string p)
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
                        reg = p[2] - '0' + 24;
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

        private string[] dealPseudo(string[] asm)//处理所遇到的伪指令
        {
            bool hasLabel = false;
            string label="";
            string[] im = new string[2];
            for (int i = 0; i < asm.Length; i++)
            {
                try
                {
                    if (asm[i].Contains(':'))
                    {
                        hasLabel = true;
                        string[] temp = asm[i].Split(new char[] { ':' }, 2);
                        label = temp[0];
                        asm[i] = temp[1];
                    }

                    string[] instruction = asm[i].Split(new char[] { ' ' }, 2);
                    instruction[1] = instruction[1].Remove(instruction[1].Length - 1);//除去最后一个字符：分号
                    string[] operant = instruction[1].Split(',');
                    switch (instruction[0])
                    {
                        case "baseaddr":
                            asm[i] = "";
                            baseAddress = Convert.ToInt32(operant[0], 16);
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
                            im = immSplit(operant[1]);
                            asm[i] = "lui " + operant[0] + "," + im[0] + ";\nori " + operant[0] + "," + operant[0] + "," + im[1] + ";";
                            break;
                        case "li":
                            im = immSplit(operant[1]);
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
                        default:
                            break;
                    }
                    if (hasLabel)
                    {
                        asm[i] = label + ":" + asm[i];
                        hasLabel = false;
                    }
                }
                catch (IndexOutOfRangeException f)
                {
                    info.Text ="第"+(i+1).ToString()+"行："+ f.Message;
                }
            }
            return asm;
        }

        private string[] immSplit(string p)
        {
            p = p.PadLeft( 8,'0');
            string[] r = new string[2];
            r[0] = "";
            for(int i=0;i<4;i++)
            {
                r[0] += p[i].ToString();
            }
            for (int i = 4; i < 8; i++)
            {
                r[1] += p[i].ToString();
            }
            return r;
        }

        private string[] dealBranch(string[] asm)
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
                       info.Text = "格式错误：第" + (i + 1).ToString() + "行\n";
                   }
               }
           }
            return asm;
        }

        private string[] reFormat(string[] asm)
        {
            char[] t1 = { ';' };
            string[] t2;
            string temp="";

            foreach(string line in asm)
            {
                string t=line.Trim();
                temp += t;
            }
            t2= temp.Split(t1,StringSplitOptions.RemoveEmptyEntries);
            for(int i=0;i<t2.Length;i++)
            {
                t2[i] = t2[i].Trim();
                t2[i] = t2[i].ToLower();
                t2[i] += ';';
            }
            return t2;
        }

        private string[] removeEmptyline(string[] asm)
        {
            IEnumerable<string> re =
                from line in asm
                where line.Trim() != ""
                select line;

            return re.ToArray();
        }

        private void printResult(string[] a)
        {
            int i = 0;
            result.Text = "";
            foreach (string line in a)
            {
                i++;
                result.Text += i.ToString() + " ";
                result.Text += line+'\n';
            }
        }
        private void printResult(string[] a,string m)
        {
            if (m == "address")
            {
                int i = 0;
                result.Text = "";
                foreach (string line in a)
                {
                    i++;
                    int j = baseAddress + 4 * (i - 1);
                    result.Text += Convert.ToString(j, 2).PadLeft(32, '0') + ":";
                    result.Text += line + '\n';
                }
            }
        }

        private void styleCodeClick(object sender, RoutedEventArgs e)
        {
            string[] codes = code.Text.Split('\n');
            resultToPrint = doStyle(codes);
            printResult(resultToPrint);
        }

        private string[] doStyle(string[] codes)
        {
            
            string[] asm = removesharp(codes);
            asm = removeComment(asm);
            asm = removeEmptyline(asm);
            asm = reFormat(asm);
            return asm;
        }

        private void disassembleClick(object sender, RoutedEventArgs e)
        {
            string[] codes = code.Text.Split('\n');
            codes = removeEmptyline(codes);
            string[] result = doDisassem(codes);
            printResult(result);
        }

        private string[] doDisassem(string[] codes)
        {
            string opcode, rs, rt, rd, shamt, funct,immdediate,address;


            for (int i = 0; i < codes.Length; i++)
            {
                try
                {
                    if (codes[i].Contains(':'))
                    {
                        codes[i] = codes[i].Split(':')[1];
                    }
                    opcode = getOpcode(codes[i]);
                    address = getAddress(codes[i]);
                    rs = regNam(getRs(codes[i]));
                    rt = regNam(getRt(codes[i]));
                    immdediate = getImmediate(codes[i]);
                    int j=Convert.ToInt32(opcode, 2);
                    switch (j)
                    {
                        case 0:

                            rd = regNam(getRd(codes[i]));
                            shamt = getShamt(codes[i]);
                            funct = getFunct(codes[i]);
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
                                default:
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
                            codes[i] = "beq " + rs + "," + rt + "," + immdediate;
                            break;
                        case 5:
                            codes[i] = "bne " + rs + "," + rt + "," + immdediate;
                            break;
                        case 32:
                            codes[i] = "lb " + rt + "," + getC(codes[i]) + "(" + rs + ")";
                            break;
                        case 33:
                            codes[i] = "lh " + rt + "," + getC(codes[i]) + "(" + rs + ")";
                            break;
                        case 34:
                            codes[i] = "lwl " + rt + "," + getC(codes[i]) + "(" + rs + ")";
                            break;
                        case 35:
                            codes[i] = "lw " + rt + "," + getC(codes[i]) + "(" + rs + ")";
                            break;
                        case 36:
                            codes[i] = "lbu " + rt + "," + getC(codes[i]) + "(" + rs + ")";
                            break;
                        case 37:
                            codes[i] = "lhu " + rt + "," + getC(codes[i]) + "(" + rs + ")";
                            break;
                        case 38:
                            codes[i] = "lwr " + rt + "," + getC(codes[i]) + "(" + rs + ")";
                            break;
                        case 40:
                            codes[i] = "sb " + rt + "," + getC(codes[i]) + "(" + rs + ")";
                            break;
                        case 41:
                            codes[i] = "sh " + rt + "," + getC(codes[i]) + "(" + rs + ")";
                            break;
                        case 42:
                            codes[i] = "swl " + rt + "," + getC(codes[i]) + "(" + rs + ")";
                            break;
                        case 43:
                            codes[i] = "sw " + rt + "," + getC(codes[i]) + "(" + rs + ")";
                            break;
                        case 46:
                            codes[i] = "swr " + rt + "," + getC(codes[i]) + "(" + rs + ")";
                            break;


                        default:
                            break;
                    }
                    codes[i] += ";";
                }
                catch(FormatException)
                {
                    info.Text="格式错误，第"+(i+1).ToString()+"行";
                }
            }
            
        
            return codes;
           
        }

        private string getC(string p)
        {
            int a;
            string t = "";
            for (int i = 16; i < 32; i++)
                t += p[i].ToString();
            a = Convert.ToInt32(t, 2);
            return Convert.ToString(a,16) ;
        }

        private string getImmediate(string p)
        {
            int a;
            string t = "";
            for (int i = 16; i < 32; i++)
                t += p[i].ToString();
            a = Convert.ToInt32(t, 2) / 4 + 1;
            return a.ToString();
        }

        private string getAddress(string p)
        {
            int a;
            string t="";
            for (int i = 6; i < 32; i++)
                t += p[i].ToString();
            a = Convert.ToInt32(t, 2)/4+1;
            return a.ToString();
        }

        private string regNam(string p)
        {
            int r = Convert.ToInt32(p, 2);
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

        private string getOpcode(string p)
        {
            string t="";
            for (int i = 0; i < 6; i++)
                t += p[i].ToString();
            return t;
        }
        private string getRs(string p)
        {
            string t = "";
            for (int i = 6; i < 11; i++)
                t += p[i].ToString();
            return t;
        }
        private string getRt(string p)
        {
            string t = "";
            for (int i = 11; i < 16; i++)
                t += p[i].ToString();
            return t;
        }
        private string getRd(string p)
        {
            string t = "";
            for (int i = 16; i < 21; i++)
                t += p[i].ToString();
            return t;
        }

        private string getShamt(string p)
        {
            string t = "";
            for (int i = 21; i < 26; i++)
                t += p[i].ToString();
            int m = Convert.ToInt32(t, 2);
            return Convert.ToString(m, 16);
        }
        private string getFunct(string p)
        {
            string t = "";
            for (int i = 26; i < 32; i++)
                t += p[i].ToString();
            return t;
        }

        private void exchangeClick(object sender, RoutedEventArgs e)
        {
            string temp = code.Text;
            code.Text = result.Text;
            result.Text = temp;
            
        }
    }
}
