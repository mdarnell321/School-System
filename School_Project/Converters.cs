using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Provider;
using System.Windows.Data;

namespace School_Project
{
   
    public class GetModuleType : IValueConverter
    {
        public object Convert(object value, Type targettype, object param, System.Globalization.CultureInfo culture)
        {
            return ((string[])value)[3] == "1" ? "(Exam) " : (((string[])value)[5] == "1" ? "(Assignment) ": "");
        }

        public object ConvertBack(object value, Type targettype, object param, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class GetDueDate : IValueConverter
    {
        public object Convert(object value, Type targettype, object param, System.Globalization.CultureInfo culture)
        {
            return ((string[])value)[3] == "1" ? string.Format("Due: {0} ", ((string[])value)[7]) : (((string[])value)[5] == "1" ? string.Format("Due: {0} ", ((string[])value)[8]) : "");
        }

        public object ConvertBack(object value, Type targettype, object param, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class GetEffectiveGrade : IValueConverter
    {
        public object Convert(object value, Type targettype, object param, System.Globalization.CultureInfo culture)
        {
            return ((string[])value)[4] != "" ? ((string[])value)[4] : (((string[])value)[6] != "" ? ((string[])value)[6] : "");
        }

        public object ConvertBack(object value, Type targettype, object param, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class PercentToLetterGrade : IValueConverter
    {
        public object Convert(object value, Type targettype, object param, System.Globalization.CultureInfo culture)
        {
            if(value.GetType() == typeof(string[]))
            {
                value = ((string[])value)[3];
                return Functions.PercentToLetter((int.Parse((string)value)));

            }
            return "";

        }

        public object ConvertBack(object value, Type targettype, object param, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class TeachToVisibility : IValueConverter
    {
        public object Convert(object value, Type targettype, object param, System.Globalization.CultureInfo culture)
        {
            if(((bool)value) == true)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targettype, object param, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
