using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Data;
using System.Threading;
using System.IO;
//using Sanford.Multimedia.Midi;
using NAudio.Wave;
using NAudio.MediaFoundation;
using NAudio.CoreAudioApi;
using NAudio.Dmo.Effect;
using NAudio.Dmo;
using NFluidsynth;

namespace ChordingCoding.SFX
{
    /// <summary>
    /// 음악 및 효과음을 재생하고 타이머를 관리하는 클래스입니다.
    /// </summary>
    class Music
    {
        public delegate void PlayEventDelegate(int pitch);

        private static float TICK_PER_SECOND = 29f;   // 1초에 호출되는 tick 수
        private static long tickNumber = 0;           // 테마 변경 후 지금까지 지난 tick 수

        /// <summary>
        /// 현재 마디 수를 나타냅니다.
        /// 테마를 바꾸면 초기화됩니다.
        /// </summary>
        public static long Measure
        {
            get
            {
                return tickNumber / 64;
            }
        }

        /// <summary>
        /// 현재 마디 내 위치를 나타냅니다.
        /// 0 이상 64 미만의 값을 가지며, 값이 1씩 올라갈 때마다 64분음표 하나만큼의 시간이 흐른 위치를 나타냅니다.
        /// 4/4박자를 가정합니다.
        /// </summary>
        public static int Position
        {
            get
            {
                return (int)(tickNumber % 64);
            }
        }

        // 반주 재생에 사용되는, 새 패턴을 재생하고 나서 지금까지 지난 tick 수 (Key는 staff 번호)
        private static Dictionary<int, int> accompanimentTickNumber = new Dictionary<int, int>();

        // 같은 반주를 연속으로 재생한 횟수 (Key는 staff 번호)
        private static Dictionary<int, int> accompanimentPlayNumber = new Dictionary<int, int>();

        /// <summary>
        /// 현재 화음
        /// </summary>
        public static Chord chord;

        private static int _noteResolution = 4;
        public static int NoteResolution
        {
            get
            {
                return _noteResolution;
            }
            set
            {
                if (!new int[] { 0, 2, 4, 8, 16 }.Contains(value)) return;
                _noteResolution = value;

                if (value == 0 && syncPlayBuffer.Count > 0)
                {
                    FlushSyncPlayBuffer();
                }
            }
        }
        //public static OutputDevice outDevice;
        public static Settings settings;
        public static Synth syn;
        public static AudioDriver adriver;

        public static BufferedWaveProvider sound;
        public static RawSourceWaveStream soundStream;
        //public static WaveOutEvent playback;
        public static DmoEffectWaveProvider<DmoWavesReverb, DmoWavesReverb.Params> reverb;
        public static WasapiOut outputDevice;
        public static byte[] buffer;
        public static MemoryStream stream;


        public static bool IsReady { get; private set; } = false;

        /// <summary>
        /// 동기화된 박자에 맞게 음표가 재생되는 순간 호출되는 이벤트입니다.
        /// </summary>
        public static event PlayEventDelegate OnPlayNotes;

        /// <summary>
        /// 동기화된 박자에 맞게 화음이 전이되는 순간 호출되는 이벤트입니다.
        /// </summary>
        public static event PlayEventDelegate OnChordTransition;

        private static Timer.TickDelegate tickDelegate;
        private static Timer timer;
        private static int timerNumber = 0;             // 1/1000초마다 1씩 증가, tick 간격마다 0으로 초기화
        private static List<Note> syncPlayBuffer = new List<Note>();
        private static bool syncTransitionBuffer = false;
        private static List<int> playPitchEventBuffer = new List<int>();                                    // PlayNoteInChord()에서 재생되는 메인 음을 저장
        //private static long time = 0;

        private static PowerManagement mgmt;

