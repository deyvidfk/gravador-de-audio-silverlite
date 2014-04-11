using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using AudioRecorder.Audio;
using System.IO;
using System.IO.IsolatedStorage;


namespace AudioRecorder
{
    public partial class MainPage : UserControl
    {
        RecorderViewModel ViewModel;
        WebClient Appclient=new WebClient();
        public const int CHUNK_SIZE=4096^10;
        public const string UPLOAD_URI="http://localhost:2567/Upload.ashx?filename={0}";
        private Stream _data;
        private string _fileName;

        public MainPage()
        {
            InitializeComponent();
            ViewModel=new RecorderViewModel();
            ViewModel.PlayPauseCommand.CanExecuteChanged+=new EventHandler(PlayPauseCommand_CanExecuteChanged);
            DataContext=ViewModel;
        }

        void PlayPauseCommand_CanExecuteChanged(object sender, EventArgs e)
        {
            tapeMeter.MeterMode=((ICommand)sender).CanExecute(null)?Codeplex.Dashboarding.Odometer.Mode.AutoIncrement:Codeplex.Dashboarding.Odometer.Mode.Static;
        }

        #region Upload
        private void buttonOpenWriteAsync_Click(object sender, RoutedEventArgs e)
        {
            _fileName=String.Concat(String.Concat("audio-", Guid.NewGuid(), ".wav"));
            ViewModel.EscreverArquivo(_fileName);

            //Lendo o Arquivo      
            using (IsolatedStorageFile store1=IsolatedStorageFile.GetUserStoreForApplication())
            {
                string uploadUri=String.Format(UPLOAD_URI, _fileName); // Dont't appen
                if (store1.FileExists(_fileName))
                {
                    _data=new IsolatedStorageFileStream(_fileName, FileMode.Open, store1);
                    Uri URL=new Uri(uploadUri, UriKind.RelativeOrAbsolute);
                    Appclient.OpenWriteAsync(URL, "POST", _data);
                    Appclient.OpenWriteCompleted+=new OpenWriteCompletedEventHandler(Appclient_OpenWriteCompleted);
                }
            }
        }
        private void Appclient_OpenWriteCompleted(object sender, OpenWriteCompletedEventArgs e)
        {
            //Enviar o fluxo do arquivo para servidor
            Stream inputStream=_data;
            Stream outputStream=e.Result;
            byte[] buffer=new byte[4096];
            int bytesRead=0;
            while ((bytesRead=inputStream.Read(buffer, 0, buffer.Length))>0)
            {
                outputStream.Write(buffer, 0, bytesRead);
            }
            outputStream.Close();
            inputStream.Close();
            MessageBox.Show("Arquivo transferido com sucesso!");
            Appclient.OpenWriteCompleted-=new OpenWriteCompletedEventHandler(Appclient_OpenWriteCompleted);
        }
        #endregion
    }


}
