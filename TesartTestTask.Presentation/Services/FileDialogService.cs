using Microsoft.Win32;
using TesartTestTask.Presentation.Interfaces;

namespace TesartTestTask.Presentation.Services;

public class FileDialogService : IFileDialogService
{
    public string? ShowSaveCsvDialog(string defaultFileName)
    {
        var saveFileDialog = new SaveFileDialog
        {
            DefaultExt = ".csv",
            FileName = defaultFileName,
            Filter = "CSV files|*.csv",
            Title = "Ёкспорт истории измерений"
        };
        
        return saveFileDialog.ShowDialog() == true ? saveFileDialog.FileName : null;
    }
}