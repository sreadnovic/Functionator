﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Functionator.Common.Converters;

internal class BooleanToVisibilityInvertedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (bool)value == false ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}