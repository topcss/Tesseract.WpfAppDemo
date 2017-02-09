using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Tesseract
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private static SynchronizationContext _uiContext;
        SynchronizationContext _dataContext;
        private void ThreadSafePost()
        {
            _dataContext = new SynchronizationContext();
        }

        public ICommand OpenImageCommand { get; set; }
        public ICommand BeginOcrCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindowViewModel()
        {
            _uiContext = SynchronizationContext.Current;

            var dataThread = new Thread(ThreadSafePost);
            dataThread.Start();

            OpenImageCommand = new RelayCommand(OpenImages);
            BeginOcrCommand = new RelayCommand((obj) =>
            {
                if (!File.Exists(CurrentImage?.FilePath))
                {
                    RecognizedText = "请先选择图片！";
                    return;
                }

                RecognizedText = "正在处理，请稍候。。。";
                _dataContext.Post(SafeOcr, CurrentImage?.FilePath);
            });

            var defaultImage = AppDomain.CurrentDomain.BaseDirectory + @"./tessdata/case.png";
            CurrentImage = new ImageClass
            {
                Image = new BitmapImage(new Uri(defaultImage, UriKind.RelativeOrAbsolute)),
                FilePath = defaultImage
            };

        }

        public void SafeOcr(object obj)
        {
            var dataPath = AppDomain.CurrentDomain.BaseDirectory + @"./tessdata";
            using (var engine = new TesseractEngine(dataPath, "chi_sim", EngineMode.Default))
            {
                using (var img = Pix.LoadFromFile(CurrentImage.FilePath))
                {
                    using (var page = engine.Process(img))
                    {
                        _uiContext.Post(SafeSetText, page.GetText());
                    }
                }
            }
        }

        public void SafeSetText(object obj)
        {
            RecognizedText = obj.ToString();
        }

        private string _recognizedText;
        public string RecognizedText
        {
            get
            {
                return _recognizedText;
            }
            set
            {
                _recognizedText = value;
                // ReSharper disable once RedundantArgumentDefaultValue
                OnPropertyChanged("RecognizedText");
            }
        }

        public void OpenImages(object obj)
        {
            try
            {
                var dlg = new Microsoft.Win32.OpenFileDialog { Multiselect = false };
                if (!dlg.ShowDialog().Value) return;

                var filenames = dlg.FileNames[0];

                CurrentImage = new ImageClass
                {
                    Image = new BitmapImage(new Uri(filenames, UriKind.Absolute)),
                    FilePath = filenames
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace);
            }
        }

        private ImageClass _currentImage;
        public ImageClass CurrentImage
        {
            get
            {
                return _currentImage;
            }
            set
            {
                _currentImage = value;

                // ReSharper disable once RedundantArgumentDefaultValue
                OnPropertyChanged("CurrentImage");
                // ReSharper disable once ExplicitCallerInfoArgument
                //OnPropertyChanged("CurrentImageText");
            }
        }
    }
}
