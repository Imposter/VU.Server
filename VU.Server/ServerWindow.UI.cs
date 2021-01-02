using System.Collections.Generic;
using Terminal.Gui;

namespace VU.Server
{
    internal sealed partial class ServerWindow : Window
    {
        public const int LOG_LIMIT = 256;

        private ListView _logsListView;
        private TextField _inputTextField;
        private List<string> _logList;
        private int _logSelectedIndex;
        private bool _logAutoScrollEnabled;

        public void CreateUserInterface()
        {
            // Create UI elements
            _logList = new List<string>();
            _logsListView = new ListView { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill(1) };
            _logsListView.SelectedItemChanged += LogsListView_SelectedItemChanged;
            _logsListView.SetSource(_logList);
            _logSelectedIndex = 0;
            _logAutoScrollEnabled = true;

            _inputTextField = new TextField { X = 0, Y = Pos.AnchorEnd(1), Width = Dim.Fill(), Height = 1 };
            _inputTextField.EnsureFocus();

            Add(_logsListView);
            Add(_inputTextField);
        }

        private void LogsListView_SelectedItemChanged(ListViewItemEventArgs args)
        {
            _logSelectedIndex = args.Item;
            if (args.Item != _logList.Count - 1 || _logList.Count == 1)
                _logAutoScrollEnabled = false;
            else
                _logAutoScrollEnabled = true;
        }

        private void WriteToLog(string line)
        {
            Application.MainLoop.AddIdle(() =>
            {
                _logList.Add(line);
                if (_logList.Count > LOG_LIMIT)
                    _logList.RemoveAt(0);

                if (_logList.Count > 0)
                {
                    // Select the last selected item
                    if (_logAutoScrollEnabled)
                        _logsListView.SelectedItem = _logList.Count - 1;
                    else
                    {
                        if (_logSelectedIndex > _logList.Count - 1)
                            _logSelectedIndex = _logList.Count - 1;

                        _logsListView.SelectedItem = _logSelectedIndex;
                    }
                }

                Application.Refresh();

                return false; // This idle function is no longer needed and can be removed
            });
        }
    }
}
