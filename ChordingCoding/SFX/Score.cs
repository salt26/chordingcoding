using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
//using Sanford.Multimedia.Midi;
using NFluidsynth;

namespace ChordingCoding.SFX
{
    /// <summary>
    /// 음표들을 담고 있는 악보 클래스입니다.
    /// </summary>
    class Score
    {
        /// <summary>
        /// 음표들을 담을 리스트.
        /// </summary>
        private List<Note> score = new List<Note>();

        /// <summary>
        /// NoteOff()를 호출할 음표들을 저장하는 버퍼
        /// </summary>
        private static List<Note> noteOffBuffer = new List<Note>();

        /// <summary>
        /// PlayEnumerable()에 의해 재생되어야 하는 악보들의 목록.
        /// Key는 악보, Value는 악보의 다음 재생 위치(64 * measure + position)
        /// </summary>
        private static List<ScoreWithPosition> playingScores = new List<ScoreWithPosition>();

        /// <summary>
        /// 악보의 길이
        /// </summary>
        private long length = 0;

        /// <summary>
        /// 악보의 길이. 읽기 전용입니다.
        /// </summary>
        public long Length
        {
            get
            {
                return length;
            }
        }

        /// <summary>
        /// 현재 재생 중이면 true가 됩니다.
        /// </summary>
        private bool isPlaying = false;

        /// <summary>
        /// 현재 재생 중이면 true를 반환합니다. 읽기 전용입니다.
        /// </summary>
        public bool IsPlaying
        {
            get
            {
                return isPlaying;
            }
        }

        /// <summary>
        /// 음표를 생성하여 악보에 추가합니다.
        /// </summary>
        /// <param name="pitch">음 높이(0 ~ 127). 예) 60: C4 / 64: E4 / 67: G4 / 72: C5</param>
        /// <param name="velocity">음 세기(1 ~ 127).</param>
        /// <param name="rhythm">음표의 길이(1 이상). 4/4박에서 한 마디를 64등분한 길이를 기준으로 합니다. 예) 64: 온음표 / 16: 4분음표 / 4: 16분음표 / 1: 64분음표</param>
        /// <param name="measure">음표가 위치한 마디 번호(0부터 시작).</param>
        /// <param name="position">음표의 마디 내 위치(0 ~ 63). 4/4박에서 한 마디를 64등분한 길이를 기준으로 합니다.</param>
        /// <param name="staff">음표가 놓일 Staff 번호(0 ~ 15). 9번 Staff는 타악기 전용 Staff입니다.</param>
        public void AddNote(int pitch, int velocity, int rhythm, long measure, int position, int staff = 0)
        {
            Note note = new Note(pitch, velocity, rhythm, measure, position, staff);
            score.Add(note);

            if (length < rhythm + measure * 64 + position)
            {
                length = rhythm + measure * 64 + position;
            }
        }

        /// <summary>
        /// 음표를 생성하여 악보에 추가합니다.
        /// </summary>
        /// <param name="pitch">음 높이(0 ~ 127)를 반환하는 함수. 예) () => 60: C4 / () => 64: E4 / () => 67: G4 / () => 72: C5</param>
        /// <param name="velocity">음 세기(1 ~ 127).</param>
        /// <param name="rhythm">음표의 길이(1 이상). 4/4박에서 한 마디를 64등분한 길이를 기준으로 합니다. 예) 64: 온음표 / 16: 4분음표 / 4: 16분음표 / 1: 64분음표</param>
        /// <param name="measure">음표가 위치한 마디 번호(0부터 시작).</param>
        /// <param name="position">음표의 마디 내 위치(0 ~ 63). 4/4박에서 한 마디를 64등분한 길이를 기준으로 합니다.</param>
        /// <param name="staff">음표가 놓일 Staff 번호(0 ~ 15). 9번 Staff는 타악기 전용 Staff입니다.</param>
        public void AddNote(Note.PitchGenerator pitch, int velocity, int rhythm, long measure, int position, int staff = 0)
        {
            Note note = new Note(pitch, SFXTheme.CurrentSFXTheme.Instruments[staff].accompanimentVolume, rhythm, measure, position, staff);
            score.Add(note);

            if (length < rhythm + measure * 64 + position)
            {
                length = rhythm + measure * 64 + position;
            }
        }

