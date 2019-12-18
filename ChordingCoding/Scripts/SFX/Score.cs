using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Sanford.Multimedia.Midi;

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
        /// NoteOff를 호출할 음표들을 저장하는 버퍼
        /// </summary>
        private static List<Note> noteOffBuffer = new List<Note>();

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
        /// <param name="rhythm">음표의 길이(1 이상). 4/4박에서 한 마디를 64등분한 길이를 기준으로 합니다. 예) 64: 온음표 / 16: 4분음표 / 4: 16분음표 / 1: 64분음표</param>
        /// <param name="measure">음표가 위치한 마디 번호(0부터 시작).</param>
        /// <param name="position">음표의 마디 내 위치(0 ~ 63). 4/4박에서 한 마디를 64등분한 길이를 기준으로 합니다.</param>
        /// <param name="staff">음표가 놓일 Staff 번호(0 ~ 15). 9번 Staff는 타악기 전용 Staff입니다.</param>
        public void AddNote(int pitch, int rhythm, long measure, int position, int staff = 0)
        {
            Note note = new Note(pitch, rhythm, measure, position, staff);
            score.Add(note);
        }

        /// <summary>
        /// 음표를 생성하여 악보에 추가합니다.
        /// </summary>
        /// <param name="pitch">음 높이(0 ~ 127)를 반환하는 함수. 예) () => 60: C4 / () => 64: E4 / () => 67: G4 / () => 72: C5</param>
        /// <param name="rhythm">음표의 길이(1 이상). 4/4박에서 한 마디를 64등분한 길이를 기준으로 합니다. 예) 64: 온음표 / 16: 4분음표 / 4: 16분음표 / 1: 64분음표</param>
        /// <param name="measure">음표가 위치한 마디 번호(0부터 시작).</param>
        /// <param name="position">음표의 마디 내 위치(0 ~ 63). 4/4박에서 한 마디를 64등분한 길이를 기준으로 합니다.</param>
        /// <param name="staff">음표가 놓일 Staff 번호(0 ~ 15). 9번 Staff는 타악기 전용 Staff입니다.</param>
        public void AddNote(Note.PitchGenerator pitch, int rhythm, long measure, int position, int staff = 0)
        {
            Note note = new Note(pitch, rhythm, measure, position, staff);
            score.Add(note);
        }

        /// <summary>
        /// 음표를 악보에 추가합니다.
        /// </summary>
        /// <param name="note">음표</param>
        public void AddNote(Note note)
        {
            score.Add(note);
        }

        /// <summary>
        /// 음표를 악보에서 제거합니다.
        /// </summary>
        /// <param name="note">음표</param>
        public void RemoveNote(Note note)
        {
            score.Remove(note);
        }

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

            /*
            float last = 0;

            // 악보에 있는 모든 음표를 재생합니다.
            foreach (KeyValuePair<float, int> p in ToMidi())
            {
                if (last != p.Key)
                {
                    // 다음 Message의 타이밍이 올 때까지 쉽니다.
                    // (만약 Unity에서 작업할 경우, Sleep 대신 Coroutine의 WaitForSecondsRealtime을 사용하면 됩니다.)
                    System.Threading.Thread.Sleep((int)((p.Key - last) / 2 * 1000));
                    last = p.Key;
                }

                if (p.Value > 0)
                {
                    // 음표를 재생합니다.
                    // (Midi message pair를 번역하여 Midi message를 생성합니다.)
                    try
                    {
                        outDevice.Send(new ChannelMessage(ChannelCommand.NoteOn, p.Value >> 16, p.Value & 65535, 127));
                    }
                    catch (ObjectDisposedException) { }
                    catch (OutputDeviceException) { }
                }
                else
                {
                    // 음표의 재생을 멈춥니다.
                    // (Midi message pair를 번역하여 Midi message를 생성합니다.)
                    try
                    {
                        outDevice.Send(new ChannelMessage(ChannelCommand.NoteOff, -p.Value >> 16, -p.Value & 65535, 127));
                    }
                    catch (ObjectDisposedException) { }
                    catch (OutputDeviceException) { }
                }
            }
            */
            foreach (Note note in score)
            {
                if (note.Measure == measure && note.Position == position && (staff == -1 || note.Staff == staff))
                {
                    Console.WriteLine("Play");
                    PlayANote(outDevice, note, velocity);
                    /*
                    Thread t1 = new Thread(new ThreadStart(() => PlayANote(outDevice, note, velocity)));
                    t1.Start();
                    */
                }
            }

            isPlaying = false;
            //Console.WriteLine("End of score.");
        }

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
            /*
            Thread t1 = new Thread(new ThreadStart(() => NoteOff(outDevice, note)));
            t1.Start();
            */
            noteOffBuffer.Add(note);
            Console.WriteLine("PlayANote");
            //Console.WriteLine("End of note.");
        }

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

        /// <summary>
        /// 매 64분음표 길이의 시간마다 호출되어, 현재 재생이 멈춰야 할 음표의 재생을 멈춥니다.
        /// </summary>
        /// <param name="outDevice"></param>
        public static void NoteOff(OutputDevice outDevice)
        {
            if (noteOffBuffer.Count <= 0) return;
            long measure = Music.Measure;
            float position = Music.Position;

            Console.WriteLine("Before: " + noteOffBuffer.Count);
            Console.WriteLine("curr: " + (measure * 64f + position));

            // TODO
            List<Note> tempBuffer = noteOffBuffer;
            List<Note> removingBuffer = new List<Note>();
            for (int i = tempBuffer.Count - 1; i >= 0; i--) {
                Note note = tempBuffer[i];

                // 악보에 있는 모든 음표를 재생합니다.
                KeyValuePair<float, int> p = note.ToMidi()[1];
                Console.WriteLine("note: " + p.Key);

                if (p.Value <= 0 && p.Key <= measure * 64f + position)
                {
                    // 음표의 재생을 멈춥니다.
                    // (Midi message pair를 번역하여 Midi message를 생성합니다.)
                    try
                    {
                        outDevice.Send(new ChannelMessage(ChannelCommand.NoteOff, -p.Value >> 16, -p.Value & 65535, 10));
                    }
                    catch (ObjectDisposedException) { }
                    catch (OutputDeviceException) { }
                    finally
                    {
                        removingBuffer.Add(tempBuffer[i]);
                    }
                }
            }
            noteOffBuffer.RemoveAll(x => removingBuffer.Contains(x));
            Console.WriteLine("After:  " + noteOffBuffer.Count);
        }

        public static void ClearNoteOffBuffer()
        {
            noteOffBuffer.Clear();
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
    }
}
