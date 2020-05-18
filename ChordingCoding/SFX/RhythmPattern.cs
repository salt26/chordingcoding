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
        /// 같은 OnsetPosition에 여러 음표를 삽입하려 할 경우 삽입 연산이 수행되지 않고 int.MaxValue / 2를 반환합니다.
        /// (새 음표를 정의할 때 GetNewClusterNumber() 또는
        /// GetExistingClusterNumber()를 사용하면 편리합니다.)
        /// </summary>
        /// <param name="note">삽입할 새 음표</param>
        /// <returns></returns>
        public int InsertNote(RhythmPatternNote note)
        {
            if (note == null) return int.MaxValue / 2;
            note = note.Copy();

            LinkedListNode<RhythmPatternNote> afterNote = null;
            foreach (RhythmPatternNote n in noteList)
            {
                // 같은 위치에 여러 음표를 삽입할 수 없음 (단선율만 허용)
                if (n.OnsetPosition == note.OnsetPosition) return int.MaxValue / 2;
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
        /// 같은 OnsetPosition에 여러 음표를 삽입하려 할 경우 삽입 연산이 수행되지 않고 int.MaxValue / 2를 반환합니다.
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
        /// 존재하지 않는 음표를 제거하려 할 경우 제거 연산이 수행되지 않고 int.MaxValue / 2를 반환합니다.
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
            if (n == null) return int.MaxValue / 2;

            RhythmPatternNote note = n.Value;

            // 메타데이터 편집
            RhythmPatternMetadata m = metadataList.Find(e => e.pitchCluster == note.PitchCluster);
            if (m != null)
            {
                if (!m.noteOnsets.Remove(note.OnsetPosition))
                {
                    Console.WriteLine("Error: RhythmPattern DeleteNote metadata 1");
                    return int.MaxValue / 2;
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
                return int.MaxValue / 2;
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
        /// 빈 리듬 패턴에서 이 연산을 수행하려고 하면 제거 연산이 수행되지 않고 int.MaxValue / 2를 반환합니다.
        /// </summary>
        /// <returns></returns>
        public int DeleteLastNote()
        {
            if (noteList.Last == null) return int.MaxValue / 2;
            return DeleteNote(noteList.Last.Value.OnsetPosition);
        }

        /// <summary>
        /// 리듬 패턴에 있던 음표 하나의 인덱스(음표 목록에서의 상대적 위치)를 유지하면서
        /// OnsetPosition과 클러스터를 옮기는 연산을 수행합니다.
        /// 반환값은 수행한 연산의 비용입니다.
        /// 존재하지 않는 음표를 옮기려고 하거나 새 OnsetPosition이 음표의 인덱스에 영향을 미칠 경우
        /// 옮기는 연산을 수행하지 않고 int.MaxValue / 2를 반환합니다.
        /// (새 음표를 정의할 때 GetNewClusterNumber() 또는
        /// GetExistingClusterNumber()를 사용하면 편리합니다.)
        /// </summary>
        /// <param name="oldNoteOnset">옮기는 대상이 될 기존 음표의 시작 위치</param>
        /// <param name="newNote">옮겨질 새 음표</param>
        /// <returns></returns>
        public int MoveNote(int oldNoteOnset, RhythmPatternNote newNote)
        {
            if (newNote == null) return int.MaxValue / 2;
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
            if (n == null) return int.MaxValue / 2;

            RhythmPatternNote oldNote = n.Value;

            if ((n.Previous != null &&
                newNote.OnsetPosition <= n.Previous.Value.OnsetPosition) ||
                (n.Next != null &&
                newNote.OnsetPosition >= n.Next.Value.OnsetPosition))
            {
                // 옮긴 결과로 음표의 인덱스가 바뀌는 경우 연산을 수행하지 않음
                return int.MaxValue / 2;
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
                        return int.MaxValue / 2;
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
                    return int.MaxValue / 2;
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
        /// 옮기는 연산을 수행하지 않고 int.MaxValue / 2를 반환합니다.
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
        /// 빈 리듬 패턴에서 이 연산을 수행하려고 하거나 새 OnsetPosition이 음표의 인덱스에 영향을 미칠 경우
        /// 옮기는 연산을 수행하지 않고 int.MaxValue / 2를 반환합니다.
        /// (새 음표를 정의할 때 GetNewClusterNumber() 또는
        /// GetExistingClusterNumber()를 사용하면 편리합니다.)
        /// </summary>
        /// <param name="newNoteOnset">옮겨질 새 음표의 시작 위치</param>
        /// <param name="newNotePitchCluster">옮겨질 새 음표의 음 높이 클러스터 번호</param>
        /// <returns></returns>
        public int MoveLastNote(int newNoteOnset, float newNotePitchCluster)
        {
            if (noteList.Last == null) return int.MaxValue / 2;
            return MoveNote(noteList.Last.Value.OnsetPosition, new RhythmPatternNote(newNoteOnset, newNotePitchCluster));
        }

        /// <summary>
        /// 리듬 패턴에 있던 마지막 음표 하나의 인덱스(음표 목록에서의 상대적 위치)를 유지하면서
        /// OnsetPosition과 클러스터를 옮기는 연산을 수행합니다.
        /// 반환값은 수행한 연산의 비용입니다.
        /// 빈 리듬 패턴에서 이 연산을 수행하려고 하거나 새 OnsetPosition이 음표의 인덱스에 영향을 미칠 경우
        /// 옮기는 연산을 수행하지 않고 int.MaxValue / 2를 반환합니다.
        /// (새 음표를 정의할 때 GetNewClusterNumber() 또는
        /// GetExistingClusterNumber()를 사용하면 편리합니다.)
        /// </summary>
        /// <param name="newNote">옮겨질 새 음표</param>
        /// <returns></returns>
        public int MoveLastNote(RhythmPatternNote newNote)
        {
            if (noteList.Last == null) return int.MaxValue / 2;
            return MoveNote(noteList.Last.Value.OnsetPosition, newNote);
        }

        /// <summary>
        /// 이 리듬 패턴을 보기 좋게 출력합니다.
        /// 만약 'X'가 출력에 포함된다면 오류가 발생한 것입니다.
        /// </summary>
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

            for (int i = 0; i <= lenThis; i++)
            {
                List<DistanceTable> temp =
                    new List<DistanceTable>(lenOther + 1);
                for (int j = 0; j <= lenOther; j++)
                {
                    temp.Add(new DistanceTable(int.MaxValue / 2, null));
                }
                distanceTable.Add(temp);

                for (int j = 0; j <= lenOther; j++)
                {
                    if (i == 0 && j == 0)
                    {
                        var dict = new Dictionary<DistanceEnvironment, List<int>>
                        {
                            // Pitch cluster for non-existing note is float.NaN.
                            { new DistanceEnvironment(float.NaN, new List<float>(), new List<float>()), new List<int>() }
                        };
                        distanceTable[i][j] = new DistanceTable(0, dict);
                        Console.Write(distanceTable[i][j].intermediateDistance + "\t");
                        continue;
                    }

                    List<DistanceTable> costs = new List<DistanceTable>();
                    if (i > 0)  // Delete
                    {
                        int k = i - 1;
                        int l = j;
                        DistanceTable previous = distanceTable[k][l];
                        // forward (정방향)
                        foreach (var path in previous.intermediatePaths) // 이전 단계에서 최적이라고 알려진 모든 경로를 고려
                        {
                            DistanceEnvironment env = path.Key;
                            List<int> operations = new List<int>(path.Value);

                            // 이전 단계(d_k,l)에서 마지막으로 연산된 음표의
                            // 연산 직전(=직후)에 바로 앞에 놓인 음표의 상태
                            RhythmPatternNote cPreviousBeforeOp = new RhythmPatternNote();

                            // 이전 단계(d_k,l)에서 마지막으로 연산된 음표의 연산 직전 상태
                            RhythmPatternNote cBeforeOp = new RhythmPatternNote();

                            // 이전 단계(d_k,l)에서 마지막으로 연산된 음표의 연산 직후 상태
                            RhythmPatternNote cAfterOp = new RhythmPatternNote();

                            // 이전 단계(d_k,l)에서 마지막으로 연산된 음표의
                            // 연산 직전(=직후)에 바로 뒤에 놓인 음표의 상태
                            RhythmPatternNote cNextBeforeOp = new RhythmPatternNote();

                            // 현재 단계(d_i,j)에서 마지막으로 연산될 음표의
                            // 연산 직전(=직후)에 바로 앞에 놓인 음표의 상태
                            RhythmPatternNote dPreviousBeforeOp = new RhythmPatternNote();

                            // 현재 단계(d_i,j)에서 마지막으로 연산될 음표의 연산 직전 상태
                            RhythmPatternNote dBeforeOp = this.GetNoteByIndex(i - 1);

                            // 현재 단계(d_i,j)에서 마지막으로 연산될 음표의 연산 직후 상태
                            RhythmPatternNote dAfterOp = new RhythmPatternNote();

                            // 현재 단계(d_i,j)에서 마지막으로 연산될 음표의
                            // 연산 직전(=직후)에 바로 뒤에 놓인 음표의 상태
                            RhythmPatternNote dNextBeforeOp = new RhythmPatternNote();

                            RhythmPattern rpTemp;

                            if (operations.Count > 0)
                            {
                                int lastOperation = operations.Last();  // 지난 단계에서 마지막으로 수행한 연산
                                switch (lastOperation)
                                {
                                    case 1:     // forward delete
                                        cBeforeOp = this.GetNoteByIndex(k - 1);
                                        rpTemp = Copy(other, l - 1);
                                        if (rpTemp.InsertNote(cBeforeOp) != int.MaxValue / 2)
                                        {
                                            LinkedListNode<RhythmPatternNote> lln = rpTemp.noteList.Find(cBeforeOp).Previous;
                                            if (lln != null) cPreviousBeforeOp = lln.Value;
                                            else cPreviousBeforeOp = new RhythmPatternNote();
                                        }
                                        else
                                        {
                                            // invalid operation performing order
                                            Console.WriteLine("Warning: Invalid operation performing order in Distance()");
                                            cPreviousBeforeOp = new RhythmPatternNote();
                                        }
                                        break;
                                    case -1:    // backward delete
                                        cPreviousBeforeOp = this.GetNoteByIndex(k - 2);
                                        cBeforeOp = this.GetNoteByIndex(k - 1);
                                        break;
                                    case 2:     // forward insert
                                        cPreviousBeforeOp = other.GetNoteByIndex(l - 2);
                                        cAfterOp = other.GetNoteByIndex(l - 1);
                                        break;
                                    case -2:    // backward insert
                                        cAfterOp = other.GetNoteByIndex(l - 1);
                                        rpTemp = Copy(this, k - 1);
                                        if (rpTemp.InsertNote(cAfterOp) != int.MaxValue / 2)
                                        {
                                            LinkedListNode<RhythmPatternNote> lln = rpTemp.noteList.Find(cAfterOp).Previous;
                                            if (lln != null) cPreviousBeforeOp = lln.Value;
                                            else cPreviousBeforeOp = new RhythmPatternNote();
                                        }
                                        else
                                        {
                                            // invalid operation performing order
                                            Console.WriteLine("Warning: Invalid operation performing order in Distance()");
                                            cPreviousBeforeOp = new RhythmPatternNote();
                                        }
                                        break;
                                    case 3:     // forward move
                                        cPreviousBeforeOp = other.GetNoteByIndex(l - 2);
                                        cBeforeOp = this.GetNoteByIndex(k - 1);
                                        cAfterOp = other.GetNoteByIndex(l - 1);
                                        break;
                                    case -3:    // backward move
                                        cPreviousBeforeOp = this.GetNoteByIndex(k - 2);
                                        cBeforeOp = this.GetNoteByIndex(k - 1);
                                        cAfterOp = other.GetNoteByIndex(l - 1);
                                        break;
                                }
                            }

                            // forward delete (current operation)
                            rpTemp = Copy(other, j - 1);
                            if (rpTemp.InsertNote(dBeforeOp) != int.MaxValue / 2)
                            {
                                LinkedListNode<RhythmPatternNote> lln = rpTemp.noteList.Find(dBeforeOp).Previous;
                                if (lln != null) dPreviousBeforeOp = lln.Value;
                                else dPreviousBeforeOp = new RhythmPatternNote();

                                if (!float.IsNaN(env.lastPreviousPitchClusterBeforeOp) &&
                                    !float.IsNaN(cPreviousBeforeOp.PitchCluster) &&
                                    env.lastPreviousPitchClusterBeforeOp != cPreviousBeforeOp.PitchCluster)
                                {
                                    Console.WriteLine("Error: lastPreviousPitchCluster are differ!");
                                }

                                // TODO C?D에 연산 적용 (첫 연산인 경우 안 해도 됨)
                                int cCost = 0;
                                if (operations.Count > 0)
                                {
                                    // TODO dBeforeOp가 cBeforeOp의 바로 뒤에 놓인 음표가 아닐 수 있음
                                    cCost = CalculateCost(cBeforeOp.OnsetPosition, cBeforeOp.PitchCluster,
                                        cPreviousBeforeOp.OnsetPosition, cPreviousBeforeOp.PitchCluster,
                                        dBeforeOp.OnsetPosition, dBeforeOp.PitchCluster,
                                        cAfterOp.OnsetPosition, cAfterOp.PitchCluster,
                                        env.pitchClusterListBeforeOp, env.pitchClusterListAfterOp);
                                }

                                // TODO D'에 연산 적용
                                int dCost = 0;
                                dCost = rpTemp.DeleteNote(dBeforeOp.OnsetPosition);

                                List<float> dPCLBeforeOp = other.GetPitchClusterList();
                                dPCLBeforeOp.Add(dBeforeOp.PitchCluster);
                                List<float> dPCLAfterOp = other.GetPitchClusterList();

                                // TODO dBeforeOp가 맨 마지막 음표가 아닐 수 있음
                                // 목표 리듬 패턴(0 ~ j-1)에 D만 연산 적용 전 음표로 삽입되어 있는 상태에서 D 뒤의 음표를 찾아야 함
                                if (dCost != CalculateCost(dBeforeOp.OnsetPosition, dBeforeOp.PitchCluster,
                                    dPreviousBeforeOp.OnsetPosition, dPreviousBeforeOp.PitchCluster,
                                    _, _,
                                    dAfterOp.OnsetPosition, dAfterOp.PitchCluster,
                                    dPCLBeforeOp, dPCLAfterOp
                                    ))
                                {
                                    Console.WriteLine("Error: The cost of CalculateCost() and DeleteNote() are differ!");
                                }

                                operations.Add(1);  // forward delete operation
                                costs.Add(new DistanceTable(_,
                                    new Dictionary<DistanceEnvironment, List<int>> {
                                    { new DistanceEnvironment(_, new List<float>(), new List<float>()),
                                        operations }
                                    }));
                            }
                            else
                            {
                                // invalid operation
                            }
                        }


                        // backward (역방향)
                        if (!(previous.intermediatePaths.Values.Count == 1 &&
                            previous.intermediatePaths.Values.ToList()[0].Count == 0))
                        {
                            // This is not the first operation.

                            foreach (var path in previous.intermediatePaths)
                            {
                                DistanceEnvironment env = path.Key;
                                List<int> operations = path.Value;

                                // TODO D에 연산 적용

                                // TODO C?D^에 연산 적용
                            }
                        }
                    }
                    if (j > 0)  // Insert
                    {
                        int k = i;
                        int l = j - 1;
                        DistanceTable previous = distanceTable[k][l];
                        // forward (정방향)


                        // backward (역방향)
                        if (!(previous.intermediatePaths.Values.Count == 1 &&
                            previous.intermediatePaths.Values.ToList()[0].Count == 0))
                        {
                            // This is not the first operation.

                        }
                    }
                    if (i > 0 && j > 0) // Move
                    {
                        int k = i - 1;
                        int l = j - 1;
                        DistanceTable previous = distanceTable[k][l];
                        // forward (정방향)


                        // backward (역방향)
                        if (!(previous.intermediatePaths.Values.Count == 1 &&
                            previous.intermediatePaths.Values.ToList()[0].Count == 0))
                        {
                            // This is not the first operation.

                        }
                    }

                    // TODO 최솟값을 가지는 모든 c들(argmin)을 구해서 하나의 DistanceTable로 만들어야 한다.
                    // 최솟값은 iDistance로 저장되고, 각 경로들은 Dictionary로 묶여서 iPaths로 저장된다. 
                    foreach (DistanceTable c in costs)
                    {
                        /*
                        // Overflow means there is an invalid operation.
                        if (c.distance < distanceTable[i][j].distance && c.distance >= 0)
                        {
                            distanceTable[i][j] = c;
                        }
                        */
                    }
                    Console.Write(distanceTable[i][j].intermediateDistance + "\t");  // TODO
                }
                Console.WriteLine();                                // TODO
            }

            // Print the shortest path.
            for (int i = lenThis, j = lenOther; i > 0 || j > 0;)
            {
                string s = distanceTable[i][j].path;
                Console.Write(s + ": ");
                if (s.Length > 0)
                {
                    if (s[0] == 'd')
                    {
                        Console.Write(distanceTable[i][j].distance - distanceTable[i - 1][j].distance);
                        i--;
                    }
                    else if (s[0] == 'i')
                    {
                        Console.Write(distanceTable[i][j].distance - distanceTable[i][j - 1].distance);
                        j--;
                    }
                    else // (s[0] == 'm')
                    {
                        Console.Write(distanceTable[i][j].distance - distanceTable[i - 1][j - 1].distance);
                        i--;
                        j--;
                    }
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine(distanceTable[i][j].distance);
                    break;
                }
            }

            return distanceTable[lenThis][lenOther].distance;
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

        /// <summary>
        /// Distance() 안에서 사용될 연산 비용 계산 함수입니다.
        /// 실제로 Delete, Insert, Move를 적용한 비용과 같은 결과가 나옵니다.
        /// Onset 인자는 없는 음표의 경우 -1로 넣습니다.
        /// PC(클러스터 번호) 인자는 없는 음표의 경우 float.NaN으로 넣습니다.
        /// </summary>
        /// <param name="targetNoteOnsetBeforeOp">연산을 적용하는 음표의 연산 직전 Onset</param>
        /// <param name="targetNotePCBeforeOp">연산을 적용하는 음표의 연산 직전 클러스터 번호</param>
        /// <param name="previousNoteOnsetBeforeOp">직전 음표의 연산 직전(=직후) Onset</param>
        /// <param name="previousNotePCBeforeOp">직전 음표의 연산 직전(=직후) 클러스터 번호</param>
        /// <param name="nextNoteOnsetBeforeOp">직후 음표의 연산 직전(=직후) Onset</param>
        /// <param name="nextNotePCBeforeOp">직후 음표의 연산 직전(=직후) 클러스터 번호</param>
        /// <param name="targetNoteOnsetAfterOp">연산을 적용하는 음표의 연산 직후 Onset</param>
        /// <param name="targetNotePCAfterOp">연산을 적용하는 음표의 연산 직후 클러스터 번호</param>
        /// <param name="pitchClusterListBeforeOp">연산 적용 직전 리듬 패턴에 존재하는 모든 클러스터 번호의 목록</param>
        /// <param name="pitchClusterListAfterOp">연산 적용 직후 리듬 패턴에 존재하는 모든 클러스터 번호의 목록</param>
        // PV(음 높이 변화) 인자는 없는 음표의 경우 0으로 취급합니다.
        private static int CalculateCost(int targetNoteOnsetBeforeOp, float targetNotePCBeforeOp,
            int previousNoteOnsetBeforeOp, float previousNotePCBeforeOp,
            int nextNoteOnsetBeforeOp, float nextNotePCBeforeOp,
            int targetNoteOnsetAfterOp, float targetNotePCAfterOp,
            List<float> pitchClusterListBeforeOp, List<float> pitchClusterListAfterOp)
        {
            int cost = 0;
            int operationType;
            int targetNotePVBeforeOp = -2, targetNotePVAfterOp = -2;

            if (targetNoteOnsetAfterOp < 0 || float.IsNaN(targetNotePCAfterOp))
            {
                operationType = 1; // Delete
                targetNoteOnsetAfterOp = -1;
                targetNotePCAfterOp = float.NaN;
                targetNotePVAfterOp = 0;
                if (targetNoteOnsetBeforeOp < 0 || float.IsNaN(targetNotePCBeforeOp))
                    throw new ArgumentException();
            }
            else if (targetNoteOnsetBeforeOp < 0 || float.IsNaN(targetNotePCBeforeOp))
            {
                operationType = 2; // Insert
                targetNoteOnsetBeforeOp = -1;
                targetNotePCBeforeOp = float.NaN;
                targetNotePVBeforeOp = 0;
            }
            else
                operationType = 3; // Move

            #region Onset cost
            if (targetNoteOnsetBeforeOp != -1 &&
                (targetNoteOnsetBeforeOp <= previousNoteOnsetBeforeOp ||
                (nextNoteOnsetBeforeOp != -1 && targetNoteOnsetBeforeOp >= nextNoteOnsetBeforeOp)))
                return int.MaxValue / 2;
            else if (targetNoteOnsetAfterOp != -1 &&
                (targetNoteOnsetAfterOp <= previousNoteOnsetBeforeOp ||
                (nextNoteOnsetBeforeOp != -1 && targetNoteOnsetAfterOp >= nextNoteOnsetBeforeOp)))
                return int.MaxValue / 2;
            else if (targetNoteOnsetBeforeOp != targetNoteOnsetAfterOp)
                cost += 1;
            #endregion

            #region target Pitch Variance cost
            if (float.IsNaN(previousNotePCBeforeOp))
            {
                targetNotePVBeforeOp = 0;
                targetNotePVAfterOp = 0;
            }
            else 
            {
                if (operationType == 1 || operationType == 3)
                {
                    // Delete or Move
                    if (previousNotePCBeforeOp < targetNotePCBeforeOp) targetNotePVBeforeOp = 1;
                    else if (previousNotePCBeforeOp == targetNotePCBeforeOp) targetNotePVBeforeOp = 0;
                    else targetNotePVBeforeOp = -1;
                    targetNotePVAfterOp = 0;
                }

                if (operationType == 2 || operationType == 3)
                {
                    // Insert or Move
                    targetNotePVBeforeOp = 0;
                    if (previousNotePCBeforeOp < targetNotePCAfterOp) targetNotePVAfterOp = 1;
                    else if (previousNotePCBeforeOp == targetNotePCAfterOp) targetNotePVAfterOp = 0;
                    else targetNotePVAfterOp = -1;
                }
            }

            if (targetNotePVBeforeOp == -2 || targetNotePVAfterOp == -2)
                throw new InvalidOperationException();

            if (targetNotePVBeforeOp != targetNotePVAfterOp)
                cost += 1;
            #endregion

            #region Cluster Rank cost
            pitchClusterListBeforeOp = pitchClusterListBeforeOp.Distinct().ToList();
            pitchClusterListBeforeOp.Sort();
            pitchClusterListAfterOp = pitchClusterListAfterOp.Distinct().ToList();
            pitchClusterListAfterOp.Sort();

            int rankBeforeOp = -1, rankAfterOp = -1;

            if ((operationType == 1 && targetNotePVBeforeOp != 0) ||
                (operationType == 2 && targetNotePVAfterOp != 0))
            {
                // Delete or Insert
                cost += 1;
            }
            if (operationType == 3)
            {
                // Move
                rankBeforeOp = pitchClusterListBeforeOp.IndexOf(targetNotePCBeforeOp);
                rankAfterOp = pitchClusterListAfterOp.IndexOf(targetNotePCAfterOp);

                if (rankBeforeOp == -1 || rankAfterOp == -1)
                    throw new ArgumentException();
                if (rankBeforeOp != rankAfterOp)
                    cost += 1;
            }
            #endregion

            #region next Pitch Variance cost
            int nextNotePVBeforeOp = -2, nextNotePVAfterOp = -2;

            if (!float.IsNaN(nextNotePCBeforeOp))
            {
                if (operationType == 1 || operationType == 3)
                {
                    // Delete or Move
                    if (targetNotePCBeforeOp < nextNotePCBeforeOp) nextNotePVBeforeOp = 1;
                    else if (targetNotePCBeforeOp == nextNotePCBeforeOp) nextNotePVBeforeOp = 0;
                    else nextNotePVBeforeOp = -1;
                    nextNotePVAfterOp = 0;
                }

                if (operationType == 2 || operationType == 3)
                {
                    // Insert or Move
                    nextNotePVBeforeOp = 0;
                    if (targetNotePCAfterOp < nextNotePCBeforeOp) nextNotePVAfterOp = 1;
                    else if (targetNotePCAfterOp == nextNotePCBeforeOp) nextNotePVAfterOp = 0;
                    else nextNotePVAfterOp = -1;
                }

                if (nextNotePVBeforeOp == -2 || nextNotePVAfterOp == -2)
                    throw new InvalidOperationException();

                if (nextNotePVBeforeOp != nextNotePVAfterOp)
                    cost += 1;
            }
            #endregion

            return cost;
        }

        /// <summary>
        /// 리듬 패턴 거리 계산 시 이전의 최적 연산 비용
        /// 계산 정보를 저장하는 구조체입니다.
        /// 원래 리듬 패턴의 맨 앞에서부터 i개의 음표를
        /// 목표 리듬 패턴의 맨 앞에서부터 j개의 음표로 바꾸는
        /// 비용과 관련된 정보 d_i,j를 표현합니다.
        /// </summary>
        private class DistanceTable
        {
            /*
            public readonly int distance;
            public readonly string path;
            public readonly RhythmPattern thisRP;
            public readonly RhythmPattern otherRP;

            public DistanceTable(int distance, string path, RhythmPattern thisRP, RhythmPattern otherRP)
            {
                this.distance = distance;
                this.path = path;
                this.thisRP = thisRP;
                this.otherRP = otherRP;
            }
            */
            /// <summary>
            /// 다음 연산 비용을 계산할 때 필요한 연산 비용 합의 최적
            /// </summary>
            public readonly int intermediateDistance;

            /// <summary>
            /// 다음 연산 비용을 계산할 때 필요한, 현재 마지막 음표의 연산이 이루어진 환경(Key)과 최적 경로(Value).
            /// 최적 경로는 지금까지 적용한 연산 종류와 순서를 나타낸 int 목록인데, 목록 안의 값은
            /// 정방향 Delete이면 1, 정방향 Insert이면 2, 정방향 Move이면 3,
            /// 역방향 Delete이면 -1, 역방향 Insert이면 -2, 역방향 Move이면 -3을 가집니다.
            /// </summary>
            public readonly Dictionary<DistanceEnvironment, List<int>> intermediatePaths;
            // 최적 경로(int 목록)의 첫 번째 값은 항상 양수 (첫 번째 연산이라 순서를 정의할 수 없음)
            // 정방향: 이전 연산들을 먼저 모두 수행한 후에 마지막 음표의 연산을 수행하는 순서
            // 역방향: 마지막 음표의 연산을 가장 먼저 수행한 후에 이전 연산들을 모두 수행하는 순서

            /// <summary>
            /// 최종 거리.
            /// 목표 리듬 패턴에 도달하기까지 필요한 연산 비용 합의 최적.
            /// </summary>
            public readonly int finalDistance;

            /// <summary>
            /// 최종 경로.
            /// 목표 리듬 패턴에 도달하기까지 필요한 최적 경로(연산 종류와 순서).
            /// 목록 안의 값은 정방향 Delete이면 1, 정방향 Insert이면 2, 정방향 Move이면 3,
            /// 역방향 Delete이면 -1, 역방향 Insert이면 -2, 역방향 Move이면 -3을 가집니다.
            /// </summary>
            public readonly List<int> finalPath;

            /// <summary>
            /// 중간 결과를 저장할 때 사용합니다.
            /// </summary>
            /// <param name="iDistance"></param>
            /// <param name="iPaths"></param>
            public DistanceTable(int iDistance, Dictionary<DistanceEnvironment, List<int>> iPaths)
            {
                this.intermediateDistance = iDistance;
                this.intermediatePaths = iPaths;
            }

            /// <summary>
            /// 최종 결과를 저장할 때 사용합니다.
            /// </summary>
            /// <param name="iDistance"></param>
            /// <param name="iPaths"></param>
            /// <param name="fDistance"></param>
            /// <param name="fPath"></param>
            public DistanceTable(int iDistance, Dictionary<DistanceEnvironment, List<int>> iPaths,
                int fDistance, List<int> fPath)
            {
                this.intermediateDistance = iDistance;
                this.intermediatePaths = iPaths;
                this.finalDistance = fDistance;
                this.finalPath = fPath;
            }
        }

        /// <summary>
        /// 리듬 패턴 거리 계산 시 이전에 연산 비용을 계산했던 환경을
        /// 그대로 계승하여 나중에 재사용하기 위해 필요한 정보를 담는 구조체입니다.
        /// </summary>
        private class DistanceEnvironment
        {
            /// <summary>
            /// d_i,j에서 마지막 연산이 수행되는 음표의 직전 음표의 연산 직전(=직후) 클러스터 번호
            /// </summary>
            public readonly float lastPreviousPitchClusterBeforeOp;

            /// <summary>
            /// d_i,j에서 마지막 연산이 수행되는 음표의 연산 직전 리듬 패턴의 클러스터 번호 목록
            /// (중복된 클러스터 번호가 없어야 함)
            /// </summary>
            public readonly List<float> pitchClusterListBeforeOp;

            /// <summary>
            /// d_i,j에서 마지막 연산이 수행되는 음표의 연산 직후 리듬 패턴의 클러스터 번호 목록
            /// (중복된 클러스터 번호가 없어야 함)
            /// </summary>
            public readonly List<float> pitchClusterListAfterOp;

            public DistanceEnvironment(float lastPrevPCBeforeOp,
                List<float> PCBeforeOp, List<float> PCAfterOp)
            {
                lastPreviousPitchClusterBeforeOp = lastPrevPCBeforeOp;
                pitchClusterListBeforeOp = PCBeforeOp;
                pitchClusterListAfterOp = PCAfterOp;

                pitchClusterListBeforeOp.Sort();
                pitchClusterListAfterOp.Sort();
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
