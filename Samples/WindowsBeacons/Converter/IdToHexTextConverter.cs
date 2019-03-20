// Copyright 2015 - 2019 Andreas Jakl, Chris Tacke and Contributors. All rights reserved. 
// https://github.com/andijakl/universal-beacon 
// 
// This code is licensed under the MIT License.
// See the LICENSE file in the project root for more information.

using System;
using System.Numerics;
using Windows.UI.Xaml.Data;

namespace WindowsBeacons.Converter
{
    public class IdToHexTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is byte[])
            {
                return BitConverter.ToString((byte[])value);
            }
            if (value is ulong)
            {
                return ((ulong) value).ToString("X");
            }
            if (value is BigInteger)
            {
                return ((BigInteger) value).ToString("X");
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