        /// <summary>
        /// 음악 출력 장치와 타이머를 초기화하고, 현재 음악 테마를 SFXThemeName으로 설정합니다.
        /// </summary>
        /// <param name="SFXThemeName">설정할 음악 테마 이름</param>
        /// <param name="noteResolution">단위 리듬</param>
        /// <param name="timerTickDelegates">타이머의 틱마다 추가로 실행할 메서드의 대리자 목록</param>
        public static void Initialize(string SFXThemeName, int noteResolution, Timer.TickDelegate[] timerTickDelegates = null)
        {
            //outDevice = new OutputDevice(0);
            settings = new Settings();
            settings[ConfigurationKeys.SynthAudioChannels].IntValue = 2;

            syn = new Synth(settings);
            try
            {
                syn.LoadSoundFont("FluidR3_GM.sf2", true);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.StackTrace);
            }
            for (int i = 0; i < 16; i++)
            {
                syn.SoundFontSelect(i, 0);
            }

            adriver = new AudioDriver(syn.Settings, syn);
            /*
            WaveInEvent recorder = new WaveInEvent
            {
                WaveFormat = new WaveFormat(44100, 2)
            };
            BufferedWaveProvider sound = new BufferedWaveProvider(recorder.WaveFormat);
            recorder.DataAvailable += (object sender, WaveInEventArgs e) =>
            {
                sound.AddSamples(e.Buffer, 0, e.BytesRecorded);
            };
            recorder.StartRecording();
            //sound.Read();
            */
            //playback = new WaveOutEvent();
            //playback.Init(sound);
            //playback.Play();
            /*
            sound = new BufferedWaveProvider(new WaveFormat(44100, 2));
            buffer = new byte[44100 * 4];
            stream = new MemoryStream();
            Task.Run(() => {
                soundStream = new RawSourceWaveStream(stream, new WaveFormat(44100, 2));
                reverb = new DmoEffectWaveProvider<DmoWavesReverb, DmoWavesReverb.Params>(soundStream);
                outputDevice = new WasapiOut();

                outputDevice.Init(reverb);
                outputDevice.Play();
            });
            */
            SFXTheme.CurrentSFXTheme = SFXTheme.FindSFXTheme(SFXThemeName);
            NoteResolution = noteResolution;
            Accompaniment.Initialize();
            ThemeChanged();

            tickDelegate += Tick;
            if (timerTickDelegates != null)
            {
                foreach (Timer.TickDelegate t in timerTickDelegates)
                {
                    tickDelegate += t;
                }
            }

            timerNumber = 0;
            timer = new Timer(1, TickTimer);

            mgmt = new PowerManagement();
            mgmt.InitPowerEvents();
            mgmt.OnPowerSuspend += Suspend;
            mgmt.OnPowerResume += Resume;

            syncPlayBuffer = new List<Note>();
            syncTransitionBuffer = false;
            playPitchEventBuffer = new List<int>();

            accompanimentTickNumber = new Dictionary<int, int>();
            accompanimentPlayNumber = new Dictionary<int, int>();
            accompanimentTickNumber.Add(7, 0);
            accompanimentTickNumber.Add(8, 0);
            accompanimentPlayNumber.Add(7, 0);
            accompanimentPlayNumber.Add(8, 0);

            IsReady = true;
            /*
            Task.Run(() =>
            {
                var capture = new WasapiLoopbackCapture();
                var effectProvider = new DmoEffectWaveProvider<DmoWavesReverb, DmoWavesReverb.Params>(new WaveInProvider(capture));

                PlayEffectorSound(capture, effectProvider);
                using (var outputDevice = new WasapiOut())
                {
                    outputDevice.Init(effectProvider);
                    capture.StartRecording();
                    outputDevice.Play();
                    while (capture.CaptureState != NAudio.CoreAudioApi.CaptureState.Stopped)
                    {
                        Thread.Sleep(500);
                        if (!IsReady) capture.StopRecording();
                    }
                    Console.WriteLine("Effector stopped");
                }

            });
            */
        }

        /// <summary>
        /// 타이머를 멈추고 일시적으로 음악이 재생되지 않게 합니다.
        /// </summary>
        public static void Suspend()
        {
            if (IsReady)
            {
                for (int i = 0; i <= 8; i++) StopPlaying(i);
                Score.ClearNoteOffBuffer();
                timer.Stop();
                IsReady = false;
            }
        }

