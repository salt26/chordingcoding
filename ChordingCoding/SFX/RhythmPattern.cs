/*
MIT License

Copyright (c) 2019 salt26

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using com.sun.org.apache.regexp.@internal;
using com.sun.xml.@internal.bind.v2.model.core;
using javax.xml.transform;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChordingCoding.SFX
{
    /// <summary>
    /// 추상적인 리듬 패턴을 표현하는 클래스입니다.
    /// 리듬 패턴을 구체화하여 악보로 만들 수 있습니다.
    /// </summary>
    public class RhythmPattern
    {
        /// <summary>
        /// 음표 목록.
        /// 항상 음표의 시작 위치 순으로 정렬됩니다.
        /// 같은 시작 위치를 갖는 음표를 둘 이상 포함할 수 없습니다.
        /// 서로 다른 클러스터 번호는 128개까지만 가질 수 있습니다.
        /// </summary>
        public LinkedList<RhythmPatternNote> noteList = new LinkedList<RhythmPatternNote>();

        /// <summary>
        /// 메타데이터 목록.
        /// 항상 클러스터 번호 순으로 정렬됩니다.
        /// 같은 클러스터 번호를 갖는 메타데이터를 둘 이상 포함할 수 없습니다.
        /// </summary>
        public List<RhythmPatternMetadata> metadataList = new List<RhythmPatternMetadata>();

        /// <summary>
        /// 이 리듬 패턴이 변경된 적이 있으면 true가 되며,
        /// 이때는 몇몇 값들을 다시 계산해야 합니다.
        /// </summary>
        public bool IsDirty { get; private set; } = false;

        /// <summary>
        /// 모든 클러스터 번호마다 절대적인 음 높이가 할당된 경우 true가 됩니다.
        /// </summary>
        public bool HasImplemented { get; private set; } = false;

        /// <summary>
        /// 새 리듬 패턴을 생성합니다.
        /// 처음에 넣을 음표들을 인자로 지정할 수 있습니다.
        /// </summary>
        /// <param name="rhythmPatternNotes">리듬 패턴에 넣을 음표들</param>
        public RhythmPattern(params RhythmPatternNote[] rhythmPatternNotes)
        {
            foreach (RhythmPatternNote n in rhythmPatternNotes)
            {
                this.InsertNote(n);
            }
        }

        /// <summary>
        /// 불가능한 편집 연산을 수행한 경우 발생하는 비용입니다.
        /// 무한대라고 취급하면 됩니다.
        /// </summary>
        public const int INVALID_COST = int.MaxValue / 2;

        /// <summary>
        /// 음표 목록에 있는 한 음표가 직전 음표보다 높은 음을 가지면 1,
        /// 낮은 음을 가지면 -1, 같은 음을 가지거나 첫 번째 음표이면 0을 반환합니다.
        /// 음표 목록에서 이 음표를 찾지 못한 경우 예외를 발생시킵니다.
        /// </summary>
        /// <param name="noteInNoteList">음표 목록에 있는 한 음표</param>
        /// <returns></returns>
        public int PitchVariance(RhythmPatternNote noteInNoteList)
        {
            LinkedListNode<RhythmPatternNote> node = noteList.Find(noteInNoteList);
            if (node == null)
            {
                throw new InvalidOperationException("Error in PitchVariance");
                //return -2;
            }
            else
            {
                if (node.Value.pitchVariance != -2) return node.Value.pitchVariance;
                else
                {
                    int variance;
                    LinkedListNode<RhythmPatternNote> prev = node.Previous;
                    if (prev == null) variance = 0;
                    else if (prev.Value.PitchCluster < node.Value.PitchCluster) variance = 1;
                    else if (prev.Value.PitchCluster == node.Value.PitchCluster) variance = 0;
                    else variance = -1;

                    node.Value.pitchVariance = variance;
                    return variance;
                }
            }
        }

        /// <summary>
        /// 한 클러스터에 절대적인 음 높이를 지정합니다.
        /// 음 높이는 0 이상 127 이하이어야 하고,
        /// 클러스터 번호가 클수록 항상 더 높은 음만 지정될 수 있습니다.
        /// 지정에 성공한 경우 true를 반환합니다.
        /// </summary>
        /// <param name="pitch"></param>
        public bool SetAbsolutePitch(float cluster, int pitch)
        {
            if (pitch < 0 || pitch > 127) return false;
            if (!metadataList.Exists(e => e.pitchCluster == cluster)) return false;
            int metadataIndex = metadataList.FindIndex(e => e.pitchCluster == cluster);

            int prevPitch = -1;
            int i;
            for (i = 1; metadataIndex - i >= 0; i++)
            {
                if (metadataList[metadataIndex - i].absolutePitch != -1)
                {
                    prevPitch = metadataList[metadataIndex - i].absolutePitch;
                    break;
                }
            }
            int minPitch = prevPitch + i;

            int nextPitch = 128;
            int j;
            for (j = 1; metadataIndex + j < metadataList.Count; j++)
            {
                if (metadataList[metadataIndex + j].absolutePitch != -1)
                {
                    nextPitch = metadataList[metadataIndex + j].absolutePitch;
                    break;
                }
            }
            int maxPitch = nextPitch - j;

            if (pitch >= minPitch && pitch <= maxPitch)
            {
                metadataList[metadataIndex].absolutePitch = pitch;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 한 클러스터의 절대적인 음 높이를 설정하지 않은 상태로 초기화합니다.
        /// </summary>
        /// <param name="cluster"></param>
        public void ClearAbsolutePitch(float cluster)
        {
            if (!metadataList.Exists(e => e.pitchCluster == cluster)) return;
            int metadataIndex = metadataList.FindIndex(e => e.pitchCluster == cluster);

            metadataList[metadataIndex].absolutePitch = -1;
            HasImplemented = false;
        }

        /// <summary>
        /// 주어진 클러스터 번호의 클러스터 위상 인덱스를 반환합니다.
        /// 가장 작은 번호의 위상 인덱스는 0입니다.
        /// 메타데이터 목록에 존재하지 않는 클러스터 번호가 들어온 경우 -1을 반환합니다.
        /// </summary>
        /// <returns></returns>
        public int GetClusterRank(float cluster)
        {
            //metadataList.Sort();  // 여기서 이 코드를 실행하지 않아도 버그가 없어야 정상
            return metadataList.IndexOf(
                metadataList.Find(e => e.pitchCluster == cluster));
        }

        /// <summary>
        /// 주어진 클러스터 위상 인덱스에 삽입할, 적절한 새 클러스터 번호를 반환합니다.
        /// 삽입 후에 이 인덱스 이후의 기존 클러스터들은 위상이 한 칸씩 뒤로 밀려납니다.
        /// 클러스터 번호가 작을수록 낮은 음입니다.
        /// (주의: 이 메서드는 새 클러스터를 삽입해주지 않습니다.
        /// 새 클러스터를 삽입하려면 InsertNote() 또는 MoveNote()를 호출하고,
        /// 여기에 인자로 넣을 새 음표에 대해 이 메서드를 호출하십시오.)
        /// </summary>
        /// <param name="clusterRankToInsert">새 클러스터를 삽입할 클러스터 위상 인덱스</param>
        /// <returns></returns>
        public float GetNewClusterNumber(int clusterRankToInsert)
        {
            if (metadataList.Count == 0)
            {
                // 빈 리듬 패턴에 삽입할 음표의 클러스터 번호
                return 0f;
            }
            else if (clusterRankToInsert <= 0)
            {
                // 리듬 패턴의 어떤 음보다도 더 낮은 음으로 삽입할 음표의 클러스터 번호
                return metadataList[0].pitchCluster - 8f;
            }
            else if (clusterRankToInsert >= metadataList.Count)
            {
                // 리듬 패턴의 어떤 음보다도 더 높은 음으로 삽입할 음표의 클러스터 번호
                return metadataList[metadataList.Count - 1].pitchCluster + 8f;
            }
            else
            {
                // 리듬 패턴의 연속된 두 음 사이의 음 높이를 갖도록 삽입할 음표의 클러스터 번호
                return (metadataList[clusterRankToInsert - 1].pitchCluster +
                    metadataList[clusterRankToInsert].pitchCluster) / 2f;
            }
        }

        /// <summary>
        /// 주어진 클러스터 위상 인덱스를 가진 기존 클러스터 번호를 반환합니다.
        /// 이 클러스터에 새 음표를 삽입해도
        /// 다른 클러스터들의 위상에는 영향을 주지 않습니다.
        /// 클러스터 번호가 작을수록 낮은 음입니다.
        /// (주의: 이 메서드는 새 클러스터를 삽입해주지 않습니다.
        /// 새 클러스터를 삽입하려면 InsertNote() 또는 MoveNote()를 호출하고,
        /// 여기에 인자로 넣을 새 음표에 대해 이 메서드를 호출하십시오.)
        /// </summary>
        /// <param name="clusterRank">클러스터 위상 인덱스 (0 이상, 서로 다른 클러스터 개수 미만)</param>
        /// <returns></returns>
        public float GetExistingClusterNumber(int clusterRank)
        {
            if (metadataList.Count == 0)
            {
                // 빈 리듬 패턴에 삽입할 음표의 클러스터 번호
                return 0f;
            }
            else if (clusterRank < 0)
            {
                // 리듬 패턴에 있던 가장 낮은 음과 같은 음으로 삽입할 음표의 클러스터 번호
                return metadataList[0].pitchCluster;
            }
            else if (clusterRank >= metadataList.Count)
            {
                // 리듬 패턴에 있던 가장 높은 음과 같은 음으로 삽입할 음표의 클러스터 번호
                return metadataList[metadataList.Count - 1].pitchCluster;
            }
            else
            {
                // 주어진 클러스터 위상 인덱스를 갖는 클러스터 번호
                return metadataList[clusterRank].pitchCluster;
            }
        }

        /// <summary>
        /// 리듬 패턴에 음표 하나를 삽입하는 연산을 수행합니다.
        /// 반환값은 수행한 연산의 비용입니다.
        /// 같은 OnsetPosition에 여러 음표를 삽입하려 할 경우 삽입 연산이 수행되지 않고 INVALID_COST를 반환합니다.
        /// (새 음표를 정의할 때 GetNewClusterNumber() 또는
        /// GetExistingClusterNumber()를 사용하면 편리합니다.)
        /// </summary>
        /// <param name="note">삽입할 새 음표</param>
        /// <returns></returns>
        public int InsertNote(RhythmPatternNote note)
        {
            if (note == null) return INVALID_COST;
            note = note.Copy();

            LinkedListNode<RhythmPatternNote> afterNote = null;
            foreach (RhythmPatternNote n in noteList)
            {
                // 같은 위치에 여러 음표를 삽입할 수 없음 (단선율만 허용)
                if (n.OnsetPosition == note.OnsetPosition) return INVALID_COST;
                else if (n.OnsetPosition > note.OnsetPosition)
                {
                    // 이 음표 바로 앞에 삽입
                    afterNote = noteList.Find(n);
                    break;
                }
            }

            if (afterNote == null)
            {
                // 리듬 패턴이 비어있거나 삽입될 위치가 맨 뒤인 경우 맨 뒤에 삽입
                noteList.AddLast(note);

                // 메타데이터 편집
                RhythmPatternMetadata m = metadataList.Find(e => e.pitchCluster == note.PitchCluster);
                if (m != null)
                {
                    // 이미 같은 클러스터 번호의 음표가 존재하는 경우
                    m.noteOnsets.Add(note.OnsetPosition);
                }
                else
                {
                    // 새로운 클러스터 번호를 가진 경우
                    m = new RhythmPatternMetadata(note.PitchCluster, note.OnsetPosition);
                    metadataList.Add(m);
                    metadataList.Sort();
                }

                // 삽입된 음표의 음 높이 변화 계산
                int pv = PitchVariance(note);
                if (pv == 0)
                {
                    // (음표 추가 비용)
                    return 1;
                }
                else
                {
                    // 삽입된 음표의 음 높이 변화가 0이 아니므로 비용 추가
                    // (음표 추가 비용 + 삽입된 음표의 클러스터 위상 변경 비용 + 삽입된 음표의 음 높이 변화 변경 비용)
                    return 3;
                }
            }
            else
            {
                int afterOldPv = PitchVariance(afterNote.Value);

                noteList.AddBefore(afterNote, note);

                // 메타데이터 편집
                RhythmPatternMetadata m = metadataList.Find(e => e.pitchCluster == note.PitchCluster);
                if (m != null)
                {
                    // 이미 같은 클러스터 번호의 음표가 존재하는 경우
                    m.noteOnsets.Add(note.OnsetPosition);
                }
                else
                {
                    // 새로운 클러스터 번호를 가진 경우 메타데이터 추가 후 정렬
                    m = new RhythmPatternMetadata(note.PitchCluster, note.OnsetPosition);
                    metadataList.Add(m);
                    metadataList.Sort();
                }

                // 삽입된 음표 직후의 음표의 음 높이 변화를 다시 계산
                afterNote.Value.pitchVariance = -2;
                int afterPv = PitchVariance(afterNote.Value);
                int beta = 0;
                if (afterPv != afterOldPv)
                {
                    // 직후 음표의 음 높이 변화가 이전과 달라진 경우 비용 추가
                    // (직후 음표의 음 높이 변화 변경 비용)
                    beta = 1;
                }

                // 삽입된 음표의 음 높이 변화 계산
                int pv = PitchVariance(note);
                if (pv == 0)
                {
                    // (음표 추가 비용)
                    return 1 + beta;
                }
                else
                {
                    // 삽입된 음표의 음 높이 변화가 0이 아니므로 비용 추가
                    // (음표 추가 비용 + 삽입된 음표의 클러스터 위상 변경 비용 + 삽입된 음표의 음 높이 변화 변경 비용 + beta)
                    return 3 + beta;
                }
            }
        }

        /// <summary>
        /// 리듬 패턴에 음표 하나를 삽입하는 연산을 수행합니다.
        /// 반환값은 수행한 연산의 비용입니다.
        /// 같은 OnsetPosition에 여러 음표를 삽입하려 할 경우 삽입 연산이 수행되지 않고 INVALID_COST를 반환합니다.
        /// (새 음표를 정의할 때 GetNewClusterNumber() 또는
        /// GetExistingClusterNumber()를 사용하면 편리합니다.)
        /// </summary>
        /// <param name="noteOnset">삽입할 새 음표의 시작 위치</param>
        /// <param name="notePitchCluster">삽입할 새 음표의 음 높이 클러스터 번호</param>
        /// <returns></returns>
        public int InsertNote(int noteOnset, float notePitchCluster)
        {
            return InsertNote(new RhythmPatternNote(noteOnset, notePitchCluster));
        }

        /// <summary>
        /// 리듬 패턴에서 음표 하나를 제거하는 연산을 수행합니다.
        /// 반환값은 수행한 연산의 비용입니다.
        /// 존재하지 않는 음표를 제거하려 할 경우 제거 연산이 수행되지 않고 INVALID_COST를 반환합니다.
        /// </summary>
        /// <param name="noteOnset">제거할 기존 음표의 시작 위치</param>
        /// <returns></returns>
        public int DeleteNote(int noteOnset)
        {
            LinkedListNode<RhythmPatternNote> n = null;
            foreach (RhythmPatternNote note2 in noteList)
            {
                if (note2.OnsetPosition == noteOnset)
                {
                    n = noteList.Find(note2);
                    break;
                }
            }
            if (n == null) return INVALID_COST;

            RhythmPatternNote note = n.Value;

            // 메타데이터 편집
            RhythmPatternMetadata m = metadataList.Find(e => e.pitchCluster == note.PitchCluster);
            if (m != null)
            {
                if (!m.noteOnsets.Remove(note.OnsetPosition))
                {
                    Console.WriteLine("Error: RhythmPattern DeleteNote metadata 1");
                    return INVALID_COST;
                }
                if (m.noteOnsets.Count == 0)
                {
                    // 이 클러스터에 해당하는 음표가 모두 제거된 경우 메타데이터 제거
                    metadataList.Remove(m);
                }
            }
            else
            {
                Console.WriteLine("Error: RhythmPattern DeleteNote metadata 2");
                return INVALID_COST;
            }

            int pv = PitchVariance(note);
            int alpha = 0;
            if (pv == -1 || pv == 1)
            {
                // 제거할 음표의 음 높이 변화가 0이 아니므로 비용 추가
                // (제거할 음표의 클러스터 위상 변경 비용 + 제거할 음표의 음 높이 변화 변경 비용)
                alpha = 2;
            }

            int afterOldPv = -2;
            LinkedListNode<RhythmPatternNote> next = n.Next;
            if (next != null)
            {
                // 제거할 음표 직후의 음표가 존재하는 경우
                afterOldPv = PitchVariance(next.Value);
            }

            noteList.Remove(n);

            int beta = 0;
            if (afterOldPv != -2)
            {
                // 직후 음표의 음 높이 변화를 다시 계산
                next.Value.pitchVariance = -2;
                int afterPv = PitchVariance(next.Value);
                if (afterPv != afterOldPv)
                {
                    // 직후 음표의 음 높이 변화가 이전과 달라진 경우 비용 추가
                    // (직후 음표의 음 높이 변화 변경 비용)
                    beta = 1;
                }
            }

            // (음표 제거 비용 + alpha + beta)
            return 1 + alpha + beta;
        }

        /// <summary>
        /// 리듬 패턴에서 음표 하나를 제거하는 연산을 수행합니다.
        /// 반환값은 수행한 연산의 비용입니다.
        /// 인자로 넘긴 음표와 시작 위치가 같은 음표를 찾아 제거합니다.
        /// 존재하지 않는 음표를 제거하려 할 경우 제거 연산이 수행되지 않고 INVALID_COST를 반환합니다.
        /// </summary>
        /// <param name="note">제거할 기존 음표 (시작 위치만 중요)</param>
        /// <returns></returns>
        public int DeleteNote(RhythmPatternNote note)
        {
            return DeleteNote(note.OnsetPosition);
        }

        /// <summary>
        /// 리듬 패턴에 있던 음표 하나의 인덱스(음표 목록에서의 상대적 위치)를 유지하면서
        /// OnsetPosition과 클러스터를 옮기는 연산을 수행합니다.
        /// 반환값은 수행한 연산의 비용입니다.
        /// 존재하지 않는 음표를 옮기려고 하거나 새 OnsetPosition이 음표의 인덱스에 영향을 미칠 경우
        /// 옮기는 연산을 수행하지 않고 INVALID_COST를 반환합니다.
        /// (새 음표를 정의할 때 GetNewClusterNumber() 또는
        /// GetExistingClusterNumber()를 사용하면 편리합니다.)
        /// </summary>
        /// <param name="oldNoteOnset">옮기는 대상이 될 기존 음표의 시작 위치</param>
        /// <param name="newNote">옮겨질 새 음표</param>
        /// <returns></returns>
        public int MoveNote(int oldNoteOnset, RhythmPatternNote newNote)
        {
            if (newNote == null) return INVALID_COST;
            newNote = newNote.Copy();

            LinkedListNode<RhythmPatternNote> n = null;
            foreach (RhythmPatternNote note in noteList)
            {
                if (note.OnsetPosition == oldNoteOnset)
                {
                    n = noteList.Find(note);
                    break;
                }
            }
            if (n == null) return INVALID_COST;

            RhythmPatternNote oldNote = n.Value;

            if ((n.Previous != null &&
                newNote.OnsetPosition <= n.Previous.Value.OnsetPosition) ||
                (n.Next != null &&
                newNote.OnsetPosition >= n.Next.Value.OnsetPosition))
            {
                // 옮긴 결과로 음표의 인덱스가 바뀌는 경우 연산을 수행하지 않음
                return INVALID_COST;
            }

            int oldPv = PitchVariance(oldNote);
            int oldClusterIndex = GetClusterRank(oldNote.PitchCluster);

            // 메타데이터 편집
            // 기존 음표와 새 음표의 클러스터 번호가 서로 같은 경우 건드릴 필요 없음
            // -> 서로 같더라도 OnsetPosition이 다르면 건드려주어야 함!
            if (oldNote.PitchCluster != newNote.PitchCluster || oldNote.OnsetPosition != newNote.OnsetPosition)
            {
                // 기존 음표에 대한 메타데이터 수정
                RhythmPatternMetadata m = metadataList.Find(e => e.pitchCluster == oldNote.PitchCluster);
                if (m != null)
                {
                    if (!m.noteOnsets.Remove(oldNote.OnsetPosition))
                    {
                        Console.WriteLine("Error: RhythmPattern MoveNote metadata 1");
                        return INVALID_COST;
                    }
                    if (m.noteOnsets.Count == 0)
                    {
                        // 이 클러스터에 해당하는 음표가 모두 제거된 경우 메타데이터 제거
                        metadataList.Remove(m);
                    }
                }
                else
                {
                    Console.WriteLine("Error: RhythmPattern MoveNote metadata 2");
                    return INVALID_COST;
                }

                // 새 음표에 대한 메타데이터 수정
                m = metadataList.Find(e => e.pitchCluster == newNote.PitchCluster);
                if (m != null)
                {
                    // 이미 같은 클러스터 번호의 음표가 존재하는 경우
                    m.noteOnsets.Add(newNote.OnsetPosition);
                }
                else
                {
                    // 새로운 클러스터 번호를 가진 경우 메타데이터 추가 후 정렬
                    m = new RhythmPatternMetadata(newNote.PitchCluster, newNote.OnsetPosition);
                    metadataList.Add(m);
                    metadataList.Sort();
                }
            }

            int gamma = 0;
            //Console.WriteLine("Onset: " + oldNote.OnsetPosition + " -> " + newNote.OnsetPosition);
            if (oldNote.OnsetPosition != newNote.OnsetPosition)
            {
                // (음표 시작 위치 옮기는 비용)
                gamma = 1;
            }

            int afterOldPv = -2;
            if (n.Next != null)
            {
                afterOldPv = PitchVariance(n.Next.Value);
            }

            // 새 음표로 옮김
            n.Value = newNote;

            int alpha = 0;
            int newPv = PitchVariance(newNote);
            //Console.WriteLine("PitchVariance: " + oldPv + " -> " + newPv);
            if (oldPv != newPv)
            {
                // (옮기는 음표의 음 높이 변화 변경 비용)
                alpha += 1;
            }

            int newClusterIndex = GetClusterRank(newNote.PitchCluster);
            //Console.WriteLine("ClusterIndex: " + oldClusterIndex + " -> " + newClusterIndex);
            if (oldClusterIndex != newClusterIndex)
            {
                // (옮기는 음표의 클러스터 위상 변경 비용)
                alpha += 1;
            }

            int beta = 0;
            if (afterOldPv != -2)
            {
                // 직후 음표의 음 높이 변화를 다시 계산
                n.Next.Value.pitchVariance = -2;
                int afterPv = PitchVariance(n.Next.Value);
                //Console.WriteLine("afterPV: " + afterOldPv + " -> " + afterPv);
                if (afterPv != afterOldPv)
                {
                    // 직후 음표의 음 높이 변화가 이전과 달라진 경우 비용 추가
                    // (직후 음표의 음 높이 변화 변경 비용)
                    beta = 1;
                }
            }

            return gamma + alpha + beta;
        }

        /// <summary>
        /// 리듬 패턴에 있던 음표 하나의 인덱스(음표 목록에서의 상대적 위치)를 유지하면서
        /// OnsetPosition과 클러스터를 옮기는 연산을 수행합니다.
        /// 반환값은 수행한 연산의 비용입니다.
        /// 존재하지 않는 음표를 옮기려고 하거나 새 OnsetPosition이 음표의 인덱스에 영향을 미칠 경우
        /// 옮기는 연산을 수행하지 않고 INVALID_COST를 반환합니다.
        /// (새 음표를 정의할 때 GetNewClusterNumber() 또는
        /// GetExistingClusterNumber()를 사용하면 편리합니다.)
        /// </summary>
        /// <param name="oldNoteOnset">옮기는 대상이 될 기존 음표의 시작 위치</param>
        /// <param name="newNoteOnset">옮겨질 새 음표의 시작 위치</param>
        /// <param name="newNotePitchCluster">옮겨질 새 음표의 음 높이 클러스터 번호</param>
        /// <returns></returns>
        public int MoveNote(int oldNoteOnset, int newNoteOnset, float newNotePitchCluster)
        {
            return MoveNote(oldNoteOnset, new RhythmPatternNote(newNoteOnset, newNotePitchCluster));
        }

        /// <summary>
        /// 리듬 패턴에 있던 음표 하나의 인덱스(음표 목록에서의 상대적 위치)를 유지하면서
        /// OnsetPosition과 클러스터를 옮기는 연산을 수행합니다.
        /// 반환값은 수행한 연산의 비용입니다.
        /// 인자로 넘긴 oldNote와 시작 위치가 같은 음표를 찾아 옮깁니다.
        /// 존재하지 않는 음표를 옮기려고 하거나 새 OnsetPosition이 음표의 인덱스에 영향을 미칠 경우
        /// 옮기는 연산을 수행하지 않고 INVALID_COST를 반환합니다.
        /// (새 음표를 정의할 때 GetNewClusterNumber() 또는
        /// GetExistingClusterNumber()를 사용하면 편리합니다.)
        /// </summary>
        /// <param name="oldNote">옮기는 대상이 될 기존 음표 (시작 위치만 중요)</param>
        /// <param name="newNote">옮겨질 새 음표</param>
        /// <returns></returns>
        public int MoveNote(RhythmPatternNote oldNote, RhythmPatternNote newNote)
        {
            return MoveNote(oldNote.OnsetPosition, newNote);
        }

        /// <summary>
        /// 주어진 편집 연산 정보에 따라 해당 연산(Delete, Insert, Move)을 수행합니다.
        /// 반환값은 수행한 연산의 비용입니다.
        /// </summary>
        /// <param name="operation">편집 연산 정보</param>
        /// <returns></returns>
        public int PerformOperation(OperationInfo operation)
        {
            if (operation == null) return INVALID_COST;
            switch (operation.type)
            {
                case OperationInfo.Type.Delete:
                    return DeleteNote(operation.noteBeforeOp);
                case OperationInfo.Type.Insert:
                    return InsertNote(operation.noteAfterOp);
                case OperationInfo.Type.Move:
                    return MoveNote(operation.noteBeforeOp, operation.noteAfterOp);
                default:
                    return INVALID_COST;
            }
        }

        /// <summary>
        /// 이 리듬 패턴을 콘솔에 보기 좋게 출력합니다.
        /// 만약 'X'가 출력에 포함된다면 오류가 발생한 것입니다.
        /// </summary>
        public void Print()
        {
            if (noteList == null || noteList.Count == 0)
            {
                Console.WriteLine("Empty RhythmPattern");
                return;
            }
            for (int j = 0; j <= noteList.Last().OnsetPosition; j++)
            {
                if (j > 0 && j % 10 == 0) Console.Write((j / 10) % 10);
                else Console.Write(" ");
            }
            Console.WriteLine();

            for (int i = metadataList.Count - 1; i >= 0; i--)
            {
                for (int j = 0; j <= noteList.Last().OnsetPosition; j++)
                {
                    if (metadataList[i].noteOnsets.Contains(j))
                    {
                        bool b = false;
                        foreach (RhythmPatternNote n in noteList)
                        {
                            if (n.PitchCluster == metadataList[i].pitchCluster &&
                                n.OnsetPosition == j)
                            {
                                switch (PitchVariance(n))
                                {
                                    case 0:
                                        Console.Write("0");
                                        break;
                                    case 1:
                                        Console.Write("+");
                                        break;
                                    case -1:
                                        Console.Write("-");
                                        break;
                                }
                                b = true;
                            }
                        }
                        // Check if metadataList and noteList are inconsistent.
                        if (!b) Console.Write("X");
                    }
                    else
                    {
                        Console.Write(".");
                    }
                }
                Console.WriteLine(" " + metadataList[i].pitchCluster);
            }
        }

        /// <summary>
        /// 두 리듬 패턴 사이의 거리(비유사도)를 계산하여 반환합니다.
        /// 최소 거리를 구하기 위해 필요한 편집 연산들도 출력합니다.
        /// 거리는 이 리듬 패턴에 여러 번의 편집 연산(insert, delete, move)을 적용하여
        /// other 리듬 패턴으로 만들 수 있는 최소 비용으로 정의됩니다.
        /// </summary>
        /// <param name="other">다른 리듬 패턴</param>
        /// <returns></returns>
        public int Distance(RhythmPattern other)
        {
            // Dynamic programming

            int lenThis = this.noteList.Count;
            int lenOther = other.noteList.Count;
            List<List<DistanceTable>> distanceTable =
                new List<List<DistanceTable>>(lenThis + 1);

            // 우선 4가지 연산(forward delete, backward insert, forward move, backward move)만 가지고
            // distanceTable 초기화 (시간 복잡도 O(n^2))
            for (int i = 0; i <= lenThis; i++)
            {
                List<DistanceTable> temp =
                    new List<DistanceTable>(lenOther + 1);
                for (int j = 0; j <= lenOther; j++)
                {
                    temp.Add(new DistanceTable(INVALID_COST, null, INVALID_COST, null));
                }
                distanceTable.Add(temp);

                for (int j = 0; j <= lenOther; j++)
                {
                    if (i == 0 && j == 0)
                    {
                        var list = new List<KeyValuePair<List<int>, int>>
                        {
                            { new KeyValuePair<List<int>, int> (new List<int>(), 0) }
                        };
                        distanceTable[i][j] = new DistanceTable(0, list, 0, new List<int>(), 0);
                        Console.Write(distanceTable[i][j].intermediateDistance + "\t");     // TODO
                        continue;
                    }

                    List<DistanceTable> costs = new List<DistanceTable>();

                    #region forward delete, backward insert, forward move, backward move
                    
                    // 현재 단계(d_i,j)에서 이 연산들을 새로 수행하게 되면
                    // 이전 단계의 연산들을 다시 수행하는 순간에
                    // 리듬 패턴(환경)의 맨 마지막에 이전 단계에 없던 새로운 음표가 생기기 때문에
                    // 이전 단계에서 계산했던 몇몇 연산들(직후 음표가 없는 환경에서 수행한 연산들)의
                    // 비용을 다시 계산해야 한다.
                    List<int> operationCases = new List<int>
                    {
                        1,  // forward delete
                        -2, // backward insert
                        3,  // forward move
                        -3  // backward move
                    };

                    foreach (int operationCase in operationCases)
                    {
                        // 해당 연산을 수행할 수 없는 경우 배제
                        if ((operationCase == 1 || operationCase == 3 ||
                            operationCase == -3) && i <= 0) continue;
                        if ((operationCase == -2 || operationCase == 3 ||
                            operationCase == -3) && j <= 0) continue;

                        // 현재 단계(d_i,j)에서 수행할 연산 정보 정의
                        OperationInfo currentOp = new OperationInfo(operationCase, this.GetNoteByIndex(i - 1), other.GetNoteByIndex(j - 1));

                        // 현재 단계(d_i,j)에서 수행할 연산이 정방향(forward)이면 true, 역방향(backward)이면 false
                        bool isCurrentOpForward = operationCase > 0;
                        int k, l;

                        switch (operationCase)
                        {
                            case 1:
                            case -1:    // cannot occur
                                k = i - 1;
                                l = j;
                                break;
                            case 2:     // cannot occur
                            case -2:
                                k = i;
                                l = j - 1;
                                break;
                            case 3:
                            case -3:
                            default:
                                k = i - 1;
                                l = j - 1;
                                break;
                        }
                        // 이전 단계(d_k,l)
                        DistanceTable previous = distanceTable[k][l];

                        // Backward operation cannot be the first operation.
                        if (operationCase < 0 && previous.fixedPaths.Count == 1 &&
                            previous.fixedPaths[0].Key.Count == 0) continue;

                        // 이전 단계에서 최적이라고 알려진 모든 경로를 고려
                        foreach (var path in previous.fixedPaths) 
                        {
                            List<int> operations = new List<int>(path.Key);

                            RhythmPattern backwardRP, forwardRP;
                            int cCost, dCost = INVALID_COST;
                            List<OperationInfo> backwardOps = new List<OperationInfo>();
                            List<OperationInfo> forwardOps = new List<OperationInfo>();
                            k = i;
                            l = j;
                            bool pass = false;

                            // 1. 지금 수행할 연산이 수행 가능한 연산인지 확인하고 비용 계산
                            if (isCurrentOpForward)
                            {
                                forwardRP = Copy(other, j - 1);
                                if (forwardRP.PerformOperation(currentOp.Inverse()) == INVALID_COST)
                                {
                                    pass = true;
                                }
                                dCost = forwardRP.PerformOperation(currentOp);
                                if (dCost == INVALID_COST)
                                {
                                    pass = true;
                                }
                            }
                            else
                            {
                                backwardRP = Copy(this, i - 1);
                                dCost = backwardRP.PerformOperation(currentOp);
                                if (dCost == INVALID_COST)
                                {
                                    pass = true;
                                }
                            }
                            //currentOp.Print(false);
                            //Console.WriteLine(dCost);
                            // dCost는 현재 단계(d_i,j)에서 마지막으로 수행할 연산의 비용이 된다.

                            if (!pass)
                            {
                                // 2. 지난 연산들 중 다시 수행해야 하는 연산들을 찾아, 다시 수행할 순서대로 정렬

                                // 지금 수행할 연산도 operations에 추가
                                operations.Add(operationCase);

                                for (int m = operations.Count - 1; m >= 0; m--)
                                {
                                    int op = operations[m];
                                    if (op < 0)
                                    {
                                        // 역방향 연산
                                        backwardOps.Add(new OperationInfo(op, this.GetNoteByIndex(k - 1), other.GetNoteByIndex(l - 1)));
                                        switch (op)
                                        {
                                            case -1:
                                                k--;
                                                break;
                                            case -2:
                                                l--;
                                                break;
                                            case -3:
                                                k--;
                                                l--;
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        // 정방향 연산
                                        forwardOps.Insert(0, new OperationInfo(op, this.GetNoteByIndex(k - 1), other.GetNoteByIndex(l - 1)));
                                        switch (op)
                                        {
                                            case 1:
                                                k--;
                                                break;
                                            case 2:
                                                l--;
                                                break;
                                            case 3:
                                                k--;
                                                l--;
                                                break;
                                        }
                                    }

                                    if (!(op == -1 || op == 2))
                                    {
                                        // 이 연산이 역방향 delete도 아니고 정방향 insert도 아닌 경우
                                        // 여기까지만 비용을 재계산하면 됨 (더 전의 연산들을 확인하지 않아도 됨)
                                        break;
                                    }
                                }

                                // 3. 연산을 적용하는 순간의 리듬 패턴 환경 재구성
                                backwardRP = Copy(this, i - 1);
                                forwardRP = Copy(other, j - 1);
                                for (int m = forwardOps.Count - 1; m >= 0; m--)
                                {
                                    // 목표 리듬 패턴 상태에서 정방향 연산들의 역연산들을 취함
                                    if (forwardRP.PerformOperation(forwardOps[m].Inverse()) == INVALID_COST)
                                    {
                                        Console.WriteLine("RhythmPattern Distance() Error: How it is possible?");
                                    }
                                }

                                // 4. 연산 비용 재계산
                                cCost = 0;
                                for (int m = 0; m < backwardOps.Count; m++)
                                {
                                    cCost += backwardRP.PerformOperation(backwardOps[m]);
                                    if (cCost < 0 || cCost >= INVALID_COST) cCost = INVALID_COST;
                                }
                                for (int m = 0; m < forwardOps.Count; m++)
                                {
                                    cCost += forwardRP.PerformOperation(forwardOps[m]);
                                    if (cCost < 0 || cCost >= INVALID_COST) cCost = INVALID_COST;
                                }
                                cCost -= dCost;
                                if (cCost < 0 || cCost >= INVALID_COST) cCost = INVALID_COST;

                                // cCost는 현재 단계(d_i,j)에서 마지막으로 수행할 연산을 제외하고
                                // 다시 수행해야 할 이전 연산들의 비용의 합이 된다.

                                // 5. 최적 비용 계산을 위해 지금 계산한 비용 기록
                                if (cCost != INVALID_COST && dCost != INVALID_COST)
                                {
                                    costs.Add(new DistanceTable(previous.fixedDistance + cCost,
                                        new List<KeyValuePair<List<int>, int>>() { new KeyValuePair<List<int>, int>
                                            (new List<int>(operations), previous.fixedDistance + cCost) },
                                        previous.fixedDistance + cCost + dCost,
                                        new List<int>(operations),
                                        dCost,
                                        false));
                                }
                            }
                        }
                    }
                    #endregion

                    #region 현재 단계의 연산 결과(d_i,j) 기록

                    distanceTable[i][j] = new DistanceTable(INVALID_COST, new List<KeyValuePair<List<int>, int>>(), INVALID_COST, null);
                    //Console.WriteLine();
                    //Console.WriteLine();
                    //Console.WriteLine("costs.Count = " + costs.Count);

                    // 최솟값을 가지는 모든 c들(argmin)을 구해서 하나의 DistanceTable로 만든다.
                    // 최솟값은 iDistance로 저장되고, 각 경로들은 Dictionary로 묶여서 iPaths로 저장된다. 
                    foreach (DistanceTable c in costs)
                    {
                        // Overflow means there is an invalid operation.
                        if (c.fixedDistance < distanceTable[i][j].fixedDistance &&
                            c.fixedDistance >= 0 &&
                            c.fixedDistance < INVALID_COST)
                        {
                            distanceTable[i][j].fixedDistance = c.fixedDistance;
                            distanceTable[i][j].fixedPaths = c.fixedPaths;
                            distanceTable[i][j].intermediateDistance = c.fixedDistance;
                            distanceTable[i][j].finalDistance = c.finalDistance;
                            distanceTable[i][j].finalPath = c.finalPath;
                            distanceTable[i][j].lastOperationCost = c.lastOperationCost;
                        }
                        else if (c.fixedDistance == distanceTable[i][j].fixedDistance &&
                            c.fixedDistance >= 0 &&
                            c.fixedDistance < INVALID_COST)
                        {
                            distanceTable[i][j].fixedPaths.Add(c.fixedPaths[0]);
                            distanceTable[i][j].finalDistance = c.finalDistance;
                            distanceTable[i][j].finalPath = c.finalPath;
                            distanceTable[i][j].lastOperationCost = c.lastOperationCost;
                        }

                        if (i == lenThis && j == lenOther &&
                            c.finalDistance < distanceTable[i][j].finalDistance &&
                            c.finalDistance >= 0)
                        {
                            distanceTable[i][j].fixedDistance = c.fixedDistance;
                            distanceTable[i][j].fixedPaths = c.fixedPaths;
                            distanceTable[i][j].intermediateDistance = c.fixedDistance;
                            distanceTable[i][j].finalDistance = c.finalDistance;
                            distanceTable[i][j].finalPath = c.finalPath;
                            distanceTable[i][j].lastOperationCost = c.lastOperationCost;
                        }
                    }
                    Console.Write(distanceTable[i][j].intermediateDistance + "\t");  // TODO
                    #endregion
                }
                Console.WriteLine();                                // TODO
            }

            // 위에서 4가지 연산만으로 계산한 distanceTable의 스냅샷 저장
            List<List<DistanceTable>> oldDistanceTable =
                new List<List<DistanceTable>>(lenThis + 1);
            for (int i = 0; i <= lenThis; i++)
            {
                List<DistanceTable> temp =
                    new List<DistanceTable>(lenOther + 1);
                for (int j = 0; j <= lenOther; j++)
                {
                    DistanceTable dt = distanceTable[i][j];
                    temp.Add(new DistanceTable(dt.fixedDistance, null, dt.finalDistance, dt.finalPath, dt.lastOperationCost, dt.isLastOpBDOrFI));
                }
                oldDistanceTable.Add(temp);
            }

            Console.WriteLine("---------------------------------------------------------------------------");

            // 이번에는 6가지 연산 모두를 가지고
            // distanceTable 다시 계산 (시간 복잡도 O(2^n))
            for (int i = 0; i <= lenThis; i++)
            { 
                for (int j = 0; j <= lenOther; j++)
                {
                    if (i == 0 && j == 0)
                    {
                        var list = new List<KeyValuePair<List<int>, int>>
                        {
                            { new KeyValuePair<List<int>, int> (new List<int>(), 0) }
                        };
                        distanceTable[i][j] = new DistanceTable(0, list, 0, 0, new List<int>(), 0);
                        Console.Write(distanceTable[i][j].intermediateDistance + "\t");     // TODO
                        continue;
                    }

                    List<DistanceTable> costs = new List<DistanceTable>();

                    #region forward delete, backward insert, forward move, backward move

                    // 현재 단계(d_i,j)에서 이 연산들을 새로 수행하게 되면
                    // 이전 단계의 연산들을 다시 수행하는 순간에
                    // 리듬 패턴(환경)의 맨 마지막에 이전 단계에 없던 새로운 음표가 생기기 때문에
                    // 이전 단계에서 계산했던 몇몇 연산들(직후 음표가 없는 환경에서 수행한 연산들)의
                    // 비용을 다시 계산해야 한다.
                    List<int> operationCases = new List<int>
                    {
                        1,  // forward delete
                        -2, // backward insert
                        3,  // forward move
                        -3  // backward move
                    };

                    foreach (int operationCase in operationCases)
                    {
                        // 해당 연산을 수행할 수 없는 경우 배제
                        if ((operationCase == 1 || operationCase == 3 ||
                            operationCase == -3) && i <= 0) continue;
                        if ((operationCase == -2 || operationCase == 3 ||
                            operationCase == -3) && j <= 0) continue;

                        // 현재 단계(d_i,j)에서 수행할 연산 정보 정의
                        OperationInfo currentOp = new OperationInfo(operationCase, this.GetNoteByIndex(i - 1), other.GetNoteByIndex(j - 1));

                        // 현재 단계(d_i,j)에서 수행할 연산이 정방향(forward)이면 true, 역방향(backward)이면 false
                        bool isCurrentOpForward = operationCase > 0;
                        int k, l;

                        switch (operationCase)
                        {
                            case 1:
                            case -1:    // cannot occur
                                k = i - 1;
                                l = j;
                                break;
                            case 2:     // cannot occur
                            case -2:
                                k = i;
                                l = j - 1;
                                break;
                            case 3:
                            case -3:
                            default:
                                k = i - 1;
                                l = j - 1;
                                break;
                        }
                        // 이전 단계(d_k,l)
                        DistanceTable previous = distanceTable[k][l];

                        // Backward operation cannot be the first operation.
                        if (operationCase < 0 && previous.fixedPaths.Count == 1 &&
                            previous.fixedPaths[0].Key.Count == 0) continue;

                        // 이전 단계에서 최적이라고 알려진 모든 경로를 고려
                        foreach (var path in previous.fixedPaths)
                        {
                            List<int> operations = new List<int>(path.Key);

                            RhythmPattern backwardRP, forwardRP;
                            int cCost, dCost = INVALID_COST;
                            List<OperationInfo> backwardOps = new List<OperationInfo>();
                            List<OperationInfo> forwardOps = new List<OperationInfo>();
                            k = i;
                            l = j;
                            bool pass = false;

                            // 1. 지금 수행할 연산이 수행 가능한 연산인지 확인하고 비용 계산
                            if (isCurrentOpForward)
                            {
                                forwardRP = Copy(other, j - 1);
                                if (forwardRP.PerformOperation(currentOp.Inverse()) == INVALID_COST)
                                {
                                    pass = true;
                                }
                                dCost = forwardRP.PerformOperation(currentOp);
                                if (dCost == INVALID_COST)
                                {
                                    pass = true;
                                }
                            }
                            else
                            {
                                backwardRP = Copy(this, i - 1);
                                dCost = backwardRP.PerformOperation(currentOp);
                                if (dCost == INVALID_COST)
                                {
                                    pass = true;
                                }
                            }
                            //currentOp.Print(false);
                            //Console.WriteLine(dCost);
                            // dCost는 현재 단계(d_i,j)에서 마지막으로 수행할 연산의 비용이 된다.

                            if (!pass)
                            {
                                // 2. 지난 연산들 중 다시 수행해야 하는 연산들을 찾아, 다시 수행할 순서대로 정렬

                                // 지금 수행할 연산도 operations에 추가
                                operations.Add(operationCase);

                                for (int m = operations.Count - 1; m >= 0; m--)
                                {
                                    int op = operations[m];
                                    if (op < 0)
                                    {
                                        // 역방향 연산
                                        backwardOps.Add(new OperationInfo(op, this.GetNoteByIndex(k - 1), other.GetNoteByIndex(l - 1)));
                                        switch (op)
                                        {
                                            case -1:
                                                k--;
                                                break;
                                            case -2:
                                                l--;
                                                break;
                                            case -3:
                                                k--;
                                                l--;
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        // 정방향 연산
                                        forwardOps.Insert(0, new OperationInfo(op, this.GetNoteByIndex(k - 1), other.GetNoteByIndex(l - 1)));
                                        switch (op)
                                        {
                                            case 1:
                                                k--;
                                                break;
                                            case 2:
                                                l--;
                                                break;
                                            case 3:
                                                k--;
                                                l--;
                                                break;
                                        }
                                    }

                                    if (!(op == -1 || op == 2))
                                    {
                                        // 이 연산이 역방향 delete도 아니고 정방향 insert도 아닌 경우
                                        // 여기까지만 비용을 재계산하면 됨 (더 전의 연산들을 확인하지 않아도 됨)
                                        break;
                                    }
                                }

                                // 3. 연산을 적용하는 순간의 리듬 패턴 환경 재구성
                                backwardRP = Copy(this, i - 1);
                                forwardRP = Copy(other, j - 1);
                                for (int m = forwardOps.Count - 1; m >= 0; m--)
                                {
                                    // 목표 리듬 패턴 상태에서 정방향 연산들의 역연산들을 취함
                                    if (forwardRP.PerformOperation(forwardOps[m].Inverse()) == INVALID_COST)
                                    {
                                        Console.WriteLine("RhythmPattern Distance() Error: How it is possible?");
                                    }
                                }

                                // 4. 연산 비용 재계산
                                cCost = 0;
                                for (int m = 0; m < backwardOps.Count; m++)
                                {
                                    cCost += backwardRP.PerformOperation(backwardOps[m]);
                                    if (cCost < 0 || cCost >= INVALID_COST) cCost = INVALID_COST;
                                }
                                for (int m = 0; m < forwardOps.Count; m++)
                                {
                                    cCost += forwardRP.PerformOperation(forwardOps[m]);
                                    if (cCost < 0 || cCost >= INVALID_COST) cCost = INVALID_COST;
                                }
                                cCost -= dCost;
                                if (cCost < 0 || cCost >= INVALID_COST) cCost = INVALID_COST;

                                // cCost는 현재 단계(d_i,j)에서 마지막으로 수행할 연산을 제외하고
                                // 다시 수행해야 할 이전 연산들의 비용의 합이 된다.

                                // 5. 최적 비용 계산을 위해 지금 계산한 비용 기록
                                if (cCost != INVALID_COST && dCost != INVALID_COST)
                                {
                                    // fixedDistance와 intermediateDistance가 서로 같음!
                                    costs.Add(new DistanceTable(previous.fixedDistance + cCost,
                                        new List<KeyValuePair<List<int>, int>>() { new KeyValuePair<List<int>, int>
                                            (new List<int>(operations), previous.fixedDistance + cCost) },
                                        previous.fixedDistance + cCost,
                                        previous.fixedDistance + cCost + dCost,
                                        new List<int>(operations),
                                        dCost,
                                        false));
                                }
                            }
                        }
                    }
                    #endregion

                    #region backward delete, forward insert

                    // 현재 단계(d_i,j)에서 이 연산들을 새로 수행하게 되면
                    // 이전 단계의 연산들을 다시 수행하는 순간에
                    // 리듬 패턴(환경)의 맨 마지막에 새로 추가된 음표가 없기 때문에
                    // 이전에 수행한 연산들의 비용이 이전 단계에서 계산한 비용과 같게 유지된다.
                    // 따라서 이전 단계의 연산들의 비용을 아직은 다시 계산할 필요가 없다.
                    operationCases = new List<int>
                    {
                        -1, // backward delete
                        2   // forward insert
                    };

                    foreach (int operationCase in operationCases)
                    {
                        // 해당 연산을 수행할 수 없는 경우 배제
                        if ((operationCase == -1) && i <= 0) continue;
                        if ((operationCase == 2) && j <= 0) continue;

                        // 현재 단계(d_i,j)에서 수행할 연산 정보 정의
                        OperationInfo currentOp = new OperationInfo(operationCase, this.GetNoteByIndex(i - 1), other.GetNoteByIndex(j - 1));

                        // 현재 단계(d_i,j)에서 수행할 연산이 정방향(forward)이면 true, 역방향(backward)이면 false
                        bool isCurrentOpForward = operationCase > 0;
                        int k, l;

                        switch (operationCase)
                        {
                            case 1:     // cannot occur
                            case -1:
                                k = i - 1;
                                l = j;
                                break;
                            case 2:
                            case -2:    // cannot occur
                                k = i;
                                l = j - 1;
                                break;
                            default:    // cannot occur
                                k = i - 1;
                                l = j - 1;
                                break;
                        }
                        // 이전 단계(d_k,l)
                        DistanceTable previous = distanceTable[k][l];

                        // Backward operation cannot be the first operation.
                        if (operationCase < 0 && previous.fixedPaths.Count == 1 &&
                            previous.fixedPaths[0].Key.Count == 0) continue;

                        foreach (var path in previous.fixedPaths)
                        {
                            List<int> operations = new List<int>(path.Key);

                            RhythmPattern tempRP;
                            int dCost = INVALID_COST;

                            // 1. 지금 수행할 연산이 수행 가능한 연산인지 확인하고 비용 계산
                            if (isCurrentOpForward)
                            {
                                tempRP = Copy(other, j - 1);
                                if (tempRP.PerformOperation(currentOp.Inverse()) != INVALID_COST)
                                {
                                    dCost = tempRP.PerformOperation(currentOp);
                                }
                            }
                            else
                            {
                                tempRP = Copy(this, i - 1);
                                dCost = tempRP.PerformOperation(currentOp);
                            }
                            // dCost는 현재 단계(d_i,j)에서 마지막으로 수행할 연산의 비용이 된다.

                            // 2. 최적 비용 계산을 위해 지금 계산한 비용 기록
                            if (dCost != INVALID_COST)
                            {
                                // fixedDistance와 intermediateDistance가 서로 다름!
                                operations.Add(operationCase);
                                costs.Add(new DistanceTable(previous.fixedDistance,
                                    new List<KeyValuePair<List<int>, int>>() { new KeyValuePair<List<int>, int>
                                        (new List<int>(operations), previous.fixedDistance) },
                                    previous.finalDistance,
                                    previous.finalDistance + dCost,
                                    new List<int>(operations),
                                    dCost,
                                    true));
                            }
                        }
                    }
                    #endregion

                    #region 현재 단계의 연산 결과(d_i,j) 기록

                    distanceTable[i][j] = new DistanceTable(INVALID_COST, new List<KeyValuePair<List<int>, int>>(), INVALID_COST,
                        oldDistanceTable[i][j].finalDistance, oldDistanceTable[i][j].finalPath);
                    //Console.WriteLine();
                    //Console.WriteLine();
                    //Console.WriteLine("costs.Count = " + costs.Count);

                    // 최솟값을 가지는 모든 c들(argmin)을 구해서 하나의 DistanceTable로 만든다.
                    // 최솟값은 iDistance로 저장되고, 각 경로들은 Dictionary로 묶여서 iPaths로 저장된다. 
                    foreach (DistanceTable c in costs)
                    {
                        // Overflow means there is an invalid operation.

                        if (c.intermediateDistance <= oldDistanceTable[i][j].fixedDistance &&
                            c.fixedDistance >= 0 &&
                            c.fixedDistance < INVALID_COST)
                        {
                            distanceTable[i][j].fixedPaths.Add(c.fixedPaths[0]);
                            distanceTable[i][j].intermediateDistance = c.intermediateDistance;
                            distanceTable[i][j].finalDistance = c.finalDistance;
                            distanceTable[i][j].finalPath = c.finalPath;
                            distanceTable[i][j].lastOperationCost = c.lastOperationCost;
                        }

                        if (i == lenThis && j == lenOther &&
                            c.finalDistance < distanceTable[i][j].finalDistance &&
                            c.finalDistance >= 0)
                        {
                            distanceTable[i][j].fixedDistance = c.fixedDistance;
                            distanceTable[i][j].fixedPaths = c.fixedPaths;
                            distanceTable[i][j].finalDistance = c.finalDistance;
                            distanceTable[i][j].finalPath = c.finalPath;
                            distanceTable[i][j].lastOperationCost = c.lastOperationCost;
                        }
                    }
                    Console.Write(distanceTable[i][j].intermediateDistance + "\t");  // TODO
                    #endregion
                }
                Console.WriteLine();                                // TODO
            }

            //Console.WriteLine("final distance: " + distanceTable[lenThis][lenOther].finalDistance);

            LinkedList<OperationInfo> resultPath = new LinkedList<OperationInfo>();

            int i2 = 0, j2 = 0;
            for (int k = 0; k < distanceTable[lenThis][lenOther].finalPath.Count; k++)
            {
                int op = distanceTable[lenThis][lenOther].finalPath[k];
                Console.Write(op + " ");

                bool prevOpSign = true; // (k - 1)번째 연산의 방향이 정방향이면 true, 역방향이면 false
                if (k > 0) prevOpSign = distanceTable[lenThis][lenOther].finalPath[k - 1] > 0;

                OperationInfo info = new OperationInfo(op,
                    new RhythmPatternNote(), new RhythmPatternNote());
                switch (op)
                {
                    case 1:     // forward delete
                        i2++;
                        info.noteBeforeOp = this.GetNoteByIndex(i2 - 1);
                        if (k > 0)
                        {
                            if (prevOpSign)
                                resultPath.Last.Value.cost = distanceTable[i2][j2].lastOperationCost;
                            else
                                resultPath.First.Value.cost = distanceTable[i2][j2].lastOperationCost;
                        }
                        resultPath.AddLast(info);
                        break;
                    case -1:    // backward delete
                        i2++;
                        info.noteBeforeOp = this.GetNoteByIndex(i2 - 1);
                        if (k > 0)
                        {
                            if (prevOpSign)
                                resultPath.Last.Value.cost = distanceTable[i2][j2].lastOperationCost;
                            else
                                resultPath.First.Value.cost = distanceTable[i2][j2].lastOperationCost;
                        }
                        resultPath.AddFirst(info);
                        break;
                    case 2:     // forward insert
                        j2++;
                        info.noteAfterOp = other.GetNoteByIndex(j2 - 1);
                        if (k > 0)
                        {
                            if (prevOpSign)
                                resultPath.Last.Value.cost = distanceTable[i2][j2].lastOperationCost;
                            else
                                resultPath.First.Value.cost = distanceTable[i2][j2].lastOperationCost;
                        }
                        resultPath.AddLast(info);
                        break;
                    case -2:    // backward insert
                        j2++;
                        info.noteAfterOp = other.GetNoteByIndex(j2 - 1);
                        if (k > 0)
                        {
                            if (prevOpSign)
                                resultPath.Last.Value.cost = distanceTable[i2][j2].lastOperationCost;
                            else
                                resultPath.First.Value.cost = distanceTable[i2][j2].lastOperationCost;
                        }
                        resultPath.AddFirst(info);
                        break;
                    case 3:     // forward move
                        i2++;
                        j2++;
                        info.noteBeforeOp = this.GetNoteByIndex(i2 - 1);
                        info.noteAfterOp = other.GetNoteByIndex(j2 - 1);
                        if (k > 0)
                        {
                            if (prevOpSign)
                                resultPath.Last.Value.cost = distanceTable[i2][j2].lastOperationCost;
                            else
                                resultPath.First.Value.cost = distanceTable[i2][j2].lastOperationCost;
                        }
                        resultPath.AddLast(info);
                        break;
                    case -3:    // backward move
                        i2++;
                        j2++;
                        info.noteBeforeOp = this.GetNoteByIndex(i2 - 1);
                        info.noteAfterOp = other.GetNoteByIndex(j2 - 1);
                        if (k > 0)
                        {
                            if (prevOpSign)
                                resultPath.Last.Value.cost = distanceTable[i2][j2].lastOperationCost;
                            else
                                resultPath.First.Value.cost = distanceTable[i2][j2].lastOperationCost;
                        }
                        resultPath.AddFirst(info);
                        break;
                }
            }
            if (distanceTable[lenThis][lenOther].finalPath.Last() > 0)
            {
                resultPath.Last.Value.cost = distanceTable[lenThis][lenOther].lastOperationCost;
            }
            else
            {
                resultPath.First.Value.cost = distanceTable[lenThis][lenOther].lastOperationCost;
            }
            Console.WriteLine();

            RhythmPattern rpForTest = this.Copy();
            foreach (OperationInfo o in resultPath)
            {
                o.Print();
                switch (o.type)
                {
                    case OperationInfo.Type.Delete:
                        Console.WriteLine("d: " + rpForTest.DeleteNote(o.noteBeforeOp));
                        break;
                    case OperationInfo.Type.Insert:
                        Console.WriteLine("i: " + rpForTest.InsertNote(o.noteAfterOp));
                        break;
                    case OperationInfo.Type.Move:
                        Console.WriteLine("m: " + rpForTest.MoveNote(o.noteBeforeOp, o.noteAfterOp));
                        break;
                }
            }
            rpForTest.Print();

            return distanceTable[lenThis][lenOther].finalDistance;
        }

        /// <summary>
        /// 이 리듬 패턴을 새로 복제하여 반환합니다.
        /// </summary>
        /// <returns></returns>
        public RhythmPattern Copy()
        {
            return Copy(this, noteList.Count - 1);
        }

        /// <summary>
        /// original 리듬 패턴에서, 앞에서부터 index번째 음표까지 포함하는
        /// 길이 index + 1의 부분 리듬 패턴을 새로 복제하여 반환합니다.
        /// </summary>
        private static RhythmPattern Copy(RhythmPattern original, int index)
        {
            RhythmPattern rp = new RhythmPattern();
            int i = 0;
            LinkedListNode<RhythmPatternNote> node = original.noteList.First;
            while (i <= index && node != null)
            {
                rp.InsertNote(node.Value.Copy());
                node = node.Next;
                i++;
            }
            return rp;
        }

        /// <summary>
        /// 음표 목록에서 인덱스로 음표를 찾습니다.
        /// 찾는 음표가 없으면 존재하지 않는 음표를 나타내는
        /// 더미 인스턴스를 반환합니다.
        /// </summary>
        /// <param name="index">인덱스</param>
        /// <returns></returns>
        private RhythmPatternNote GetNoteByIndex(int index)
        {
            if (index < 0) return new RhythmPatternNote();
            int i = 0;
            LinkedListNode<RhythmPatternNote> node = noteList.First;
            while (i < index && node != null)
            {
                node = node.Next;
                i++;
            }
            if (node == null) return new RhythmPatternNote();
            return node.Value;
        }

        /*
        private List<float> GetPitchClusterList()
        {
            List<float> ret = new List<float>();
            foreach (RhythmPatternNote n in noteList)
            {
                if (!float.IsNaN(n.PitchCluster))
                    ret.Add(n.PitchCluster);
            }
            ret = ret.Distinct().ToList();
            ret.Sort();
            return ret;
        }
        */

        /// <summary>
        /// 리듬 패턴 거리 계산 시 이전의 최적 연산 비용
        /// 계산 정보를 저장하는 구조체입니다.
        /// 원래 리듬 패턴의 맨 앞에서부터 i개의 음표를
        /// 목표 리듬 패턴의 맨 앞에서부터 j개의 음표로 바꾸는
        /// 비용과 관련된 정보 d_i,j를 표현합니다.
        /// </summary>
        private class DistanceTable
        {
            /// <summary>
            /// 다음 연산 비용을 계산할 때 필요한 연산 비용 합의 최적.
            /// 연산을 적용하는 순간에 직후 음표가 존재하는 모든 음표에 대한
            /// 연산들의 비용의 합입니다.
            /// 따라서 한 번 최적 값이 계산된 후에는 절대로 바뀔 일이 없습니다.
            /// </summary>
            public int fixedDistance { get; set; }

            /// <summary>
            /// (다음 연산 비용을 계산할 때 필요한 최적 경로 후보, 이 경로의 fixedDistance)들의 목록.
            /// 알려진 최적 비용보다 작은 intermediateDistance를 갖는 서로 다른 경로를 모두 보관합니다.
            /// 최적 경로 후보는 지금까지 적용한 연산 종류와 순서를 나타낸 int 목록인데, 목록 안의 값은
            /// 정방향 Delete이면 1, 정방향 Insert이면 2, 정방향 Move이면 3,
            /// 역방향 Delete이면 -1, 역방향 Insert이면 -2, 역방향 Move이면 -3을 가집니다.
            /// </summary>
            public List<KeyValuePair<List<int>, int>> fixedPaths { get; set; }
            // 최적 경로(int 목록)의 첫 번째 값은 항상 양수 (첫 번째 연산이라 순서를 정의할 수 없음)
            // 정방향: 이전 연산들을 먼저 모두 수행한 후에 마지막 음표의 연산을 수행하는 순서
            // 역방향: 마지막 음표의 연산을 가장 먼저 수행한 후에 이전 연산들을 모두 수행하는 순서

            /// <summary>
            /// 최종 거리.
            /// 목표 리듬 패턴에 도달하기까지 필요한 연산 비용 합의 최적.
            /// 모든 연산들(연산을 적용하는 순간에 직후 음표가 존재하지 않는 음표에 대한
            /// 연산 포함)의 비용의 합입니다.
            /// </summary>
            public int finalDistance { get; set; }

            /// <summary>
            /// 최종 경로.
            /// 목표 리듬 패턴에 도달하기까지 필요한 최적 경로(연산 종류와 순서).
            /// 목록 안의 값은 정방향 Delete이면 1, 정방향 Insert이면 2, 정방향 Move이면 3,
            /// 역방향 Delete이면 -1, 역방향 Insert이면 -2, 역방향 Move이면 -3을 가집니다.
            /// </summary>
            public List<int> finalPath { get; set; }

            /// <summary>
            /// 연산 과정에서 필요한 중간 거리.
            /// 연산을 적용하는 순간에 직후 음표가 존재하는지와 무관하게
            /// d_i,j의 마지막 음표를 제외한 모든 음표에 적용하는 연산 비용의 합의 최적입니다.
            /// </summary>
            public int intermediateDistance { get; set; }

            /// <summary>
            /// d_i,j의 마지막 음표에 대해 수행한 연산의 비용
            /// </summary>
            public int lastOperationCost = INVALID_COST;

            /// <summary>
            /// d_i,j의 마지막 음표에 대해 수행한 연산이
            /// 역방향 Delete이거나 정방향 Insert이면 true,
            /// 그 외의 연산이면 false를 가집니다.
            /// </summary>
            public bool isLastOpBDOrFI = false;

            /// <summary>
            /// 최종 결과를 저장할 때 사용합니다.
            /// 특히 정방향 insert와 역방향 delete를 고려하지 않을 때 사용합니다.
            /// </summary>
            /// <param name="fixedDistance"></param>
            /// <param name="fixedPaths"></param>
            /// <param name="finalDistance"></param>
            /// <param name="finalPath"></param>
            public DistanceTable(int fixedDistance, List<KeyValuePair<List<int>, int>> fixedPaths,
                int finalDistance, List<int> finalPath,
                int lastOperationCost = INVALID_COST, bool isLastOpBDOrFI = false)
            {
                this.fixedDistance = fixedDistance;
                this.fixedPaths = fixedPaths;
                this.intermediateDistance = fixedDistance;
                this.finalDistance = finalDistance;
                this.finalPath = finalPath;
                this.lastOperationCost = lastOperationCost;
                this.isLastOpBDOrFI = isLastOpBDOrFI;
            }

            /// <summary>
            /// 최종 결과를 저장할 때 사용합니다.
            /// 특히 모든 연산을 고려할 때 사용합니다.
            /// </summary>
            /// <param name="fixedDistance"></param>
            /// <param name="fixedPaths"></param>
            /// <param name="finalDistance"></param>
            /// <param name="finalPath"></param>
            public DistanceTable(int fixedDistance, List<KeyValuePair<List<int>, int>> fixedPaths,
                int intermediateDistance,
                int finalDistance, List<int> finalPath,
                int lastOperationCost = INVALID_COST, bool isLastOpBDOrFI = false)
            {
                this.fixedDistance = fixedDistance;
                this.fixedPaths = fixedPaths;
                this.intermediateDistance = intermediateDistance;
                this.finalDistance = finalDistance;
                this.finalPath = finalPath;
                this.lastOperationCost = lastOperationCost;
                this.isLastOpBDOrFI = isLastOpBDOrFI;
            }
        }

        /// <summary>
        /// 리듬 패턴 편집 연산의 종류와
        /// 연산을 적용한 음표에 대한 정보를 담는 구조체입니다.
        /// </summary>
        public class OperationInfo
        {
            public enum Type { Invalid = 0, Delete = 1, Insert = 2, Move = 3 }

            /// <summary>
            /// 편집 연산 종류
            /// </summary>
            public Type type;

            /// <summary>
            /// 연산을 적용한 음표의 연산 직전 상태.
            /// Insert 연산에서는 없는 음표를 나타내는 new RhythmPatternNote()가 됩니다.
            /// </summary>
            public RhythmPatternNote noteBeforeOp;

            /// <summary>
            /// 연산을 적용한 음표의 연산 직후 상태.
            /// Delete 연산에서는 없는 음표를 나타내는 new RhythmPatternNote()가 됩니다.
            /// </summary>
            public RhythmPatternNote noteAfterOp;

            /// <summary>
            /// 편집 연산을 수행한 비용.
            /// 이 구조체의 다른 필드의 값이 모두 같아도 
            /// 어떤 리듬 패턴에서 연산을 적용했는지에 따라 비용이 다를 수 있습니다.
            /// </summary>
            public int cost;

            public OperationInfo(Type operationType,
                RhythmPatternNote noteBeforeOp, RhythmPatternNote noteAfterOp,
                int cost = INVALID_COST)
            {
                switch (operationType)
                {
                    case Type.Delete:
                        this.type = Type.Delete;
                        this.noteBeforeOp = noteBeforeOp.Copy();
                        this.noteAfterOp = new RhythmPatternNote();
                        this.cost = cost;
                        break;
                    case Type.Insert:
                        this.type = Type.Insert;
                        this.noteBeforeOp = new RhythmPatternNote();
                        this.noteAfterOp = noteAfterOp.Copy();
                        this.cost = cost;
                        break;
                    case Type.Move:
                        this.type = Type.Move;
                        this.noteBeforeOp = noteBeforeOp.Copy();
                        this.noteAfterOp = noteAfterOp.Copy();
                        this.cost = cost;
                        break;
                    default:
                        this.type = Type.Invalid;
                        this.noteBeforeOp = new RhythmPatternNote();
                        this.noteAfterOp = new RhythmPatternNote();
                        this.cost = INVALID_COST;
                        break;
                }
            }

            public OperationInfo(string operationType,
                RhythmPatternNote noteBeforeOp, RhythmPatternNote noteAfterOp,
                int cost = INVALID_COST)
            {
                switch (operationType.ToLowerInvariant())
                {
                    case "delete":
                        this.type = Type.Delete;
                        this.noteBeforeOp = noteBeforeOp.Copy();
                        this.noteAfterOp = new RhythmPatternNote();
                        this.cost = cost;
                        break;
                    case "insert":
                        this.type = Type.Insert;
                        this.noteBeforeOp = new RhythmPatternNote();
                        this.noteAfterOp = noteAfterOp.Copy();
                        this.cost = cost;
                        break;
                    case "move":
                        this.type = Type.Move;
                        this.noteBeforeOp = noteBeforeOp.Copy();
                        this.noteAfterOp = noteAfterOp.Copy();
                        this.cost = cost;
                        break;
                    default:
                        this.type = Type.Invalid;
                        this.noteBeforeOp = new RhythmPatternNote();
                        this.noteAfterOp = new RhythmPatternNote();
                        this.cost = INVALID_COST;
                        break;
                }
            }

            public OperationInfo(int operationCode,
                RhythmPatternNote noteBeforeOp, RhythmPatternNote noteAfterOp,
                int cost = INVALID_COST)
            {
                switch (operationCode)
                {
                    case 1:
                    case -1:
                        this.type = Type.Delete;
                        this.noteBeforeOp = noteBeforeOp.Copy();
                        this.noteAfterOp = new RhythmPatternNote();
                        this.cost = cost;
                        break;
                    case 2:
                    case -2:
                        this.type = Type.Insert;
                        this.noteBeforeOp = new RhythmPatternNote();
                        this.noteAfterOp = noteAfterOp.Copy();
                        this.cost = cost;
                        break;
                    case 3:
                    case -3:
                        this.type = Type.Move;
                        this.noteBeforeOp = noteBeforeOp.Copy();
                        this.noteAfterOp = noteAfterOp.Copy();
                        this.cost = cost;
                        break;
                    default:
                        this.type = Type.Invalid;
                        this.noteBeforeOp = new RhythmPatternNote();
                        this.noteAfterOp = new RhythmPatternNote();
                        this.cost = INVALID_COST;
                        break;
                }
            }

            /// <summary>
            /// 이 연산의 역연산을 반환합니다.
            /// </summary>
            /// <returns></returns>
            public OperationInfo Inverse()
            {
                switch (this.type)
                {
                    case Type.Delete:
                        return new OperationInfo(Type.Insert, new RhythmPatternNote(), this.noteBeforeOp, this.cost);
                    case Type.Insert:
                        return new OperationInfo(Type.Delete, this.noteAfterOp, new RhythmPatternNote(), this.cost);
                    case Type.Move:
                        return new OperationInfo(Type.Move, this.noteAfterOp, this.noteBeforeOp, this.cost);
                    default:
                        return new OperationInfo(Type.Invalid, new RhythmPatternNote(), new RhythmPatternNote());
                }
            }

            /// <summary>
            /// 이 연산을 콘솔에 보기 좋게 출력합니다.
            /// </summary>
            public void Print(bool printCost = true)
            {
                string s = type.ToString();
                if (type == Type.Invalid)
                {
                    s += "()";
                    if (printCost) s += ": " + INVALID_COST;
                    Console.WriteLine(s);
                    return;
                }
                s += "([";
                if (noteBeforeOp.OnsetPosition != -1)
                {
                    s += noteBeforeOp.OnsetPosition + ", " + noteBeforeOp.PitchCluster;
                    if (noteAfterOp.OnsetPosition != -1)
                    {
                        s += "] -> [";
                        s += noteAfterOp.OnsetPosition + ", " + noteAfterOp.PitchCluster;
                        s += "])";
                        if (printCost) s += ": " + cost;
                        Console.WriteLine(s);
                        return;
                    }
                    else
                    {
                        s += "])";
                        if (printCost) s += ": " + cost;
                        Console.WriteLine(s);
                        return;
                    }
                }
                else
                {
                    s += noteAfterOp.OnsetPosition + ", " + noteAfterOp.PitchCluster;
                    s += "])";
                    if (printCost) s += ": " + cost;
                    Console.WriteLine(s);
                    return;
                }
            }
        }
    }

    /// <summary>
    /// 음표.
    /// 리듬 패턴의 음표 목록에 들어갈 구조체입니다.
    /// 음표의 시작 위치 정보와 음 높이 변화 정보를 포함합니다.
    /// </summary>
    public class RhythmPatternNote : IComparable<RhythmPatternNote>
    {
        /// <summary>
        /// 음표의 시작 위치.
        /// 한 마디를 32분음표 32개로 쪼갰을 때
        /// 앞에서부터 몇 번째 위치에 음표의 시작이
        /// 나타나는지를 표현합니다. 0부터 시작합니다.
        /// </summary>
        public int OnsetPosition
        {
            get;
            private set;
        }

        /// <summary>
        /// 클러스터 번호.
        /// 같은 음 높이를 갖는 음표끼리는 같은 클러스터 번호를 가지며
        /// 클러스터 번호가 높으면 항상 절대적인 음 높이가 더 높습니다.
        /// 클러스터 번호의 절대적인 값은 의미를 갖지 않습니다.
        /// </summary>
        public float PitchCluster
        {
            get;
            private set;
        }

        /// <summary>
        /// 음 높이 변화.
        /// RhythmPattern.PitchVariance()의 값을 임시로 저장하기
        /// 위한 변수이므로 이 값에 직접 접근하면 안 됩니다.
        /// 대신 RhythmPattern.PitchVariance()을 호출하십시오.
        /// </summary>
        public int pitchVariance;

        /// <summary>
        /// 리듬 패턴에 들어갈 음표를 생성합니다.
        /// </summary>
        /// <param name="onsetPosition">음표의 시작 위치.
        /// 한 마디를 32분음표 32개로 쪼갰을 때
        /// 앞에서부터 몇 번째 위치에 음표의 시작이 나타나는지를 표현합니다.
        /// 0부터 시작합니다.</param>
        /// <param name="pitchCluster">클러스터 번호.
        /// 같은 음 높이를 갖는 음표끼리는 같은 클러스터 번호를 가지며
        /// 클러스터 번호가 높으면 항상 절대적인 음 높이가 더 높습니다.
        /// 클러스터 번호의 절대적인 값은 의미를 갖지 않습니다.
        /// 클러스터 번호를 정할 때
        /// RhythmPattern.GetNewClusterNumber()를 활용하면 도움이 됩니다.</param>
        public RhythmPatternNote(int onsetPosition, float pitchCluster)
        {
            if (onsetPosition >= 0)
                OnsetPosition = onsetPosition;
            else
                OnsetPosition = 0;
            PitchCluster = pitchCluster;
            pitchVariance = -2;
        }

        /// <summary>
        /// 리듬 패턴에 존재하지 않는 음표를 표현하는 더미 인스턴스를 생성합니다.
        /// 존재하지 않는 음표의 시작 위치는 -1로, 클러스터 번호는 float.NaN으로,
        /// 음 높이 변화는 0으로 표현됩니다.
        /// </summary>
        public RhythmPatternNote()
        {
            OnsetPosition = -1;
            PitchCluster = float.NaN;
            pitchVariance = 0;
        }

        /// <summary>
        /// 리듬 패턴 음표를 새로 복제하여 반환합니다.
        /// </summary>
        /// <returns></returns>
        public RhythmPatternNote Copy()
        {
            return new RhythmPatternNote(OnsetPosition, PitchCluster);
        }

        /// <summary>
        /// OnsetPosition을 기준으로 다른 리듬 패턴 음표와 비교합니다.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(RhythmPatternNote other)
        {
            return this.OnsetPosition.CompareTo(other.OnsetPosition);
        }

        /// <summary>
        /// 두 리듬 패턴 음표가 같은지 비교합니다.
        /// OnsetPosition과 PitchCluster의 절대적인 값이 같아야 같은 음표입니다.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(RhythmPatternNote))
            {
                if (this.OnsetPosition == ((RhythmPatternNote)obj).OnsetPosition &&
                    this.PitchCluster == ((RhythmPatternNote)obj).PitchCluster)
                    return true;
                else return false;
            }
            else return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return 31 * OnsetPosition.GetHashCode() + PitchCluster.GetHashCode();
        }

        public override string ToString()
        {
            return "[" + OnsetPosition + ", " + PitchCluster + "]";
        }
    }

    /// <summary>
    /// 메타데이터.
    /// 리듬 패턴의 클러스터 정보와
    /// 이 클러스터에 할당된 절대적인 음 높이 정보를 포함합니다.
    /// 리듬 패턴을 악보로 구체화(implement)할 때 사용됩니다.
    /// </summary>
    public class RhythmPatternMetadata : IComparable<RhythmPatternMetadata>
    {
        /// <summary>
        /// 클러스터 번호
        /// </summary>
        public float pitchCluster;

        /// <summary>
        /// 이 클러스터 번호에 속해 있는 음표들의 시작 위치 목록
        /// </summary>
        public List<int> noteOnsets = new List<int>();

        /// <summary>
        /// 이 클러스터에 할당된 절대적인 음 높이.
        /// RhythmPattern.SetAbsolutePitch()로 값을 지정할 수 있습니다.
        /// 지정되지 않은 경우 -1을 가지고 있습니다.
        /// </summary>
        public int absolutePitch;

        public RhythmPatternMetadata(float pitchCluster, params int[] noteOnsets)
        {
            this.pitchCluster = pitchCluster;
            this.noteOnsets = noteOnsets.ToList();
            absolutePitch = -1;
        }

        public int CompareTo(RhythmPatternMetadata other)
        {
            return this.pitchCluster.CompareTo(other.pitchCluster);
        }
    }
}