        /// <summary>
        /// 음표를 악보에 추가합니다.
        /// </summary>
        /// <param name="note">음표</param>
        public void AddNote(Note note)
        {
            score.Add(note);

            if (length < note.Rhythm + note.Measure * 64 + note.Position)
            {
                length = note.Rhythm + note.Measure * 64 + note.Position;
            }
        }

        /// <summary>
        /// 음표를 생성하여 악보에 추가합니다.
        /// 음 세기는 현재 악기 테마의 반주 악기 음량으로 자동 설정됩니다.
        /// 반주를 만들 때에만 사용할 수 있습니다.
        /// </summary>
        /// <param name="pitch">음 높이(0 ~ 127)를 반환하는 함수. 예) () => 60: C4 / () => 64: E4 / () => 67: G4 / () => 72: C5</param>
        /// <param name="rhythm">음표의 길이(1 이상). 4/4박에서 한 마디를 64등분한 길이를 기준으로 합니다. 예) 64: 온음표 / 16: 4분음표 / 4: 16분음표 / 1: 64분음표</param>
        /// <param name="measure">음표가 위치한 마디 번호(0부터 시작).</param>
        /// <param name="position">음표의 마디 내 위치(0 ~ 63). 4/4박에서 한 마디를 64등분한 길이를 기준으로 합니다.</param>
        /// <param name="staff">음표가 놓일 Staff 번호(0 ~ 15). 9번 Staff는 타악기 전용 Staff입니다.</param>
        public void AddNoteInAccompaniment(Note.PitchGenerator pitch, int rhythm, long measure, int position, int staff)
        {
            Note note = new Note(pitch, SFXTheme.CurrentSFXTheme.Instruments[staff].accompanimentVolume, rhythm, measure, position, staff);
            score.Add(note);

            if (length < rhythm + measure * 64 + position)
            {
                length = rhythm + measure * 64 + position;
            }
        }

        /// <summary>
        /// 음표를 악보에서 제거합니다.
        /// </summary>
        /// <param name="note">음표</param>
        public void RemoveNote(Note note)
        {
            // 주의: 동시성 문제가 일어나는 상황에서 이 함수를 사용하지 않을 것이라 믿고 lock 처리를 하지 않았음.
            bool b = score.Remove(note);

            if (b && length == note.Rhythm + note.Measure * 64 + note.Position)
            {
                // 악보에서 가장 마지막에 끝나는 음표를 제거한 경우 악보의 길이를 다시 계산해야 함
                long newLength = 0;
                foreach (Note n in score)
                {
                    if (newLength < n.Rhythm + n.Measure * 64 + n.Position)
                    {
                        newLength = n.Rhythm + n.Measure * 64 + n.Position;
                    }
                }
                length = newLength;
            }
        }

        /*
        /// <summary>
        /// 악보를 Midi 파일로 저장합니다.
        /// </summary>
        /// <param name="name">저장할 파일 이름(확장자 제외)</param>
        public void Save(string name = "Sample")
        {
            // 이 악보의 Midi message pair들을 받아옵니다.
            List<KeyValuePair<float, int>> list = ToMidi();

            // Sequence를 생성합니다.
            Sequence sequence = new Sequence();

            // Track을 생성합니다.
            Track track = new Track();

            // 악보에 있는 모든 음표를 기록합니다.
            foreach (KeyValuePair<float, int> p in list)
            {
                // Track에 음표를 추가합니다.
                // (Midi message pair를 번역하여 Midi message를 생성합니다.)
                track.Insert((int)(p.Key * 25 + 0.5f),
                    new ChannelMessage(p.Value > 0 ? ChannelCommand.NoteOn : ChannelCommand.NoteOff,
                    p.Value < 0 ? -p.Value >> 16 : p.Value >> 16, p.Value < 0 ? -p.Value & 65535 : p.Value & 65535, 127));
            }

            // Sequence에 Track을 추가하고, 이 Sequence를 파일로 저장합니다.
            sequence.Add(track);
            sequence.Save(name + ".mid");
            Console.WriteLine("Save file as " + name + ".mid");
        }
        */

