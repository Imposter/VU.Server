using System.Collections.Generic;
using Terminal.Gui;

namespace VU.Server
{
    internal sealed partial class ServerWindow : Window
    {
        public const int LOG_LIMIT = 256;

        // Server Info
        private FrameView _serverInfoFrameView;
        private Label _upTimeLabel;
        private Label _fpsLabel;
        private Label _stateLabel;
        private Label _playersLabel;
        private Label _mapLabel;
        private Label _modeLabel;

        // Logging/Input
        private ListView _logsListView;
        private TextField _inputTextField;
        private List<string> _logList;
        private int _logSelectedIndex;
        private bool _logAutoScrollEnabled;

        public void CreateUserInterface()
        {
            // Create server info labels
            _serverInfoFrameView = new FrameView { Width = Dim.Fill(), Height = 8 };
            _upTimeLabel = new Label { Y = 0, Width = Dim.Fill(), Height = 1 };
            _fpsLabel = new Label { Y = 1, Width = Dim.Fill(), Height = 1 };
            _stateLabel = new Label { Y = 2, Width = Dim.Fill(), Height = 1 };
            _playersLabel = new Label { Y = 3, Width = Dim.Fill(), Height = 1 };
            _mapLabel = new Label { Y = 4, Width = Dim.Fill(), Height = 1 };
            _modeLabel = new Label { Y = 5, Width = Dim.Fill(), Height = 1 };

            _serverInfoFrameView.Add(_upTimeLabel);
            _serverInfoFrameView.Add(_fpsLabel);
            _serverInfoFrameView.Add(_stateLabel);
            _serverInfoFrameView.Add(_playersLabel);
            _serverInfoFrameView.Add(_mapLabel);
            _serverInfoFrameView.Add(_modeLabel);

            // Create logging/input elements
            _logList = new List<string>();
            _logsListView = new ListView { X = 0, Y = 8, Width = Dim.Fill(), Height = Dim.Fill(1) };
            _logsListView.SelectedItemChanged += LogsListView_SelectedItemChanged;
            _logsListView.SetSource(_logList);
            _logSelectedIndex = 0;
            _logAutoScrollEnabled = true;

            _inputTextField = new TextField { X = 0, Y = Pos.AnchorEnd(1), Width = Dim.Fill(), Height = 1 };
            _inputTextField.EnsureFocus();

            Add(_serverInfoFrameView);
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
