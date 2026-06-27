using Microsoft.Win32;
using TesartTestTask.Presentation.Interfaces;

namespace TesartTestTask.Presentation.Services;

public class FileDialogService : IFileDialogService
{
    public string? ShowSaveCsvDialog(string defaultFileName)
    {
        return new SaveFileDialog
        {
            DefaultExt = ".csv",
            FileName = defaultFileName,
            Filter = "CSV files|*.csv",
            Title = "Ёкспорт истории измерений"
        }.ShowDialog() == true ? defaultFileName : null;
    }
}