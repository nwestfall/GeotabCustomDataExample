using System;
using System.Text;
using System.Text.RegularExpressions;

namespace geotabcustomdata
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Geotab CustomData test!");

            var data1 = new byte[] { 48, 52, 56, 55, 57, 55, 48, 65, 66 };
            var data1false = GetUserId(data1, false);
            var data1true = GetUserId(data1, true);
            Console.WriteLine($"Data1 (F): {data1false}");
            Console.WriteLine($"Data1 (T): {data1true}");

            var data2 = new byte[] { 55, 52, 67, 56, 48, 0, 0, 0, 0 };
            var data2false = GetUserId(data2, false);
            var data2true = GetUserId(data2, true);
            Console.WriteLine($"Data2 (F): {data2false}");
            Console.WriteLine($"Data2 (T): {data2true}");

            var data3 = new byte[] { 48, 52, 56, 55, 57, 55, 48, 65, 66, 55, 52, 67, 56, 48, 0, 0, 0, 0 };
            var data3false = GetUserId(data3, false);
            var data3true = GetUserId(data3, true);
            Console.WriteLine($"Data3 (F): {data3false}");
            Console.WriteLine($"Data3 (T): {data3true}");
        }

        static string GetUserId(byte[] data, bool convertRFID)
        {
            int length = data.Length;
            int employeeID;
            int facilityID;
            string rfid = null;
            try
            {
                if ((length == 14 || length == 16) && convertRFID)
                {
                    ParseRFIDData14Bytes(data, length, out employeeID, out facilityID);
                    rfid = employeeID.ToString();
                }
                else if ((length == 18) && convertRFID)
                {
                    ParseRFIDData18Bytes(data, out employeeID, out facilityID);
                    rfid = employeeID.ToString();
                }
                else
                {
                    // don't convert RFID
                    ASCIIEncoding ascii = new ASCIIEncoding();
                    rfid = Regex.Replace(ascii.GetString(data), @"[^\w\.@-]", "");
                }
            }
            catch (Exception ex)
            {
            }
            return rfid;
        }

        static void ParseRFIDData18Bytes(byte[] data, out int employeeID, out int facilityID)
        {
            StringBuilder stringBuilder = new StringBuilder(4);
            string debugString = "";
            for (int i = 14; i < 18; i++)
            {
                string str = Convert.ToString(data[i], 2);
                stringBuilder.Append(str.PadLeft(8, '0'));
                debugString = str;
            }
            string binaryString = stringBuilder.ToString();
            binaryString = binaryString.Remove(0, 6);
            // even parity check
            int count = 0;
            for (int i = 0; i < 13; i++)
            {
                if (binaryString[i] == '1')
                {
                    count++;
                }
            }
            if (count % 2 == 1)
            {
                throw new Exception("Even Parity check failed.");
            }
            // odd parity check
            count = 0;
            for (int i = 13; i < 26; i++)
            {
                if (binaryString[i] == '1')
                {
                    count++;
                }
            }
            if (count % 2 == 0)
            {

                throw new Exception(String.Format("Odd Parity check failed. debugString = {0}  binaryString = {1}", debugString, binaryString));
            }

            facilityID = Convert.ToInt32(binaryString.Substring(1, 8), 2);
            employeeID = Convert.ToInt32(binaryString.Substring(9, 16), 2);
        }

        /// <summary>
        /// Parse RFID data which has 14 or 16 bytes.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        /// <param name="employeeID"></param>
        /// <param name="facilityID"></param>
        static void ParseRFIDData14Bytes(byte[] data, int length, out int employeeID, out int facilityID)
        {
            StringBuilder stringBuilder = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                stringBuilder.Append(Convert.ToChar(data[i]));
            }
            stringBuilder = stringBuilder.Remove(0, 3);
            // The data can be either 14 bytes or 16 bytes. We need to chop off the last 2 bytes if it is 14 bytes, and last 4 bytes if it is 16 bytes.
            int bytesToRemove = length == 16 ? 4 : 2;
            stringBuilder = stringBuilder.Remove(stringBuilder.Length - bytesToRemove, bytesToRemove);
            string binaryString = GetBinaryBits(stringBuilder);
            GetID(binaryString, out employeeID, out facilityID);
        }

        static void GetID(string binaryString, out int employeeID, out int facilityID)
        {
            string facilityIDBinary = binaryString.Substring(0, 12);
            string employeeIDBinary = binaryString.Substring(12, 20);
            employeeID = Convert.ToInt32(employeeIDBinary, 2);
            facilityID = Convert.ToInt32(facilityIDBinary, 2);
        }

        static string GetBinaryBits(StringBuilder asciiStringBuilder)
        {
            StringBuilder binaryResults = new StringBuilder();
            for (int i = 0; i < asciiStringBuilder.Length; i++)
            {
                binaryResults.Append(GetBinaryBits(asciiStringBuilder[i]));
            }
            string binaryString = binaryResults.ToString();
            binaryString = binaryString.Remove(0, 3);
            binaryString = binaryString.Remove(binaryString.Length - 1, 1);
            if (binaryString.Length != 32)
            {
                throw new Exception();
            }

            return binaryString;
        }

        static string GetBinaryBits(char ch)
        {
            if (ch >= '0' && ch <= 'F')
            {
                string binaryString = Convert.ToString(Convert.ToByte(ch.ToString(), 16), 2);
                return binaryString.PadLeft(4, '0');
            }
            return null;
        }
    }
}