        /// <summary>
        /// 타이머를 설정하고 음악이 다시 재생되도록 합니다.
        /// </summary>
        public static void Resume()
        {
            if (!IsReady)
            {
                timer = new Timer(1, TickTimer);
                IsReady = true;
            }
        }

        /// <summary>
        /// 음악 출력 장치를 끄고 타이머를 멈춥니다.
        /// </summary>
        public static void Dispose()
        {
            if (IsReady)
            {
                for (int i = 0; i <= 8; i++) StopPlaying(i);
                Score.ClearNoteOffBuffer();
                timer.Stop();
                //outDevice.Close();
                adriver.Dispose();
                //playback.Stop();
                //outputDevice.Stop();
                //outputDevice.Dispose();
                //reverb.Dispose();
                syn.Dispose();
                settings.Dispose();
                mgmt.Stop();
                IsReady = false;
            }
        }

        /// <summary>
        /// 현재 음악 테마가 바뀔 때마다 호출되어야 합니다.
        /// SFXTheme.CurrentSFXTheme이 null이면 아무 일도 일어나지 않습니다.
        /// </summary>
        public static void ThemeChanged()
        {
            if (SFXTheme.CurrentSFXTheme == null) return;

            void ChangeTheme(object[] args)
            {
                Score.InitializePlaylist();
                for (int i = 0; i <= 8; i++) StopPlaying(i);
                Score.ClearNoteOffBuffer();

                foreach (KeyValuePair<int, SFXTheme.InstrumentInfo> p in SFXTheme.CurrentSFXTheme.Instruments)
                {
                    try
                    {
                        if (p.Key == 7 || p.Key == 8)
                        {
                            if (p.Value.instrumentCode == -1)
                            {
                                if (SFXTheme.CurrentSFXTheme.Instruments.ContainsKey(1))
                                    //outDevice.Send(new ChannelMessage(ChannelCommand.ProgramChange, p.Key, SFXTheme.CurrentSFXTheme.Instruments[1].instrumentCode));
                                    syn.ProgramChange(p.Key, SFXTheme.CurrentSFXTheme.Instruments[1].instrumentCode);
                                else
                                    //outDevice.Send(new ChannelMessage(ChannelCommand.ProgramChange, p.Key, p.Value.instrumentCode));
                                    syn.ProgramChange(p.Key, p.Value.instrumentCode);
                            }
                            else
                            {
                                //outDevice.Send(new ChannelMessage(ChannelCommand.ProgramChange, p.Key, p.Value.instrumentCode));
                                syn.ProgramChange(p.Key, p.Value.instrumentCode);
                            }
                        }
                        else
                        {
                            //outDevice.Send(new ChannelMessage(ChannelCommand.ProgramChange, p.Key, p.Value.instrumentCode));
                            syn.ProgramChange(p.Key, p.Value.instrumentCode);
                        }
                    }
                    catch (ObjectDisposedException) { }
                    //catch (OutputDeviceException) { }
                }
                chord = new Chord(SFXTheme.CurrentSFXTheme.ChordTransition);
                tickNumber = 0;

                if (SFXTheme.IsReady)
                {
                    for (int staff = 7; staff <= 8; staff++)
                    {
                        if (SFXTheme.CurrentSFXTheme.Instruments.ContainsKey(staff))
                        {
                            Accompaniment.SetNewCurrentPattern(staff);
                            accompanimentPlayNumber[staff] = 0;
                            accompanimentTickNumber[staff] = 0;
                        }
                    }
                }
            }

            Util.TaskQueue.Add("play", ChangeTheme);
        }