        /*
        /// <summary>
        /// 악보를 재생합니다.
        /// 이미 재생 중인 악보는 중복하여 재생할 수 없습니다.
        /// </summary>
        /// <param name="outDevice">출력 디바이스</param>
        /// <param name="measure">재생할 음표의 마디 번호 (0부터 시작)</param>
        /// <param name="position">재생할 음표의 마디 내 위치(0 ~ 63). 4/4박에서 한 마디를 64등분한 길이를 기준으로 합니다.</param>
        /// <param name="staff">재생할 음표의 채널(Staff 번호). 인자로 -1을 주면 모든 채널의 음표를 재생합니다.</param>
        /// <param name="velocity">연주 세기 (0 ~ 127)</param>
        // (만약 Unity에서 작업할 경우, 타입을 void 대신 IEnumerator로 바꿔서 Coroutine으로 사용하세요.)
        public void Play(OutputDevice outDevice, long measure, int position, int staff = -1, int velocity = 127)
        {
            // 이미 재생 중인 악보이면 중복하여 재생하지 않습니다.
            if (isPlaying) return;
            isPlaying = true;
            //Console.WriteLine("Playing...");

            foreach (Note note in score)
            {
                if (note.Measure == measure && note.Position == position && (staff == -1 || note.Staff == staff))
                {
                    Note noteInCurrentTime = new Note(note.Pitch, note.Rhythm, Music.Measure, Music.Position, note.Staff);
                    PlayANote(outDevice, noteInCurrentTime, velocity);
                }
            }

            isPlaying = false;
            //Console.WriteLine("End of score.");
        }
        */
        /// <summary>
        /// 악보를 재생합니다.
        /// 이미 재생 중인 악보는 중복하여 재생할 수 없습니다.
        /// 매 tick마다 호출되어야 악보가 제대로 재생됩니다.
        /// 일반적인 경우에는 이 메서드 대신 Score.Play()를 사용하세요.
        /// </summary>
        /// <param name="syn">신디사이저</param>
        /// <param name="measure">재생할 음표의 마디 번호 (0부터 시작)</param>
        /// <param name="position">재생할 음표의 마디 내 위치(0 ~ 63). 4/4박에서 한 마디를 64등분한 길이를 기준으로 합니다.</param>
        /// <param name="staff">재생할 음표의 채널(Staff 번호). 인자로 -1을 주면 모든 채널의 음표를 재생합니다.</param>
        /// <param name="velocityChange">연주 세기를 변화시키기 위해 음 세기에 곱해질 값</param>
        public void PlayEnumerable(Synth syn, long measure, int position, int staff = -1, float velocityChange = 1f)
        {
            // 이미 재생 중인 악보이면 중복하여 재생하지 않습니다.
            if (isPlaying) return;
            isPlaying = true;
            //Console.WriteLine("Playing...");

            foreach (Note note in score)
            {
                if (note.Measure == measure && note.Position == position && (staff == -1 || note.Staff == staff))
                {
                    Console.WriteLine("재생은 하는 거냐? velocityChange: " + velocityChange);
                    Note noteInCurrentTime = new Note(note.Pitch, note.Velocity, note.Rhythm, Music.Measure, Music.Position, note.Staff);
                    PlayANote(syn, noteInCurrentTime, velocityChange);
                }
            }

            isPlaying = false;
            //Console.WriteLine("End of score.");
        }

