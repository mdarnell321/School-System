using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace School_Project
{
    internal class Functions
    {
        public static bool URLExists(string url)
        {
            WebRequest request = WebRequest.Create(url);
            WebResponse response;
            try
            {
                response = request.GetResponse();
            }
            catch // no response
            {
                return false;
            }
            return true;
        }
        public static string PercentToLetter(int percent)
        {
            if (percent == 100)
                return "A+";
            else if (percent >= 93)
                return "A";
            else if (percent >= 90)
                return "A-";
            else if (percent >= 87)
                return "B+";
            else if (percent >= 83)
                return "B";
            else if (percent >= 80)
                return "B-";
            else if (percent >= 77)
                return "C+";
            else if (percent >= 73)
                return "C";
            else if (percent >= 70)
                return "C-";
            else if (percent >= 67)
                return "D+";
            else if (percent >= 63)
                return "D";
            else if (percent >= 60)
                return "D-";
            else
                return "F";
        }
        public static float LetterToGPA(string letter)
        {
            switch (letter)
            {
                case "A+":
                    return 4.00f;
                case "A":
                    return 4.00f;
                case "A-":
                    return 3.70f;
                case "B+":
                    return 3.30f;
                case "B":
                    return 3.00f;
                case "B-":
                    return 2.70f;
                case "C+":
                    return 2.30f;
                case "C":
                    return 2.00f;
                case "C-":
                    return 1.70f;
                case "D+":
                    return 1.30f;
                case "D":
                    return 1.00f;
                case "D-":
                    return 0.70f;
                default:
                    return 0.00f;
            }
            
        }
        public static float Overall_GPA(List<object[]> data)
        {
            float sum_gpa = 0f;
            int sum_credits = 0;
            for(int i =0; i < data.Count; ++i)
            {
                float gpa = (float)data[i][0];
                int hours = (int)data[i][1];

                sum_gpa += gpa * hours;
                sum_credits += hours;
            }
            if(sum_credits == 0)
                return 0f;
            else
                return sum_gpa / sum_credits;
        }
        public static string PHPGet(string url, params string[] data)
        {
            string new_url = url + "?";
            for (int i = 0; i < data.Length; i+=2)
            {
                new_url += data[i] + "=" + data[i+1] + "&";
            }
            Stream stream = WebRequest.Create(new_url).GetResponse().GetResponseStream();
            string content = "";
            using (StreamReader sr = new StreamReader(stream))
            {
                content = sr.ReadToEnd();
            }
            return content;
        }
    }
}
