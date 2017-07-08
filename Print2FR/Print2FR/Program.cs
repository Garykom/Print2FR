using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Print2FR
{
    class Program
    {
        public static void Log(string msg)
        {
            Console.WriteLine(msg);

            string path = "" + AppDomain.CurrentDomain.BaseDirectory + "log.txt";
            System.IO.StreamWriter sw = System.IO.File.AppendText(path);
            try
            {
                string logLine = System.String.Format("{0:G}: {1}.", System.DateTime.Now, msg);
                sw.WriteLine(logLine);
            }
            finally
            {
                sw.Close();
            }
        }

        public static Stream GenerateStreamFromString(string s)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(s));
        }

        public static T DeserializeXMLFileToObject<T>(Stream xmlstream)
        {
            T returnObject = default(T);
            //if (string.IsNullOrEmpty(XmlFilename)) return default(T);
            try
            {
                StreamReader xmlStream = new StreamReader(xmlstream);
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                returnObject = (T)serializer.Deserialize(xmlStream);
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                //ExceptionLogger.WriteExceptionToConsole(ex, DateTime.Now);
            }
            return returnObject;
        }

        public static int AtolGetTaxByString(string TaxString)
        {
            int Result = -1;
            switch (TaxString)
            {
                case "18":
                    Result = 3;
                    break;
                case "10":
                    Result = 2;
                    break;
                case "0":
                    Result = 1;
                    break;
                case "none":
                    Result = 4;
                    break;
                default:
                    break;
            }
            return Result;
        }

        public static byte AtolGetTaxVariantByLong(long TaxVariant)
        {
            // Применяемая система налогооблажения в чеке:
            // ОСН - 1
            // УСН доход - 2
            // УСН доход-расход - 4
            // ЕНВД - 8
            // ЕСН - 16
            // ПСН - 32

            byte Result = 0;
            switch (TaxVariant)
            {
                case 0:
                    Result = 1;
                    break;
                case 1:
                    Result = 2;
                    break;
                case 2:
                    Result = 4;
                    break;
                case 3:
                    Result = 8;
                    break;
                case 4:
                    Result = 16;
                    break;
                case 5:
                    Result = 32;
                    break;
                default:
                    break;
            }
            return Result;
        }

        static void PrintFiscalString(FiscalString fs)
        {
            Log("FiscalString");
            string Name = fs.Name;
            Log("Name=" + Name);

            double Price = fs.Price;
            //string fsPrice = fs.Price.Replace('.', ',');
            //if (!Double.TryParse(fsPrice, out Price))
            //{
            //    Console.WriteLine("Error parse Price: " + fs.Price);
            //    FR.Abort();
            //    return;
            //};
            //Console.WriteLine("Price=" + Price.ToString());
            double Quantity = fs.Quantity;
            double Amount = fs.Amount;

            int Tax = AtolGetTaxByString(fs.Tax);
            if (Tax == -1)
            {
                Log("Tax = " + fs.Tax);
                FR.Abort();
                return;
            }

            int Department = Convert.ToInt32(fs.Department);

            int DiscountType = 0;
            double DiscontValue = 0;

            Log("Registration: Name=" + Name + " Price=" + Price + " Quantity=" + Quantity + " Tax=" + Tax);
            FR.Registration(Name, Price, Quantity, Tax, Department, DiscountType, DiscontValue);
        }

        static void PrintTextString(TextString ts)
        {
            FR.PrintString(ts.Text);
        }

        static void PrintBarcodeString(BarcodeString bs)
        {

        }

        static void ProcessCheck(CheckPackage checkPackage)
        {
            int Result;
            if (checkPackage.Positions.Items.Length == 0)
            {
                Log("Positions = 0");
                return;
            }

            Log("ProcessCheck");
            Log("Positions = "+ checkPackage.Positions.Items.Length);

            FR.Start();

            Log("SetMode ");
            Result = FR.SetMode(1, "30");
            Log("Result = " + Result);

            if (Result == -3822)
            {
                FR.PrintString("Смена превысила 24 часа!");
                FR.PrintString("Для продолжения снимите Z-Отчет.");
                FR.Feed(6);
                FR.PartialCut();
                FR.Stop();
                return;
            }

            int CheckMode = 1;
            int CheckType = Convert.ToInt32(checkPackage.Parameters.PaymentType);

            Log("OpenCheck CheckMode=" + CheckMode + " CheckType=" + CheckType);
            Result = FR.OpenCheck(CheckMode, CheckType);
            Log("Result = " + Result);

            // Применяемая система налогооблажения в чеке:
            long TaxVariant = AtolGetTaxVariantByLong(checkPackage.Parameters.TaxVariant);
            FR.WriteAttribute(1055, TaxVariant.ToString());

            // Позиции чека
            for (int i = 0; i < checkPackage.Positions.Items.Length; i++)
            {
                if (checkPackage.Positions.ItemsElementName[i] == ItemsChoiceType.FiscalString)
                {
                    PrintFiscalString((FiscalString)checkPackage.Positions.Items[i]);
                }
                else if (checkPackage.Positions.ItemsElementName[i] == ItemsChoiceType.TextString)
                {
                    PrintTextString((TextString)checkPackage.Positions.Items[i]);
                }
                else if (checkPackage.Positions.ItemsElementName[i] == ItemsChoiceType.BarcodeString)
                {
                    PrintBarcodeString((BarcodeString)checkPackage.Positions.Items[i]);
                }
                else
                {
                    Log("Unknown position " + checkPackage.Positions.Items[i].ToString());
                }
            }

            // Оплата
            Log("Payment");
            double Cash = checkPackage.Payments.Cash;
            double CashLessType1 = checkPackage.Payments.CashLessType1;
            double CashLessType2 = checkPackage.Payments.CashLessType2;
            double CashLessType3 = checkPackage.Payments.CashLessType3;

            Log("Cash = " + Cash.ToString() + " CashLessType1 = " + CashLessType1.ToString() + " CashLessType2 = " + CashLessType2.ToString() + " CashLessType3 = " + CashLessType3.ToString());

            double Remainder = 0;
            double Change = 0;
            FR.Payment(Cash, 0, ref Remainder, ref Change);
            FR.Payment(CashLessType1, 1, ref Remainder, ref Change);
            FR.Payment(CashLessType2, 2, ref Remainder, ref Change);
            FR.Payment(CashLessType3, 3, ref Remainder, ref Change);

            Log("CloseCheck");
            Result = FR.CloseCheck();
            Log("Result = " + Result);

            string ResultDescription = FR.ResultDescription();
            if (Result != 0)
            {
                Log("Error: " + Result.ToString() + " - " + ResultDescription);
                FR.PrintString("Error: " + Result.ToString() + " - " + ResultDescription);
                Log("Abort");
                FR.Abort();
            }

            Log("Stop");
            FR.Stop();
        }

        static void PrintXML(string xml)
        {
            CheckPackage checkPackage = DeserializeXMLFileToObject<CheckPackage>(GenerateStreamFromString(xml));
            if (checkPackage == null)
            {
                FR.Start();
                if (xml.Trim().StartsWith("#PrintXReport"))
                {
                    FR.PrintXReport();
                    FR.Stop();
                    return;
                }
                else if (xml.Trim().StartsWith("#PrintZReport"))
                {
                    FR.PrintZReport();
                    FR.Stop();
                    return;
                }
                else
                {
                    //FR.PrintString("Error XML parse!");
                    FR.PrintString(xml);
                    FR.Feed(6);
                    FR.PartialCut();
                    FR.Stop();
                    return;
                }
            }
            else
            {
                //Log("checkPackage.Positions.Count = " + checkPackage.Positions.Count.ToString());
                ProcessCheck(checkPackage);
            }
        }

        static string Encode(string str)
        {
            return Encode(str, 866, 1251);
        }

        static string Encode(string str, string codePageIn, string codePageOut)
        {
            var bytes = Encoding.GetEncoding(codePageIn).GetBytes(str);
            var res = Encoding.GetEncoding(codePageOut).GetString(bytes);
            return res;
        }

        static string Encode(string str, int codePageIn, int codePageOut)
        {
            var bytes = Encoding.GetEncoding(codePageIn).GetBytes(str);
            var res = Encoding.GetEncoding(codePageOut).GetString(bytes);
            return res;
        }

        static string ListToString(List<String> Lines)
        {
            string s = "";
            foreach (string line in Lines)
            {
                s = s + line;
            }
            return s;
        }

        static void Main(string[] args)
        {
            List<String> Lines = new List<String>();

            string line;
            do
            {
                line = Console.ReadLine();
                if (line != null)
                {
                    Lines.Add(Encode(line));
                }
            } while (line != null);

            FR.Init();

            //string path = "" + AppDomain.CurrentDomain.BaseDirectory + "testCheck.xml";
            //string testXML = File.ReadAllText(path, Encoding.GetEncoding(1251));
            //Console.WriteLine(testXML);
            //PrintXML(testXML);
            //Console.ReadLine();

            PrintXML(ListToString(Lines));

        }
    }
}