        /// <summary>
        /// 악보를 재생합니다.
        /// 이미 재생 중인 악보는 중복하여 재생할 수 없습니다.
        /// 한 번 호출하면 자동으로 악보의 끝까지 재생합니다.
        /// </summary>
        /// <param name="score">재생할 악보</param>
        /// <param name="startMeasure">재생을 시작할 마디 번호 (기본값은 0)</param>
        /// <param name="startPosition">재생을 시작할 마디 내 위치(0 ~ 63, 기본값은 0). 4/4박에서 한 마디를 64등분한 길이를 기준으로 합니다.</param>
        public static void Play(Score score, long startMeasure = 0, int startPosition = 0)
        {
            if (startMeasure < 0) startMeasure = 0;
            if (startPosition < 0 || startPosition > 63) startPosition = 0;
            long start = startMeasure * 64 + startPosition;

            void playingScoresAdd(object[] args)
            {
                List<ScoreWithPosition> playingScores_ = args[0] as List<ScoreWithPosition>;
                Score score_ = args[1] as Score;
                long start_ = (long)args[2];
                Console.WriteLine("playingScoresAdd");
                playingScores_.Add(new ScoreWithPosition(score_, start_));
            }

            Util.TaskQueue.Add("playingScores", playingScoresAdd, playingScores, score, start);
        }

        /// <summary>
        /// 매 tick마다 호출되어 playingScores에 들어있는 악보들을 재생해주는 helper 메서드입니다.
        /// </summary>
        public static void PlayPerTick(Synth syn, float velocityChange = 1f)
        {
            void playingScoresPlay(object[] args)
            {
                List<ScoreWithPosition> playingScores_ = args[0] as List<ScoreWithPosition>;
                Synth syn_ = args[1] as Synth;
                float velocityChange_ = (float)args[2];
                List<ScoreWithPosition> deadScores = new List<ScoreWithPosition>();
                foreach (ScoreWithPosition p in playingScores_)
                {
                    long measure = p.position / 64;
                    int position = (int)(p.position % 64);
                    Console.WriteLine("Current position: " + p.position);
                    p.score.Print();
                    p.score.PlayEnumerable(syn, measure, position, -1, velocityChange_);

                    if (p.position + 1 <= p.score.length)
                    {
                        p.position++;
                    }
                    else
                    {
                        deadScores.Add(p);
                        Console.WriteLine("End of score - length: " + p.score.length);
                    }
                    Console.WriteLine();
                }
                playingScores_.RemoveAll(x => deadScores.Contains(x));
                Console.WriteLine(playingScores_.Count + " / " + playingScores.Count + "\n\n");
            }

            Util.TaskQueue.Add("playingScores", playingScoresPlay, playingScores, syn, velocityChange);
        }

        /*
        /// <summary>
        /// 음표 하나를 재생합니다.
        /// 이미 재생 중인 다른 음표와 동시에 재생할 수 있으며
        /// 음표의 길이만큼 연주하고 음이 사라집니다.
        /// </summary>
        /// <param name="outDevice">출력 디바이스</param>
        /// <param name="note">재생할 음표</param>
        /// <param name="velocity">연주 세기 (0 ~ 127)</param>
        // (만약 Unity에서 작업할 경우, 타입을 void 대신 IEnumerator로 바꿔서 Coroutine으로 사용하세요.)
        public static void PlayANote(OutputDevice outDevice, Note note, int velocity = 127)
        {
            // 이미 재생 중인 악보이면 중복하여 재생하지 않습니다.
            if (velocity < 0 || velocity >= 128) return;
            //Console.WriteLine("Playing...");

            // 악보에 있는 모든 음표를 재생합니다.
            KeyValuePair<float, int> p = note.ToMidi()[0];

            if (p.Value > 0)
            {
                // 음표를 재생합니다.
                // (Midi message pair를 번역하여 Midi message를 생성합니다.)
                try
                {
                    outDevice.Send(new ChannelMessage(ChannelCommand.NoteOn, p.Value >> 16, p.Value & 65535, velocity));
                }
                catch (ObjectDisposedException) { }
                catch (OutputDeviceException) { }
            }

            void noteOffBufferAdd(object[] args)
            {
                List<Note> noteOffBuffer_ = args[0] as List<Note>;
                Note note_ = args[1] as Note;
                noteOffBuffer_.Add(note_);
            }

            Util.TaskQueue.Add("noteOffBuffer", noteOffBufferAdd, noteOffBuffer, note);
            //Console.WriteLine("End of note.");
        }
        */

