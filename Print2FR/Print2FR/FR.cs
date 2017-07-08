using System;
using System.Collections.Generic;
using System.Text;

namespace Print2FR
{
    public static class FR
    {
        static private FprnM1C.IFprnM45 ECR;

        public static void Init()
        {
            try
            {
                ECR = new FprnM1C.FprnM45Class();
            }
            catch (Exception)
            {
                //MessageBox.Show("Не удалось создать объект общего драйвера ККМ!", Application.ProductName,
                //    MessageBoxButtons.OK,
                //    MessageBoxIcon.Error);
                //Close();
                return;
            }
        }

        public static void Abort()
        {
            CancelCheck();
            Stop();
        }

        public static void Start()
        {
            // занимаем порт
            ECR.DeviceEnabled = true;
            if (ECR.ResultCode != 0)
                return;

            // получаем состояние ККМ
            if (ECR.GetStatus() != 0)
                return;

            // проверяем на всякий случай ККМ на фискализированность
            //if (ECR.Fiscal)
            //    //if (MessageBox.Show("ККМ фискализирована! Вы действительно хотите продолжить?",
            //    //        Application.ProductName,
            //    //        System.Windows.Forms.MessageBoxButtons.YesNo,
            //    //        System.Windows.Forms.MessageBoxIcon.Question) == DialogResult.No)
            //    return;

            CancelCheck();
        }

        public static int SetMode(int Mode, string Password)
        {
            ECR.Password = Password;
            ECR.Mode = Mode;
            return ECR.SetMode();
        }

        public static void CancelCheck()
        {
            // если есть открытый чек, то отменяем его
            if (ECR.CheckState != 0)
                if (ECR.CancelCheck() != 0)
                    return;
        }

        public static void Stop()
        {
            // выходим в режим выбора, чтобы кто-то под введенными паролями не сделал что нибуть нехорошее
            if (ECR.ResetMode() != 0)
                return;

            // освобождаем порт
            ECR.DeviceEnabled = false;
            if (ECR.ResultCode != 0)
                return;
        }

        public static int OpenCheck(int CheckMode, int CheckType)
        {
            ECR.CheckMode = CheckMode;
            ECR.CheckType = CheckType;

            return ECR.OpenCheck();
        }

        public static void Registration(string Name, double Price, double Quantity, int TaxTypeNumber, 
            int Department = 1, int DiscountType = 0, double DiscountValue = 0)
        {
            ECR.Name = Name;
            ECR.Price = Price;
            ECR.Quantity = Quantity;
            //ECR

            ECR.Destination = 1;
            ECR.TaxTypeNumber = TaxTypeNumber;
            //ECR.Summ = (400 * 18) / 100;
            //ECR.SummTax();

            ECR.Department = Department;
            ECR.DiscountType = DiscountType;
            ECR.DiscountValue = DiscountValue;

            if (ECR.Registration() != 0)
                return;
        }

        public static void PrintString(string str)
        {
            ECR.Caption = str;
            ECR.TextWrap = 1;
            ECR.PrintString();
        }

        public static void Payment(double Summ, int TypeClose, ref double Remainder, ref double Change)
        {
            ECR.Summ = Summ;
            ECR.TypeClose = TypeClose;
            ECR.Payment();

            Remainder = ECR.Remainder;
            Change = ECR.Change;
        }

        public static void WriteAttribute(int AttrNumber, string AttrValue)
        {
            ECR.AttrNumber = AttrNumber;
            ECR.AttrValue = AttrValue;
            ECR.WriteAttribute();
        }

        public static int CloseCheck()
        {
            int Result = ECR.CloseCheck();
            return Result;
        }

        public static void Feed(int numLines)
        {
            for (int i = 0; i < numLines; i++)
            {
                ECR.Caption = " ";
                ECR.PrintString();
            }
        }

        public static void FullCut()
        {
            ECR.FullCut();
        }

        public static void PartialCut()
        {
            ECR.PartialCut();
        }

        public static void PrintXReport()
        {
            ECR.Password = "30";
            ECR.Mode = 2;
            if (ECR.SetMode() != 0)
                return;

            ECR.ReportType = 2;
            ECR.Report();
        }

        public static void PrintZReport()
        {
            ECR.Password = "30";
            ECR.Mode = 3;
            if (ECR.SetMode() != 0)
                return;

            ECR.ReportType = 1;
            ECR.Report();
        }

        public static int ResultCode()
        {
            return ECR.ResultCode;
        }

        public static string ResultDescription()
        {
            return ECR.ResultDescription;
        }

        public static void GetLastError(ref int Error, ref string ErrorDescription)
        {
            ECR.GetLastError();
            Error = ECR.ECRError;
            ErrorDescription = ECR.ECRErrorDescription;
        }

        public static void Print(List<String> Lines)
        {
            ECR.Caption = "Начало проверки";
            ECR.TextWrap = 1;
            ECR.PrintString();

            foreach (String line in Lines)
            {
                //ECR.Caption = line.ToString();
                //ECR.TextWrap = 1;
                //ECR.PrintString();
            };
        }
    }
}
