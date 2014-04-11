using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using AudioRecorder.Audio;
using System.IO;
using System.IO.IsolatedStorage;

namespace AudioRecorder
{
    public class SimpleCommand : ICommand
    {
        public Action ExecuteAction { get; set; }

        private bool _canExecute;
        public bool MayBeExecuted
        {
            get { return _canExecute; }
            set
            {
                if (_canExecute!=value)
                {
                    _canExecute=value;
                    if (CanExecuteChanged!=null)
                        CanExecuteChanged(this, new EventArgs());
                }
            }
        }

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            return MayBeExecuted;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (ExecuteAction!=null)
                ExecuteAction();
        }

        #endregion
    }

    public class RecorderViewModel : DependencyObject
    {
        private SimpleCommand _recordCommand;
        private SimpleCommand _playPauseCommand;
        private SimpleCommand _saveCommand;

        private MemoryAudioSink _sink;
        private CaptureSource _captureSource;

        public SimpleCommand RecordCommand { get { return _recordCommand; } }
        public SimpleCommand PlayPauseCommand { get { return _playPauseCommand; } }
        public SimpleCommand SaveCommand { get { return _saveCommand; } }

        public static readonly DependencyProperty StatusTextProperty=DependencyProperty.Register("StatusText", typeof(string), typeof(RecorderViewModel), null);
        public string StatusText
        {
            get
            {
                return (string)GetValue(StatusTextProperty);
            }
            set
            {
                SetValue(StatusTextProperty, value);
            }
        }

        public RecorderViewModel()
        {
            _recordCommand=new SimpleCommand()
            {
                MayBeExecuted=true,
                ExecuteAction=() => Record()
            };

            _saveCommand=new SimpleCommand()
            {
                MayBeExecuted=false,
                ExecuteAction=() => SaveFile()
            };

            _playPauseCommand=new SimpleCommand()
            {
                MayBeExecuted=false,
                ExecuteAction=() => PlayOrPause()
            };


            var audioDevice=CaptureDeviceConfiguration.GetDefaultAudioCaptureDevice();
            _captureSource=new CaptureSource() { AudioCaptureDevice=audioDevice };

            GoToStartState();
        }

        private bool EnsureAudioAccess()
        {
            return CaptureDeviceConfiguration.AllowedDeviceAccess||CaptureDeviceConfiguration.RequestDeviceAccess();
        }

        protected void GoToStartState()
        {
            StatusText="Pronto para gravação...";
            _saveCommand.MayBeExecuted=false;
            _recordCommand.MayBeExecuted=true;
            _playPauseCommand.MayBeExecuted=false;
        }

        protected void Record()
        {
            if (!EnsureAudioAccess())
                return;

            if (_captureSource.State!=CaptureState.Stopped)
                return;

            _sink=new MemoryAudioSink();
            _sink.CaptureSource=_captureSource;
            _captureSource.Start();

            // Enable pause command, disable record command
            _playPauseCommand.MayBeExecuted=true;
            _recordCommand.MayBeExecuted=false;
            StatusText="Gravando...";
        }

        protected void PlayOrPause()
        {
            if (_captureSource.State==CaptureState.Started)
            {
                _captureSource.Stop();

                // Disable pause command, enable save command
                _playPauseCommand.MayBeExecuted=false;
                _saveCommand.MayBeExecuted=true;
                StatusText="Gravação concluída. Você pode salvar o seu registro.";
            }
        }

        public void SaveFile()
        {
 
        }

        public void EscreverArquivo(string nomArquivo)
        {
            if (Application.Current.HasElevatedPermissions)
            {
                StatusText="Saving...";
                FileInfo fileInfo=new FileInfo(nomArquivo);
                string filename=fileInfo.Name;
                //Criando o Arquivo
                using (IsolatedStorageFile store=IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!store.FileExists(filename))
                    {
                        using (IsolatedStorageFileStream stream=new IsolatedStorageFileStream(filename, FileMode.Create, store))
                            stream.Close();
                    }
                    //Lendo o Arquivo
                    Stream stream1=new IsolatedStorageFileStream(filename, FileMode.Open, store);
                    WavManager.SavePcmToWav(_sink.BackingStream, stream1, _sink.CurrentFormat);
                    stream1.Close();
                }
                GoToStartState();
            }  
        }
    }
}