        /// <summary>
        /// 음표 하나를 재생합니다.
        /// 이미 재생 중인 다른 음표와 동시에 재생할 수 있으며
        /// 음표의 길이만큼 연주하고 음이 사라집니다.
        /// </summary>
        /// <param name="syn">신디사이저</param>
        /// <param name="note">재생할 음표</param>
        /// <param name="velocityChange">연주 세기를 변화시키기 위해 음 세기에 곱해질 값</param>
        public static void PlayANote(Synth syn, Note note, float velocityChange = 1f)
        {
            // 이미 재생 중인 악보이면 중복하여 재생하지 않습니다.
            if (velocityChange < 0) return;
            //Console.WriteLine("Playing...");

            // 악보에 있는 모든 음표를 재생합니다.
            KeyValuePair<float, int> p = note.ToMidi()[0];

            if (p.Value > 0)
            {
                // 음표를 재생합니다.
                // (Midi message pair를 번역하여 Midi message를 생성합니다.)
                try
                {
                    syn.NoteOn(p.Value >> 16, p.Value & 65535, Util.RoundAndClamp(note.Velocity * velocityChange, 0, 127));
                }
                catch (ObjectDisposedException) { }
                catch (FluidSynthInteropException) { }
            }

            void noteOffBufferAdd(object[] args)
            {
                List<Note> noteOffBuffer_ = args[0] as List<Note>;
                Note note_ = args[1] as Note;
                noteOffBuffer_.Add(note_);
            }

            Util.TaskQueue.Add("noteOffBuffer", noteOffBufferAdd, noteOffBuffer, note);
            //Console.WriteLine("End of note.");
        }

        /*
        /// <summary>
        /// 음표 하나를 음표의 길이와 관계없이 영원히 재생합니다.
        /// 이미 재생 중인 다른 음표와 동시에 재생할 수 있습니다.
        /// 재생을 멈추려면 해당 Staff에 Stop()을 호출해야 합니다.
        /// </summary>
        /// <param name="outDevice">출력 디바이스</param>
        /// <param name="note">재생할 음표</param>
        /// <param name="velocity">연주 세기 (0 ~ 127)</param>
        // (만약 Unity에서 작업할 경우, 타입을 void 대신 IEnumerator로 바꿔서 Coroutine으로 사용하세요.)
        public static void PlayANoteForever(OutputDevice outDevice, Note note, int velocity = 127)
        {
            // 이미 재생 중인 악보이면 중복하여 재생하지 않습니다.
            if (velocity < 0 || velocity >= 128) return;
            //Console.WriteLine("Playing...");

            // 악보에 있는 모든 음표를 재생합니다.
            KeyValuePair<float, int> p = note.ToMidi()[0];

            if (p.Value > 0)
            {
                // 음표를 재생합니다.
                // (Midi message pair를 번역하여 Midi message를 생성합니다.)
                try
                {
                    outDevice.Send(new ChannelMessage(ChannelCommand.NoteOn, p.Value >> 16, p.Value & 65535, velocity));
                }
                catch (ObjectDisposedException) { }
                catch (OutputDeviceException) { }
            }
            //Console.WriteLine("End of note.");
        }
        */
        /// <summary>
        /// 음표 하나를 음표의 길이와 관계없이 영원히 재생합니다.
        /// 이미 재생 중인 다른 음표와 동시에 재생할 수 있습니다.
        /// 재생을 멈추려면 해당 Staff에 Stop()을 호출해야 합니다.
        /// </summary>
        /// <param name="syn">신디사이저</param>
        /// <param name="note">재생할 음표</param>
        /// <param name="velocityChange">연주 세기를 변화시키기 위해 음 세기에 곱해질 값</param>
        public static void PlayANoteForever(Synth syn, Note note, float velocityChange = 1f)
        {
            // 이미 재생 중인 악보이면 중복하여 재생하지 않습니다.
            if (velocityChange < 0) return;
            //Console.WriteLine("Playing...");

            // 악보에 있는 모든 음표를 재생합니다.
            KeyValuePair<float, int> p = note.ToMidi()[0];

            if (p.Value > 0)
            {
                // 음표를 재생합니다.
                // (Midi message pair를 번역하여 Midi message를 생성합니다.)
                try
                {
                    syn.NoteOn(p.Value >> 16, p.Value & 65535, Util.RoundAndClamp(note.Velocity * velocityChange, 0, 127));
                }
                catch (ObjectDisposedException) { }
                catch (FluidSynthInteropException) { }
            }
            //Console.WriteLine("End of note.");
        }