        /// <summary>
        /// 음표 하나를 버퍼에 저장했다가 다음 박자에 맞춰 재생 장치에서 재생합니다.
        /// </summary>
        /// <param name="pitch"></param>
        /// <param name="rhythm"></param>
        /// <param name="staff"></param>
        /// <param name="velocity"></param>
        public static void PlayANoteSync(int pitch, int velocity, int rhythm, int staff)
        {
            if (!IsReady) return;

            void PlaySync(object[] args)
            {
                int pitch_ = (int)args[0];
                int velocity_ = (int)args[1];
                int rhythm_ = (int)args[2];
                int staff_ = (int)args[3];

                Note note = new Note(pitch_, velocity_, rhythm_, Measure, Position, staff_);

                if (NoteResolution == 0)
                {
                    //Score.PlayANote(outDevice, note, (int)Math.Round(velocity * (SFXTheme.CurrentSFXTheme.Volume / 100D)));
                    Score.PlayANote(syn, note, SFXTheme.CurrentSFXTheme.Volume / 100f);

                    List<int> tempPlayPitchEventBuffer = playPitchEventBuffer;
                    playPitchEventBuffer = new List<int>();
                    foreach (int p in tempPlayPitchEventBuffer)
                    {
                        OnPlayNotes?.Invoke(p);
                    }
                }
                else
                {
                    syncPlayBuffer.Add(note);
                }
            }

            Util.TaskQueue.Add("play", PlaySync, pitch, velocity, rhythm, staff);
        }

        /// <summary>
        /// 재생 장치에서 재생 중인 모든 음을 멈춥니다.
        /// </summary>
        /// <param name="staff"></param>
        public static void StopPlaying(int staff)
        {
            if (!IsReady) return;
            //Score.Stop(outDevice, staff);
            Score.Stop(syn, staff);
        }

        /// <summary>
        /// 현재 화음에 맞는 음 하나를 재생합니다.
        /// </summary>
        /// <returns></returns>
        public static void PlayNoteInChord()
        {
            // ChordingCoding에서 일반 문자를 입력할 때 호출됨

            int pitch = chord.NextNote();
            playPitchEventBuffer.Add(pitch);
            foreach (KeyValuePair<int, SFXTheme.InstrumentInfo> pair in SFXTheme.CurrentSFXTheme.Instruments)
            {
                if (pair.Value.characterVolume > 0)
                {
                    PlayANoteSync(pair.Value.characterPitchModulator(pitch),
                        pair.Value.characterVolume, pair.Value.characterRhythm, pair.Key);
                }
            }
        }

        /// <summary>
        /// 화음 전이를 일으키면서 새 화음의 음들을 재생합니다.
        /// 확률적으로 연주 속도가 변할 수 있습니다.
        /// </summary>
        /// <returns></returns>
        private static void PlayChordTransition()
        {
            // ChordingCoding에서 공백 문자를 입력할 때 호출됨

            int pitch;
            StopPlaying(0);
            chord = new Chord(SFXTheme.CurrentSFXTheme.ChordTransition, chord);
            pitch = chord.NextNote();
            syncPlayBuffer = new List<Note>();

            SFXTheme.InstrumentInfo ii = SFXTheme.CurrentSFXTheme.Instruments[0];
            foreach (int p in chord.NotesInChord())
            {
                if (p != pitch)
                {
                    PlayANoteSync(ii.whitespacePitchModulator(p),
                        ii.whitespaceVolume, ii.whitespaceRhythm, 0);
                }
            }
            foreach (KeyValuePair<int, SFXTheme.InstrumentInfo> pair in SFXTheme.CurrentSFXTheme.Instruments)
            {
                if (pair.Value.whitespaceVolume > 0 && pair.Key != 1)
                {
                    PlayANoteSync(pair.Value.whitespacePitchModulator(pitch),
                        pair.Value.whitespaceVolume, pair.Value.whitespaceRhythm, pair.Key);
                }
            }

            // 빠르기 변경
            Random r = new Random();
            SetTickPerSecond((int)TICK_PER_SECOND + (int)(1.2f * Math.Round(Util.GaussianRandom(r), MidpointRounding.AwayFromZero)));

            OnChordTransition?.Invoke(pitch);
        }

