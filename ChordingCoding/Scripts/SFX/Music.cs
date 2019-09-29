﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Data;
using Sanford.Multimedia.Midi;

namespace ChordingCoding.SFX
{
    class Music
    {
        public delegate void TimerTickDelegate();

        public static TimerTickDelegate tickDelegate;

        public static int tickNumber = 0;           // 테마 변경 후 지금까지 지난 tick 수
        public const float TICK_PER_SECOND = 32f;   // 1초에 호출되는 tick 수

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
                    Score score = new Score();
                    foreach (KeyValuePair<Note, int> p in syncPlayBuffer)
                    {
                        score.PlayANote(outDevice, p.Key, (int)Math.Round(p.Value * (SFXTheme.CurrentSFXTheme.Volume / 100D)));
                    }
                    syncPlayBuffer.Clear();
                }
            }
        }
        public static OutputDevice outDevice;
        
        public static bool IsReady { get; private set; } = false;

        static List<KeyValuePair<Note, int>> syncPlayBuffer = new List<KeyValuePair<Note, int>>();

        public static void Initialize(string themeName, int noteResolution)
        {
            outDevice = new OutputDevice(0);
            chord = new Chord(SFXTheme.CurrentSFXTheme.ChordTransition);
            Music.NoteResolution = noteResolution;

            tickDelegate += Tick;
            
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 1000f / TICK_PER_SECOND;
            timer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);
            timer.Start();

            IsReady = true;
        }

        public static void Dispose()
        {
            if (IsReady)
                outDevice.Close();
        }
        
        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (IsReady)
                tickDelegate();
        }

        /// <summary>
        /// 음표 하나를 버퍼에 저장했다가 다음 박자에 맞춰 재생 장치에서 재생합니다.
        /// </summary>
        /// <param name="pitch"></param>
        /// <param name="rhythm"></param>
        /// <param name="staff"></param>
        /// <param name="velocity"></param>
        public static void PlayANoteSync(int pitch, int rhythm, int staff, int velocity)
        {
            if (!IsReady) return;
            Note note = new Note(pitch, rhythm, 0, 0, staff);

            if (NoteResolution == 0)
            {
                Score score = new Score();
                score.PlayANote(outDevice, note, (int)Math.Round(velocity * (SFXTheme.CurrentSFXTheme.Volume / 100D)));
            }
            else
            {
                syncPlayBuffer.Add(new KeyValuePair<Note, int>(note, velocity));
            }
        }

        /// <summary>
        /// 재생 장치에서 재생 중인 모든 음을 멈춥니다.
        /// </summary>
        /// <param name="staff"></param>
        public static void StopPlaying(int staff)
        {
            if (!IsReady) return;
            Score score = new Score();
            score.Stop(outDevice, staff);
        }

        /// <summary>
        /// 현재 화음에 맞는 음 하나를 재생합니다.
        /// </summary>
        /// <returns></returns>
        public static int PlayNoteInChord()
        {
            // ChordingCoding에서 일반 문자를 입력할 때 호출됨

            int pitch = chord.NextNote();
            foreach (KeyValuePair<int, SFXTheme.InstrumentInfo> pair in SFXTheme.CurrentSFXTheme.Instruments)
            {
                if (pair.Value.characterVolume > 0)
                {
                    PlayANoteSync(pair.Value.characterPitchModulator(pitch),
                        pair.Value.characterRhythm, pair.Key, pair.Value.characterVolume);
                }
            }
            return pitch;
        }

        /// <summary>
        /// 화음 전이를 일으키면서 새 화음의 음들을 재생합니다.
        /// </summary>
        /// <returns></returns>
        public static int PlayChordTransition()
        {
            // ChordingCoding에서 공백 문자를 입력할 때 호출됨

            int pitch;
            StopPlaying(0);
            chord = new Chord(SFXTheme.CurrentSFXTheme.ChordTransition, chord);
            pitch = chord.NextNote();

            SFXTheme.InstrumentInfo ii = SFXTheme.CurrentSFXTheme.Instruments[0];
            foreach (int p in chord.NotesInChord())
            {
                if (p != pitch)
                {
                    PlayANoteSync(ii.whitespacePitchModulator(p),
                        ii.whitespaceRhythm, 0, ii.whitespaceVolume);
                }
            }
            foreach (KeyValuePair<int, SFXTheme.InstrumentInfo> pair in SFXTheme.CurrentSFXTheme.Instruments)
            {
                if (pair.Value.whitespaceVolume > 0)
                {
                    PlayANoteSync(pair.Value.whitespacePitchModulator(pitch),
                        pair.Value.whitespaceRhythm, pair.Key, pair.Value.whitespaceVolume);
                }
            }
            return pitch;
        }

        /// <summary>
        /// 리듬에 맞게 음표를 재생합니다.
        /// tickDelegate에 의해 1초에 32번씩 자동으로 호출됩니다.
        /// </summary>
        public static void Tick()
        {
            tickNumber++;

            // 지속 효과음이 멈추는 것을 대비하여 
            if (SFXTheme.CurrentSFXTheme.Instruments.ContainsKey(3) &&
                SFXTheme.CurrentSFXTheme.Instruments.ContainsKey(4))
            {
                if (tickNumber % ((int)TICK_PER_SECOND * 10) == 0)
                {
                    StopPlaying(3);
                    tickNumber = 0;
                    Score score = new Score();
                    SFXTheme.InstrumentInfo inst = SFXTheme.CurrentSFXTheme.Instruments[3];
                    Note note = new Note(inst.sfxPitchModulator(45), inst.sfxRhythm, 0, 0, 3);
                    score.PlayANoteForever(outDevice, note, (int)Math.Round(inst.sfxVolume * (SFXTheme.CurrentSFXTheme.Volume / 100D)));     // 기본 빗소리 (사라지지 않아야 함)
                }
                if (tickNumber % ((int)TICK_PER_SECOND * 10) == (int)TICK_PER_SECOND * 5)
                {
                    StopPlaying(4);
                    Score score = new Score();
                    SFXTheme.InstrumentInfo inst = SFXTheme.CurrentSFXTheme.Instruments[4];
                    Note note = new Note(inst.sfxPitchModulator(45), inst.sfxRhythm, 0, 0, 4);
                    score.PlayANoteForever(outDevice, note, (int)Math.Round(inst.sfxVolume * (SFXTheme.CurrentSFXTheme.Volume / 100D)));     // 기본 빗소리 (사라지지 않아야 함)
                }
            }

            // 동기화된 박자(최소 리듬 단위)에 맞춰 버퍼에 저장되어 있던 음표 재생
            if (NoteResolution > 0 && tickNumber % NoteResolution == 0 && syncPlayBuffer.Count > 0)
            {
                Score score = new Score();
                foreach (KeyValuePair<Note, int> p in syncPlayBuffer)
                {
                    score.PlayANote(outDevice, p.Key, (int)Math.Round(p.Value * (SFXTheme.CurrentSFXTheme.Volume / 100D)));
                }
                syncPlayBuffer.Clear();
            }
        }
    }
}