        /*
        /// <summary>
        /// 재생 중인 Staff 하나의 재생을 멈춥니다.
        /// </summary>
        /// <param name="outDevice">출력 디바이스</param>
        // (만약 Unity에서 작업할 경우, 타입을 void 대신 IEnumerator로 바꿔서 Coroutine으로 사용하세요.)
        public static void Stop(OutputDevice outDevice, int staff = 0)
        {
            // 이미 재생 중인 악보이면 중복하여 재생하지 않습니다.
            if (staff < 0 || staff >= 16) return;
            //Console.WriteLine("Playing...");
            
            for (int p = 0; p < 128; p++)
            {
                // 음표의 재생을 멈춥니다.
                // (Midi message pair를 번역하여 Midi message를 생성합니다.)
                try
                {
                    outDevice.Send(new ChannelMessage(ChannelCommand.NoteOff, staff, p, 32));
                }
                catch (ObjectDisposedException) { }
                catch (OutputDeviceException) { }
            }
            //Console.WriteLine("End of note.");
        }
        */
        /// <summary>
        /// 재생 중인 Staff 하나의 재생을 멈춥니다.
        /// </summary>
        /// <param name="syn">신디사이저</param>
        public static void Stop(Synth syn, int staff = 0)
        {
            // 이미 재생 중인 악보이면 중복하여 재생하지 않습니다.
            if (staff < 0 || staff >= 16) return;
            //Console.WriteLine("Playing...");

            for (int p = 0; p < 128; p++)
            {
                // 음표의 재생을 멈춥니다.
                // (Midi message pair를 번역하여 Midi message를 생성합니다.)
                try
                {
                    syn.NoteOff(staff, p);
                }
                catch (ObjectDisposedException) { }
                catch (FluidSynthInteropException) { }
            }
            //Console.WriteLine("End of note.");
        }