        /// <summary>
        /// 화음 전이를 버퍼에 저장했다가 2분음표(1초) 단위의 다음 박자에 맞춰 수행합니다.
        /// </summary>
        /// <returns></returns>
        public static void PlayChordTransitionSync()
        {
            if (IsReady)
            {
                if (NoteResolution == 0)
                {
                    PlayChordTransition();
                }
                else
                {
                    syncTransitionBuffer = true;
                }
            }
        }

        /// <summary>
        /// 초당 Tick() 호출 횟수를 설정하여 음악의 빠르기를 바꿉니다.
        /// </summary>
        /// <param name="tickPerSecond">초당 tick 수 (16 이상 42 이하, 높을수록 빠름)</param>
        public static void SetTickPerSecond(int tickPerSecond)
        {
            if (tickPerSecond < 16) tickPerSecond = 16;
            if (tickPerSecond > 42) tickPerSecond = 42;
            TICK_PER_SECOND = tickPerSecond;
            //Console.WriteLine(TICK_PER_SECOND);
            timerNumber = 0;
        }

        /// <summary>
        /// 리듬에 맞게 음표를 재생합니다.
        /// tickDelegate에 의해 1초에 32번씩 자동으로 호출됩니다.
        /// </summary>
        private static void Tick()
        {
            //Score.NoteOff(outDevice);
            Score.NoteOff(syn);

            void TickPlay(object[] args)
            {
                // 지속 효과음이 멈추는 것을 대비하여 
                if (SFXTheme.CurrentSFXTheme.Instruments.ContainsKey(5) &&
                    SFXTheme.CurrentSFXTheme.Instruments.ContainsKey(6))
                {
                    if (tickNumber % ((int)TICK_PER_SECOND * 10) == 0)
                    {
                        StopPlaying(5);
                        SFXTheme.InstrumentInfo inst = SFXTheme.CurrentSFXTheme.Instruments[5];
                        Note note = new Note(inst.sfxPitchModulator(45), inst.sfxVolume, inst.sfxRhythm, Measure, Position, 5);
                        //Score.PlayANoteForever(outDevice, note, (int)Math.Round(inst.sfxVolume * (SFXTheme.CurrentSFXTheme.Volume / 100D)));     // 기본 빗소리 (사라지지 않아야 함)
                        Score.PlayANoteForever(syn, note, SFXTheme.CurrentSFXTheme.Volume / 100f);
                    }
                    if (tickNumber % ((int)TICK_PER_SECOND * 10) == (int)TICK_PER_SECOND * 5)
                    {
                        StopPlaying(6);
                        SFXTheme.InstrumentInfo inst = SFXTheme.CurrentSFXTheme.Instruments[6];
                        Note note = new Note(inst.sfxPitchModulator(45), inst.sfxVolume, inst.sfxRhythm, Measure, Position, 6);
                        //Score.PlayANoteForever(outDevice, note, (int)Math.Round(inst.sfxVolume * (SFXTheme.CurrentSFXTheme.Volume / 100D)));     // 기본 빗소리 (사라지지 않아야 함)
                        Score.PlayANoteForever(syn, note, SFXTheme.CurrentSFXTheme.Volume / 100f);
                    }
                }

                for (int staff = 7; staff <= 8; staff++)
                {
                    if (SFXTheme.CurrentSFXTheme.Instruments.ContainsKey(staff))
                    {
                        if (accompanimentTickNumber[staff] >= Accompaniment.currentPatterns[staff].length)
                        {
                            accompanimentPlayNumber[staff]++;
                            if (accompanimentPlayNumber[staff] >= Accompaniment.currentPatterns[staff].iteration)
                            {
                                Accompaniment.SetNewCurrentPattern(staff);
                                accompanimentPlayNumber[staff] = 0;
                            }
                            accompanimentTickNumber[staff] = 0;
                        }
                        Score accompaniment = Accompaniment.currentPatterns[staff].score;

                        /*
                        // 64분음표 단위로 음을 하나씩 재생
                        long measure = accompanimentTickNumber[staff] / 64;
                        int position = accompanimentTickNumber[staff] % 64;
                        if (SFXTheme.CurrentSFXTheme.Instruments.ContainsKey(staff))
                        {
                            accompaniment.PlayEnumerable(syn, measure, position, staff,
                                (int)Math.Round(SFXTheme.CurrentSFXTheme.Instruments[staff].accompanimentVolume * (SFXTheme.CurrentSFXTheme.Volume / 100D)));

                        }
                        */

                        accompanimentTickNumber[staff]++;
                    }
                    else
                    {
                        accompanimentTickNumber[staff] = 0;
                    }
                }

                Score.PlayPerTick(syn, SFXTheme.CurrentSFXTheme.Volume / 100f);
            }
            Util.TaskQueue.Add("play", TickPlay);

            if (tickNumber % 64 == 0 && (SFXTheme.CurrentSFXTheme.Instruments.ContainsKey(7) || SFXTheme.CurrentSFXTheme.Instruments.ContainsKey(8)))
            {
                syncTransitionBuffer = false;
                PlayChordTransition();
            }
            else if (syncTransitionBuffer && tickNumber % 32 == 0)
            {
                syncTransitionBuffer = false;
                PlayChordTransition();
            }

            // 동기화된 박자(최소 리듬 단위)에 맞춰 버퍼에 저장되어 있던 음표 재생
            if (NoteResolution > 0 && tickNumber % NoteResolution == 0 && syncPlayBuffer.Count > 0)
            {
                FlushSyncPlayBuffer();
            }
            //SoundPipeline(buffer);

            void IncreaseTick(object[] args)
            {
                tickNumber++;
            }

            Util.TaskQueue.Add("play", IncreaseTick);
        }

