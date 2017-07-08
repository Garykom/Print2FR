using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Print2FR
{
    [XmlRoot("CheckPackage")]
    public class CheckPackage
    {
        [XmlElement("Parameters")]
        public Parameters Parameters { get; set; }

        [XmlElement("Positions")]
        public Positions Positions { get; set; }

        [XmlElement("Payments")]
        public Payments Payments { get; set; }

    }

    public class Parameters
    {
        [XmlAttribute("PaymentType")]
        public long PaymentType { get; set; }

        [XmlAttribute("TaxVariant")]
        public long TaxVariant { get; set; }

        [XmlAttribute("CustomerEmail")]
        public string CustomerEmail { get; set; }

        [XmlAttribute("SenderEmail")]
        public string SenderEmail { get; set; }

        [XmlAttribute("CustomerPhone")]
        public string CustomerPhone { get; set; }

        [XmlAttribute("AgentCompensation")]
        public string AgentCompensation { get; set; }

        [XmlAttribute("AgentPhone")]
        public string AgentPhone { get; set; }


        [XmlAttribute("SubagentPhone")]
        public string SubagentPhone { get; set; }


    }

    [XmlType("Positions")]
    public class Positions
    {
        [XmlElement("FiscalString", typeof(FiscalString), IsNullable = false)]
        [XmlElement("TextString", typeof(TextString), IsNullable = false)]
        [XmlElement("BarcodeString", typeof(BarcodeString), IsNullable = false)]
        [XmlChoiceIdentifier("ItemsElementName")]
        public Object[] Items;

        [XmlElement(IsNullable = false)]
        [XmlIgnore()]
        public ItemsChoiceType[] ItemsElementName;
    }

    public enum ItemsChoiceType
    {
        FiscalString,
        TextString,
        BarcodeString,
    }

    [XmlType("FiscalString")]
    public class FiscalString
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Quantity")]
        public double Quantity { get; set; }

        [XmlAttribute("Price")]
        public double Price { get; set; }

        [XmlAttribute("Amount")]
        public double Amount { get; set; }

        [XmlAttribute("Department")]
        public long Department { get; set; }

        [XmlAttribute("Tax")]
        public string Tax { get; set; }
    }

    [XmlType("TextString")]
    public class TextString
    {
        [XmlAttribute("Text")]
        public string Text { get; set; }
    }

    [XmlType("BarcodeString")]
    public class BarcodeString
    {
        [XmlAttribute("BarcodeType")]
        public string BarcodeType { get; set; }

        [XmlAttribute("Barcode")]
        public string Barcode { get; set; }
    }

    public class Payments
    {
        [XmlAttribute("Cash")]
        public double Cash { get; set; }

        [XmlAttribute("CashLessType1")]
        public double CashLessType1 { get; set; }

        [XmlAttribute("CashLessType2")]
        public double CashLessType2 { get; set; }

        [XmlAttribute("CashLessType3")]
        public double CashLessType3 { get; set; }
    }

}
