using System;
using System.Threading;
using NStack;
using Terminal.Gui;

namespace Adapter.Notifier.TerminalGui
{
    public class TerminalGuiAdapter
    {
        private Thread _thread;
        private Label _currentFileLabel;
        private Label _currentGameLabel;
        private Label _statusLabel;
        private Label _currentGamePercentCompleteLabel;

        public void Initialize()
        {
            Application.Init();
            
            var top = Application.Top;
            
            // Creates the top-level window to show
            var win = new Window ("XBox FTP Upload") {
                X = 0,
                Y = 1, // Leave one row for the toplevel menu

                // By using Dim.Fill(), it will automatically resize without manual intervention
                Width = Dim.Fill (),
                Height = Dim.Fill ()
            };
	
            top.Add (win);
            
            // Add some controls, 
            _currentGameLabel = new Label(new Rect(3,4, 40, 1));
            _currentGamePercentCompleteLabel = new Label(new Rect(3,5, 20, 1));
            _currentFileLabel = new Label(new Rect(3,6, 40, 1));
            _statusLabel = new Label(new Rect(3, 15, 40, 1));
            
            win.Add (
                _currentGameLabel,
                _currentGamePercentCompleteLabel,
                _currentFileLabel,
                _statusLabel
            );
            
            _thread = new Thread(() =>
            {
                try
                {
                    Application.Run();
                }
                catch (NullReferenceException ex)
                {
                    // Terminal.Gui throws a null reference exception when being shutdown
                }
                
            });
            
            _thread.Start();
        }

        public TerminalGuiProgressNotifier CreateNotifier()
        {
            return new TerminalGuiProgressNotifier(_currentFileLabel, _currentGameLabel, _statusLabel, _currentGamePercentCompleteLabel);
        }
        
        private bool Quit()
        {
            return false;
        }

        private void Close()
        {
        }

        private void NewFile()
        {
        }

        public void Shutdown()
        {
            Application.RequestStop();
            Application.Shutdown();
            _thread.Join(TimeSpan.FromSeconds(2));
        }
    }
}