        /*
        /// <summary>
        /// 매 64분음표 길이의 시간마다 호출되어, 현재 재생이 멈춰야 할 음표의 재생을 멈춥니다.
        /// </summary>
        /// <param name="outDevice"></param>
        public static void NoteOff(OutputDevice outDevice)
        {
            if (noteOffBuffer.Count <= 0) return;
            long measure = Music.Measure;
            int position = Music.Position;
            
            void noteOffBufferStop(object[] args)
            {
                List<Note> noteOffBuffer_ = args[0] as List<Note>;
                long measure_ = (long)args[1];
                int position_ = (int)args[2];
                OutputDevice outDevice_ = args[3] as OutputDevice;
                
                List<Note> deadBuffer = new List<Note>();
                for (int i = noteOffBuffer_.Count - 1; i >= 0; i--)
                {
                    Note note = noteOffBuffer_[i];

                    // 악보에 있는 모든 음표를 재생합니다.
                    KeyValuePair<float, int> p = note.ToMidi()[1];

                    if (p.Value <= 0 && p.Key <= measure_ * 64f + position_)
                    {
                        // 음표의 재생을 멈춥니다.
                        // (Midi message pair를 번역하여 Midi message를 생성합니다.)
                        try
                        {
                            outDevice_.Send(new ChannelMessage(ChannelCommand.NoteOff, -p.Value >> 16, -p.Value & 65535, 10));
                        }
                        catch (ObjectDisposedException) { }
                        catch (OutputDeviceException) { }
                        finally
                        {
                            deadBuffer.Add(noteOffBuffer_[i]);
                        }
                    }
                }
                noteOffBuffer_.RemoveAll(x => deadBuffer.Contains(x));
            }

            Util.TaskQueue.Add("noteOffBuffer", noteOffBufferStop, noteOffBuffer, measure, position, outDevice);
            
        }
        */
        /// <summary>
        /// 매 64분음표 길이의 시간마다 호출되어, 현재 재생이 멈춰야 할 음표의 재생을 멈춥니다.
        /// </summary>
        /// <param name="syn">신디사이저</param>
        public static void NoteOff(Synth syn)
        {
            if (noteOffBuffer.Count <= 0) return;
            long measure = Music.Measure;
            int position = Music.Position;

            void noteOffBufferStop(object[] args)
            {
                List<Note> noteOffBuffer_ = args[0] as List<Note>;
                long measure_ = (long)args[1];
                int position_ = (int)args[2];
                Synth syn_ = args[3] as Synth;

                List<Note> deadBuffer = new List<Note>();
                for (int i = noteOffBuffer_.Count - 1; i >= 0; i--)
                {
                    Note note = noteOffBuffer_[i];

                    // 악보에 있는 모든 음표를 재생합니다.
                    KeyValuePair<float, int> p = note.ToMidi()[1];

                    if (p.Value <= 0 && p.Key <= measure_ * 64f + position_)
                    {
                        // 음표의 재생을 멈춥니다.
                        // (Midi message pair를 번역하여 Midi message를 생성합니다.)
                        try
                        {
                            syn_.NoteOff(-p.Value >> 16, -p.Value & 65535);
                        }
                        catch (ObjectDisposedException) { }
                        catch (FluidSynthInteropException) { }
                        finally
                        {
                            deadBuffer.Add(noteOffBuffer_[i]);
                        }
                    }
                }
                noteOffBuffer_.RemoveAll(x => deadBuffer.Contains(x));
            }

            Util.TaskQueue.Add("noteOffBuffer", noteOffBufferStop, noteOffBuffer, measure, position, syn);

        }

        public static void ClearNoteOffBuffer()
        {
            void noteOffBufferClear(object[] args)
            {
                List<Note> noteOffBuffer_ = args[0] as List<Note>;
                noteOffBuffer_.Clear();
            }

            Util.TaskQueue.Add("noteOffBuffer", noteOffBufferClear, noteOffBuffer);
        }

        /// <summary>
        /// 음표들을 연주하기 위해 Midi message pair 리스트로 변환합니다.
        /// (이 Pair들은 재생하거나 저장할 때 Message로 번역됩니다.)
        /// </summary>
        /// <returns></returns>
        List<KeyValuePair<float, int>> ToMidi()
        {
            List<KeyValuePair<float, int>> list = new List<KeyValuePair<float, int>>();

            // 모든 음표의 Midi message pair들을 모아서 list에 추가합니다.
            foreach (Note n in score)
            {
                list.Add(n.ToMidi()[0]);
                list.Add(n.ToMidi()[1]);
            }

            // Midi message pair들을 타이밍이 빠른 것부터 순서대로 정렬합니다.
            list.Sort(delegate (KeyValuePair<float, int> p1, KeyValuePair<float, int> p2)
            {
                return p1.Key < p2.Key ? -1 : p1.Key > p2.Key ? 1 : p1.Value < p2.Value ? -1 : p1.Value > p2.Value ? 1 : 0;
            });

            return list;
        }

        void Print()
        {
            Console.WriteLine("Print score - length: " + Length);
            foreach (Note n in score)
            {
                Console.WriteLine("position: " + (n.Measure * 64 + n.Position) + " / rhythm: " + n.Rhythm + " / pitch: " + n.Pitch);
            }
        }
    }

    class ScoreWithPosition
    {
        public Score score;
        public long position;

        public ScoreWithPosition(Score score, long startPosition)
        {
            this.score = score;
            this.position = startPosition;
        }
    }
}
