using System.Windows.Controls;

namespace ChatteringFoolsRecorder.AudioCapture
{
    class AudioCapturePlugin : IModule
    {
        private AudioCaptureViewModel viewModel;
        private AudioCaptureView view;

        public string Name => "Audio Capture";

        public UserControl UserInterface
        {
            get { if (view == null) CreateView(); return view; }
        }

        private void CreateView()
        {
            view = new AudioCaptureView();
            viewModel = new AudioCaptureViewModel();
            view.DataContext = viewModel;
        }

        public void Deactivate()
        {
            viewModel.Dispose();
            view = null;
        }
    }
}
