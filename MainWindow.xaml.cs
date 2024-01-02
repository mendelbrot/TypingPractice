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
    private string? filePath;
    private string? fileContent;
    private int currentPosition = 0;
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OpenFileButton_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        if (openFileDialog.ShowDialog() == true)
        {
            filePath = openFileDialog.FileName;
            LoadFileContent();
            InitializeTypingText();
        }
    }

    private void LoadFileContent()
    {
        try
        {
            #pragma warning disable CS8604 // Possible null reference
            fileContent = File.ReadAllText(filePath);
            #pragma warning restore CS8604
        }
        catch (Exception ex) 
        {
            MessageBox.Show($"Error reading file: {ex.Message}");
            fileContent = null;
        }
    }

    private void InitializeTypingText()
    {
        typingArea.Document.Blocks.Clear();
        typingArea.Document.Blocks.Add(new Paragraph(new Run(fileContent)));
        // UpdateTextFormatting();
    }

    private void TypingArea_TextInput(object sender, TextCompositionEventArgs e)
    {
        currentPosition++;
        UpdateTextFormatting();
        e.Handled = true;
        // if (e.Text.Length == 1)
        // {
        //     char nextChar = fileContent[currentPosition];
        //     char typedChar = e.Text[0];
        //     // Check if the pressed key matches the next character
        //     if (typedChar == nextChar)
        //     {
        //         currentPosition++;
        //         UpdateTextFormatting();
        //         e.Handled = true; // Mark the event as handled
        //     }
        // }  
    }

    private void UpdateTextFormatting()
    {
        TextRange previousChar = new TextRange(
            GetPositionAtOffset(
                typingArea.Document.Blocks.FirstBlock.Inlines,
                currentPosition,
                typingArea.Document.ContentStart),
            GetPositionAtOffset(
                typingArea.Document.Blocks.FirstBlock.Inlines,
                currentPosition + 1,
                typingArea.Document.ContentStart));

        previousChar.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.PaleGreen);
        // Dispatcher.Invoke(() =>
        // {
        // Clear existing formatting if necessary
        // TextRange entireText = new TextRange(
        //     typingArea.Document.ContentStart, 
        //     typingArea.Document.ContentEnd);
        // entireText.ClearAllProperties();

        // Logic to update the text formatting in the RichTextBox
        // Text up to upToPosition should be black, and the rest should be grey
        // TextRange beforeCursor = new TextRange(
        //     typingArea.Document.ContentStart,
        //     typingArea.Document.ContentStart.GetPositionAtOffset(currentPosition + 3));

        // TextRange afterCursor = new TextRange(
        //     typingArea.Document.ContentStart.GetPositionAtOffset(currentPosition + 3),
        //     typingArea.Document.ContentEnd);

        // beforeCursor.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
        // afterCursor.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.DarkGray);

        // Force redraw if necessary
        // typingArea.Width = typingArea.ActualWidth - 1;
        // typingArea.Width = typingArea.ActualWidth;
        // });
    }

    /// <summary>
    /// Returns the position of the specified offset in the text specified by the inlines.
    /// https://stackoverflow.com/questions/10212662/textrange-getpositionatoffset-not-behaving-as-expected
    /// </summary>
    /// <param name="inlines">The inlines which specifies the text.</param>
    /// <param name="offset">The offset within the text to get the position of.</param>
    /// <param name="contentStartPosition">The position where the content starts. If null, the position before the start of the first inline will be used. If null and there are no inlines, an exception is thrown.</param>
    /// <returns>A <see cref="TextPointer"/> indicating the position of the specified offset.</returns>
    private TextPointer GetPositionAtOffset(InlineCollection inlines, int offset, TextPointer contentStartPosition = null)
    {
        if (inlines == null)
            throw new ArgumentNullException(nameof(inlines));
        if (!inlines.Any() && contentStartPosition == null)//if no inlines, can't determine start of content
            throw new ArgumentException("A content start position has to be specified if the inlines collection is empty.", nameof(contentStartPosition));

        if (contentStartPosition == null)
            contentStartPosition = inlines.First().ContentStart.DocumentStart;//if no content start specified, gets it
        int offsetWithInlineBorders = 0;//collects the value of offset (with inline borders)
        foreach (var inline in inlines)
        {
            int inlineLength = (inline as Run)?.Text.Length ?? (inline is LineBreak ? 1 : 0);//gets the length of the inline (length of a Run is the lengts of its text, length of a LineBreak is 1, other types are ignored)

            if (inlineLength < offset)//if position specified by the offset is beyond this inline...
                offsetWithInlineBorders += inlineLength + 2;//...then the whole length is added with the two borders
            else if (inlineLength == offset)//if position specified by the offset is at the end of this inline...
                offsetWithInlineBorders += inlineLength + 1;//...then the whole length is added with only the opening border
            else //inlineLength > value, if the position specified by the offset is within this inline
            {
                offsetWithInlineBorders += offset + 1;//...then adds the remaining length (the offset itself), plus the opening border
                break;//the inlines beyond are not needed
            }
            offset -= inlineLength;//substracts the added inline length
        }

        return contentStartPosition.GetPositionAtOffset(
            Math.Min(Math.Max(offsetWithInlineBorders, 0), contentStartPosition.GetOffsetToPosition(contentStartPosition.DocumentEnd)));//if the value is not within the boundaries of the text, returns the start or the end of the text
    }
}