        /// <summary>
        /// 1/1000초마다 호출되어, tick 간격마다 tickDelegate를 호출합니다.
        /// </summary>
        private static void TickTimer()
        {
            if (IsReady)
            {
                if (timerNumber >= (int)(1000f / TICK_PER_SECOND))
                {
                    timerNumber = 0;
                    tickDelegate();
                }
                timerNumber++;
            }
        }

        /// <summary>
        /// 버퍼에 저장되어 있던 음표를 안전하게 재생하고 버퍼를 비웁니다.
        /// </summary>
        private static void FlushSyncPlayBuffer()
        {
            /*
            List<KeyValuePair<Note, int>> tempSyncPlayBuffer = syncPlayBuffer;
            syncPlayBuffer = new List<KeyValuePair<Note, int>>();
            try
            {
                foreach (KeyValuePair<Note, int> p in tempSyncPlayBuffer)
                {
                    Score.PlayANote(outDevice, p.Key, (int)Math.Round(p.Value * (SFXTheme.CurrentSFXTheme.Volume / 100D)));
                }

                List<int> tempPlayPitchEventBuffer = playPitchEventBuffer;
                playPitchEventBuffer = new List<int>();
                foreach (int pitch in tempPlayPitchEventBuffer)
                {
                    OnPlayNotes?.Invoke(pitch);
                }
            }
            catch (InvalidOperationException)
            {

            }
            */
            void FlushPlay(object[] args)
            {
                List<Note> tempSyncPlayBuffer = syncPlayBuffer;
                syncPlayBuffer = new List<Note>();
                for (int i = tempSyncPlayBuffer.Count - 1; i >= 0; i--)
                {
                    Note n = tempSyncPlayBuffer[i];
                    //Score.PlayANote(outDevice, p.Key, (int)Math.Round(p.Value * (SFXTheme.CurrentSFXTheme.Volume / 100D)));
                    Score.PlayANote(syn, n, SFXTheme.CurrentSFXTheme.Volume / 100f);
                }

                List<int> tempPlayPitchEventBuffer = playPitchEventBuffer;
                playPitchEventBuffer = new List<int>();
                for (int i = tempPlayPitchEventBuffer.Count - 1; i >= 0; i--)
                {
                    OnPlayNotes?.Invoke(tempPlayPitchEventBuffer[i]);
                }
            }

            Util.TaskQueue.Add("play", FlushPlay);
        }

