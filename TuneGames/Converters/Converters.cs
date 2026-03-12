using System.Globalization;
using TuneGames.Pages.Results;

namespace TuneGames.Converters;

public class StringNotEmptyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => !string.IsNullOrEmpty(value as string);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class InvertBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : false;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : false;
}

public class CheckMarkConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? "☑" : "☐";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class SelectedBorderColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true
            ? (Application.Current!.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#7C4DFF") : Color.FromArgb("#512BD4"))
            : (Application.Current!.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#444") : Color.FromArgb("#E0E0E0"));

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class SelectedBackgroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true
            ? (Application.Current!.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#1A7C4DFF") : Color.FromArgb("#1A512BD4"))
            : (Application.Current!.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#2A2A2A") : Color.FromArgb("#FAFAFA"));

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class ResultStatusEmojiConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            ResultStatus.Correct => "✅",
            ResultStatus.Missed => "❌",
            ResultStatus.Wrong => "⚠️",
            _ => "⬜"
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
