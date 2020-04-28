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
        /// 같은 OnsetPosition에 여러 음표를 삽입하려 할 경우 삽입 연산이 수행되지 않고 int.MaxValue를 반환합니다.
        /// (새 음표를 정의할 때 GetNewClusterNumber() 또는
        /// GetExistingClusterNumber()를 사용하면 편리합니다.)
        /// </summary>
        /// <param name="note">삽입할 새 음표</param>
        /// <returns></returns>
        public int InsertNote(RhythmPatternNote note)
        {
            if (note == null) return int.MaxValue;

            LinkedListNode<RhythmPatternNote> afterNote = null;
            foreach (RhythmPatternNote n in noteList)
            {
                // 같은 위치에 여러 음표를 삽입할 수 없음 (단선율만 허용)
                if (n.OnsetPosition == note.OnsetPosition) return int.MaxValue;
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
        /// 같은 OnsetPosition에 여러 음표를 삽입하려 할 경우 삽입 연산이 수행되지 않고 int.MaxValue를 반환합니다.
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
        /// 존재하지 않는 음표를 제거하려 할 경우 제거 연산이 수행되지 않고 int.MaxValue를 반환합니다.
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
            if (n == null) return int.MaxValue;

            RhythmPatternNote note = n.Value;

            // 메타데이터 편집
            RhythmPatternMetadata m = metadataList.Find(e => e.pitchCluster == note.PitchCluster);
            if (m != null)
            {
                if (!m.noteOnsets.Remove(note.OnsetPosition))
                {
                    Console.WriteLine("Error: RhythmPattern DeleteNote metadata 1");
                    return int.MaxValue;
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
                return int.MaxValue;
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
        /// 리듬 패턴에서 마지막 음표 하나를 제거하는 연산을 수행합니다.
        /// 반환값은 수행한 연산의 비용입니다.
        /// 빈 악보에서 이 연산을 수행하려고 하면 제거 연산이 수행되지 않고 int.MaxValue를 반환합니다.
        /// </summary>
        /// <returns></returns>
        public int DeleteLastNote()
        {
            if (noteList.Last == null) return int.MaxValue;
            return DeleteNote(noteList.Last.Value.OnsetPosition);
        }

        /// <summary>
        /// 리듬 패턴에 있던 음표 하나의 인덱스(음표 목록에서의 상대적 위치)를 유지하면서
        /// OnsetPosition과 클러스터를 옮기는 연산을 수행합니다.
        /// 반환값은 수행한 연산의 비용입니다.
        /// 존재하지 않는 음표를 옮기려고 하거나 새 OnsetPosition이 음표의 인덱스에 영향을 미칠 경우
        /// 옮기는 연산을 수행하지 않고 int.MaxValue를 반환합니다.
        /// (새 음표를 정의할 때 GetNewClusterNumber() 또는
        /// GetExistingClusterNumber()를 사용하면 편리합니다.)
        /// </summary>
        /// <param name="oldNoteOnset">옮기는 대상이 될 기존 음표의 시작 위치</param>
        /// <param name="newNote">옮겨질 새 음표</param>
        /// <returns></returns>
        public int MoveNote(int oldNoteOnset, RhythmPatternNote newNote)
        {
            if (newNote == null) return int.MaxValue;
            LinkedListNode<RhythmPatternNote> n = null;
            foreach (RhythmPatternNote note in noteList)
            {
                if (note.OnsetPosition == oldNoteOnset)
                {
                    n = noteList.Find(note);
                    break;
                }
            }
            if (n == null) return int.MaxValue;

            RhythmPatternNote oldNote = n.Value;

            if ((n.Previous != null &&
                newNote.OnsetPosition <= n.Previous.Value.OnsetPosition) ||
                (n.Next != null &&
                newNote.OnsetPosition >= n.Next.Value.OnsetPosition))
            {
                // 옮긴 결과로 음표의 인덱스가 바뀌는 경우 연산을 수행하지 않음
                return int.MaxValue;
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
                        return int.MaxValue;
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
                    return int.MaxValue;
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
        /// 옮기는 연산을 수행하지 않고 int.MaxValue를 반환합니다.
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
        /// 리듬 패턴에 있던 마지막 음표 하나의 인덱스(음표 목록에서의 상대적 위치)를 유지하면서
        /// OnsetPosition과 클러스터를 옮기는 연산을 수행합니다.
        /// 반환값은 수행한 연산의 비용입니다.
        /// 빈 악보에서 이 연산을 수행하려고 하거나 새 OnsetPosition이 음표의 인덱스에 영향을 미칠 경우
        /// 옮기는 연산을 수행하지 않고 int.MaxValue를 반환합니다.
        /// (새 음표를 정의할 때 GetNewClusterNumber() 또는
        /// GetExistingClusterNumber()를 사용하면 편리합니다.)
        /// </summary>
        /// <param name="newNoteOnset">옮겨질 새 음표의 시작 위치</param>
        /// <param name="newNotePitchCluster">옮겨질 새 음표의 음 높이 클러스터 번호</param>
        /// <returns></returns>
        public int MoveLastNote(int newNoteOnset, float newNotePitchCluster)
        {
            if (noteList.Last == null) return int.MaxValue;
            return MoveNote(noteList.Last.Value.OnsetPosition, newNoteOnset, newNotePitchCluster);
        }

        /// <summary>
        /// 리듬 패턴에 있던 마지막 음표 하나의 인덱스(음표 목록에서의 상대적 위치)를 유지하면서
        /// OnsetPosition과 클러스터를 옮기는 연산을 수행합니다.
        /// 반환값은 수행한 연산의 비용입니다.
        /// 빈 악보에서 이 연산을 수행하려고 하거나 새 OnsetPosition이 음표의 인덱스에 영향을 미칠 경우
        /// 옮기는 연산을 수행하지 않고 int.MaxValue를 반환합니다.
        /// (새 음표를 정의할 때 GetNewClusterNumber() 또는
        /// GetExistingClusterNumber()를 사용하면 편리합니다.)
        /// </summary>
        /// <param name="newNote">옮겨질 새 음표</param>
        /// <returns></returns>
        public int MoveLastNote(RhythmPatternNote newNote)
        {
            if (noteList.Last == null) return int.MaxValue;
            return MoveNote(noteList.Last.Value.OnsetPosition, newNote);
        }

        public void Print()
        {
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
                Console.WriteLine();
            }
        }

        public int Distance(RhythmPattern other)
        {
            // Dynamic programming

            int lenThis = this.noteList.Count;
            int lenOther = other.noteList.Count;
            List<List<KeyValuePair<int, string>>> distanceTable = new List<List<KeyValuePair<int, string>>>(lenThis + 1);
            for (int i = 0; i <= lenThis; i++)
            {
                List<KeyValuePair<int, string>> temp = new List<KeyValuePair<int, string>>(lenOther + 1);
                for (int j = 0; j <= lenOther; j++)
                {
                    temp.Add(new KeyValuePair<int, string>(int.MaxValue, ""));
                }
                distanceTable.Add(temp);

                for (int j = 0; j <= lenOther; j++)
                {
                    if (i == 0 && j == 0)
                    {
                        distanceTable[i][j] = new KeyValuePair<int, string>(0, "");
                        Console.Write(distanceTable[i][j].Key + "\t");
                        continue;
                    }

                    List<KeyValuePair<int, string>> costs = new List<KeyValuePair<int, string>>();
                    if (i > 0) costs.Add(new KeyValuePair<int, string>(
                        distanceTable[i - 1][j].Key + Copy(this, i - 1).DeleteLastNote(),
                        "d(" + this.GetNoteByIndex(i - 1).ToString() + ")"));  // delete
                    if (j > 0) costs.Add(new KeyValuePair<int, string>(
                        distanceTable[i][j - 1].Key + Copy(other, j - 2).InsertNote(other.GetNoteByIndex(j - 1)),
                        "i(" + other.GetNoteByIndex(j - 1).ToString() + ")"));  // insert
                    if (i > 0 && j > 0) costs.Add(new KeyValuePair<int, string>(
                        distanceTable[i - 1][j - 1].Key + Copy(this, i - 1).MoveLastNote(other.GetNoteByIndex(j - 1)),
                        "m(" + this.GetNoteByIndex(i - 1).ToString() + " -> " + other.GetNoteByIndex(j - 1).ToString() + ")")); // move
                    foreach (KeyValuePair<int, string> c in costs)
                    {
                        // Overflow means there is an invalid operation.
                        if (c.Key < distanceTable[i][j].Key && c.Key >= 0)
                        {
                            distanceTable[i][j] = c;
                        }
                    }
                    Console.Write(distanceTable[i][j].Key + "\t");
                }
                Console.WriteLine();
            }

            // Print the shortest path.
            for (int i = lenThis, j = lenOther; i > 0 || j > 0;)
            {
                string s = distanceTable[i][j].Value;
                Console.Write(s + ": ");
                if (s.Length > 0)
                {
                    if (s[0] == 'd')
                    {
                        Console.Write(distanceTable[i][j].Key - distanceTable[i - 1][j].Key);
                        i--;
                    }
                    else if (s[0] == 'i')
                    {
                        Console.Write(distanceTable[i][j].Key - distanceTable[i][j - 1].Key);
                        j--;
                    }
                    else // (s[0] == 'm')
                    {
                        Console.Write(distanceTable[i][j].Key - distanceTable[i - 1][j - 1].Key);
                        i--;
                        j--;
                    }
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine(distanceTable[i][j].Key);
                    break;
                }
            }

            return distanceTable[lenThis][lenOther].Key;
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
                rp.InsertNote(node.Value);
                node = node.Next;
                i++;
            }
            return rp;
        }

        /// <summary>
        /// 음표 목록에서 인덱스로 음표를 찾습니다.
        /// </summary>
        /// <param name="index">인덱스</param>
        /// <returns></returns>
        private RhythmPatternNote GetNoteByIndex(int index)
        {
            int i = 0;
            LinkedListNode<RhythmPatternNote> node = noteList.First;
            while (i < index && node != null)
            {
                node = node.Next;
                i++;
            }
            if (node == null) return null;
            return node.Value;
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
