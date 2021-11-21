using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;

namespace Jump
{
    public partial class JumpDialogWindow : DialogWindow, IDisposable
    {
        private readonly List<SolutionItem> _files;
        private string _fileName;
        private int _lineNumber;
        public JumpDialogWindow()
        {
            InitializeComponent();
            _files = new List<SolutionItem>();
        }
        private async void OnJumpTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var parsed = TryParseJumpTextBoxInput();
                if (parsed)
                {
                    await OpenFileAsync();
                    return;
                }
                await VS.MessageBox.ShowWarningAsync("Warning", "Invalid input");
            }
        }
        private bool TryParseJumpTextBoxInput()
        {
            string[] arr = JumpTextBox.Text.Split('@').ToArray();
            if (arr.Length <= 1) return false;
            _fileName = arr[0];
            _lineNumber = int.Parse(arr[1]);

            return true;
        }
        private async Task OpenFileAsync()
        {
            try
            {
                var projects = await VS.Solutions.GetAllProjectsAsync();
                foreach (var project in projects)
                {
                    var children = project.Children.ToList();
                    FindFiles(children);
                }
                if (_files.Count == 1)
                {
                    var file = _files.First();
                    var doc = await VS.Documents.OpenAsync(file.Name);

                    var line = doc.TextView.TextSnapshot.GetLineFromLineNumber(_lineNumber - 1);
                    var point = new SnapshotPoint(line.Snapshot, line.End.Position);
                    doc.TextView.VisualElement.Focus();
                    doc.TextView.Caret.MoveTo(point);
                    doc.TextView.Caret.EnsureVisible();
                    Close();
                }
            }
            catch(Exception)
            {
                await VS.MessageBox.ShowErrorAsync("Error", "File not found");
            }
        }
        private void FindFiles(List<SolutionItem> children)
        {
            foreach(var child in children)
            {
                if (child.Type == SolutionItemType.PhysicalFile)
                {
                    if (!_fileName.EndsWith(".cs")) _fileName += ".cs";
                    if (string.Equals(child.Text, _fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        _files.Add(child);
                    }
                }
                if (child.Children == null || child.Children.Count() == 0) continue;
                FindFiles(child.Children.ToList());
            }
        }
        public void Dispose()
        {
        }
    }
}
