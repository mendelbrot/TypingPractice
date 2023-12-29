using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;

namespace TypingPractice;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private string? _filePath;
    private string? _fileContent;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void OpenFileButton_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        if (openFileDialog.ShowDialog() == true)
        {
            _filePath = openFileDialog.FileName;
            LoadFileContent();
            DisplayFileContent();
        }
    }

    private void LoadFileContent()
    {
        try
        {
            #pragma warning disable CS8604 // Possible null reference
            _fileContent = File.ReadAllText(_filePath);
            #pragma warning restore CS8604
        }
        catch (Exception ex) 
        {
            MessageBox.Show($"Error reading file: {ex.Message}");
            _fileContent = null;
        }
    }

    private void DisplayFileContent()
    {
        textContent.Text = _fileContent;
    }
}