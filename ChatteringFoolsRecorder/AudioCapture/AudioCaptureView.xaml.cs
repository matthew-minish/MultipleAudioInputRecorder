using NAudio.CoreAudioApi;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ChatteringFoolsRecorder.AudioCapture
{
    /// <summary>
    /// Interaction logic for AudioCaptureView.xaml
    /// </summary>
    public partial class AudioCaptureView : UserControl
    {
        public AudioCaptureView()
        {
            InitializeComponent();
        }

        private void OnCbObjectsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            comboBox.SelectedItem = null;
        }

        private void OnCbObjectCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (SelectableObject<MMDevice> cbObject in ((AudioCaptureViewModel)DataContext).CaptureDevices)
            {
                if (cbObject.IsSelected)
                    sb.AppendFormat("{0}, ", cbObject.ObjectData.FriendlyName);
            }
            devicesSelection.Text = sb.ToString().Trim().TrimEnd(',');
        }
    }
}
