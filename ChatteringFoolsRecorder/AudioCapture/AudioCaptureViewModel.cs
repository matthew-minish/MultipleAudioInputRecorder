using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using ChatteringFoolsRecorder.ViewModel;
using System.Text;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace ChatteringFoolsRecorder.AudioCapture
{
    internal class AudioCaptureViewModel : ViewModelBase, IDisposable
    {
        private int sampleRate = 44100;
        private int bitDepth = 32;
        private int channelCount = 2;
        private int sampleTypeIndex;
        private string message;
        private float peak;
        private readonly SynchronizationContext synchronizationContext;

        private List<Task> recordingTasks;
        private List<CancellationTokenSource> taskCancellations;

        public RecordingsViewModel RecordingsViewModel { get; }

        public DelegateCommand RecordCommand { get; }
        public DelegateCommand StopCommand { get; }

        private int shareModeIndex;

        public AudioCaptureViewModel()
        {
            recordingTasks = new List<Task>();
            taskCancellations = new List<CancellationTokenSource>();
            synchronizationContext = SynchronizationContext.Current;
            var enumerator = new MMDeviceEnumerator();
            CaptureDevices = enumerator
                .EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToArray()
                .Select(x => new SelectableObject<MMDevice>(x, false)).ToList();
            var defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
            RecordCommand = new DelegateCommand(Record);
            StopCommand = new DelegateCommand(Stop) { IsEnabled = false };
            RecordingsViewModel = new RecordingsViewModel();
        }

        private void Stop()
        {
            foreach (CancellationTokenSource source in taskCancellations)
            {
                source.Cancel();
            }

            Message = "Stopped";

            RecordCommand.IsEnabled = true;
            StopCommand.IsEnabled = false;
        }

        private void Record()
        {
            try
            {
                foreach (MMDevice device in SelectedDevices)
                {
                    CancellationTokenSource source = new CancellationTokenSource();
                    taskCancellations.Add(source);
                    recordingTasks.Add(RecordAsync(device.ID, source.Token));
                }

                foreach (Task t in recordingTasks)
                {
                    t.Start();
                }

                RecordCommand.IsEnabled = false;
                StopCommand.IsEnabled = true;
                Message = "Recording...";
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private Task RecordAsync(string deviceId, CancellationToken cancellationToken)
        {
            return new Task(() =>
            {
                MMDevice device = new MMDeviceEnumerator().GetDevice(deviceId);
                WasapiCapture capture = new WasapiCapture(device);
                capture.ShareMode = ShareModeIndex == 0 ? AudioClientShareMode.Shared : AudioClientShareMode.Exclusive;
                string filename = String.Format("ChatteringFools {0} {1:yyy-MM-dd HH-mm-ss}.wav", device.FriendlyName, DateTime.Now);
                WaveFileWriter writer = new WaveFileWriter(Path.Combine(RecordingsViewModel.OutputFolder,
                    filename),
                    capture.WaveFormat);
                capture.StartRecording();

                capture.RecordingStopped += delegate
                {
                    writer.Dispose();
                    writer = null;
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        RecordingsViewModel.Recordings.Add(filename);
                        RecordingsViewModel.SelectedRecording = filename;
                    }));
                    capture.Dispose();
                    capture = null;
                    RecordCommand.IsEnabled = true;
                    StopCommand.IsEnabled = false;
                };

                capture.DataAvailable += new EventHandler<WaveInEventArgs>(delegate (object o, WaveInEventArgs waveInEventArgs)
                {
                    writer.Write(waveInEventArgs.Buffer, 0, waveInEventArgs.BytesRecorded);
                });

                cancellationToken.Register(() =>
                {
                    capture.StopRecording();
                });
            });
        }

        public IList<SelectableObject<MMDevice>> CaptureDevices { get; }

        public float Peak
        {
            get => peak;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (peak != value)
                {
                    peak = value;
                    OnPropertyChanged("Peak");
                }
            }
        }

        public List<MMDevice> SelectedDevices
        {
            get => CaptureDevices.Where(x => x.IsSelected).Select(x => x.ObjectData).ToList();
        }

        public int SampleRate
        {
            get => sampleRate;
            set
            {
                if (sampleRate != value)
                {
                    sampleRate = value;
                    OnPropertyChanged("SampleRate");
                }
            }
        }

        public int BitDepth
        {
            get => bitDepth;
            set
            {
                if (bitDepth != value)
                {
                    bitDepth = value;
                    OnPropertyChanged("BitDepth");
                }
            }
        }

        public int ChannelCount
        {
            get => channelCount;
            set
            {
                if (channelCount != value)
                {
                    channelCount = value;
                    OnPropertyChanged("ChannelCount");
                }
            }
        }

        public bool IsBitDepthConfigurable => SampleTypeIndex == 1;

        public int SampleTypeIndex
        {
            get => sampleTypeIndex;
            set
            {
                if (sampleTypeIndex != value)
                {
                    sampleTypeIndex = value;
                    OnPropertyChanged("SampleTypeIndex");
                    BitDepth = sampleTypeIndex == 1 ? 16 : 32;
                    OnPropertyChanged("IsBitDepthConfigurable");
                }
            }
        }

        public string Message
        {
            get => message;
            set
            {
                if (message != value)
                {
                    message = value;
                    OnPropertyChanged("Message");
                }
            }
        }

        public int ShareModeIndex
        {
            get => shareModeIndex;
            set
            {
                if (shareModeIndex != value)
                {
                    shareModeIndex = value;
                    OnPropertyChanged("ShareModeIndex");
                }
            }
        }


        public void Dispose()
        {
            Stop();
        }
    }
}