        /// <summary>
        /// Windows에서 나는 모든 소리를 20초 동안 녹음합니다.
        /// 녹음 파일은 "Desktop\NAudio" 폴더에 저장됩니다.
        /// </summary>
        /// <param name="capture"></param>
        public static void RecordSound(WasapiLoopbackCapture capture, DmoEffectWaveProvider<DmoWavesReverb, DmoWavesReverb.Params> reverb = null)
        {
            var outputFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "NAudio");
            Directory.CreateDirectory(outputFolder);
            var outputFilePath = Path.Combine(outputFolder, "recorded.wav");
            var writer = new WaveFileWriter(outputFilePath, capture.WaveFormat);
            var mp3FilePath = Path.Combine(outputFolder, $"recorded_{ DateTime.Now:yyMMdd-HH-mm-ss-fffff}.mp3");

            capture.DataAvailable += (s, a) =>
            {
                if (reverb != null)
                {
                    reverb.Read(a.Buffer, 0, a.BytesRecorded);
                }
                writer.Write(a.Buffer, 0, a.BytesRecorded);
                if (writer.Position > capture.WaveFormat.AverageBytesPerSecond * 20)
                {
                    capture.StopRecording();
                }
            };

            capture.RecordingStopped += (s, a) =>
            {
                MediaFoundationApi.Startup();
                writer.Dispose();
                writer = null;
                using (var reader = new WaveFileReader(outputFilePath))
                {
                    try
                    {
                        MediaFoundationEncoder.EncodeToMp3(reader, mp3FilePath);
                    }
                    catch (InvalidOperationException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                capture.Dispose();
                if (reverb != null)
                {
                    reverb.Dispose();
                }
            };
        }

        /// <summary>
        /// Windows에서 나는 모든 소리를 받아 reverb 효과를 주고 재생합니다.
        /// </summary>
        /// <param name="capture"></param>
        public static void PlayEffectorSound(WasapiLoopbackCapture capture, DmoEffectWaveProvider<DmoWavesReverb, DmoWavesReverb.Params> reverb)
        {
            if (reverb is null) return;
            capture.DataAvailable += (s, a) =>
            {
                reverb.Read(a.Buffer, 0, a.BytesRecorded);
            };

            capture.RecordingStopped += (s, a) =>
            {
                capture.Dispose();
                reverb.Dispose();
            };
        }

        public static void SynthesizeSound()
        {
            using (Settings settings = new Settings())
            {
                settings[ConfigurationKeys.SynthAudioChannels].IntValue = 2;
                using (Synth syn = new Synth(settings))
                {
                    syn.LoadSoundFont("FluidR3_GM.sf2", true);
                    for (int i = 0; i < 16; i++)
                    {
                        syn.SoundFontSelect(i, 0);
                    }
                }
            }
        }

        public static void SoundPipeline(byte[] tempLeftBuffer)
        {
            #region TODO 곧 없어질, 음악을 1초 단위로 합성하는 로직
            float second_per_tick = 1f / TICK_PER_SECOND;
            int frameNum = (int)(second_per_tick * 44100 * 2);
            GCHandle pinnedLeftArray = GCHandle.Alloc(tempLeftBuffer, GCHandleType.Pinned);
            IntPtr leftPointer = pinnedLeftArray.AddrOfPinnedObject();
            syn.WriteSample16(frameNum, leftPointer, 0, frameNum * 2, 2, leftPointer, 1, frameNum * 2, 2);
            /*
            for (int i = 0; i < frame * 4; i++)
            {
                Console.Write(tempLeftBuffer[i] + " ");
            }
            */
            stream.Write(tempLeftBuffer, 0, frameNum);
            //soundStream.Read(tempLeftBuffer, 0, frameNum);
            //sound.AddSamples(tempLeftBuffer, 0, frameNum);

            reverb.Read(tempLeftBuffer, 0, frameNum);
            Console.WriteLine();

            pinnedLeftArray.Free();
            #endregion
        }
    }
}
