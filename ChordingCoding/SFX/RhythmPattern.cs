using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// 음표 목록에 있는 한 음표가 직전 음표보다 높은 음을 가지면 1,
        /// 낮은 음을 가지면 -1, 같은 음을 가지거나 첫 번째 음표이면 0을 반환합니다.
        /// 음표 목록에서 이 음표를 찾지 못한 경우 -2를 반환합니다.
        /// </summary>
        /// <param name="noteInNoteList">음표 목록에 있는 한 음표</param>
        /// <returns></returns>
        public int PitchVariance(RhythmPatternNote noteInNoteList)
        {
            LinkedListNode<RhythmPatternNote> node = noteList.Find(noteInNoteList);
            if (node == null) return -2;
            else
            {
                if (node.Value.pitchVariance != -2) return node.Value.pitchVariance;
                else
                {
                    LinkedListNode<RhythmPatternNote> prev = node.Previous;
                    if (prev == null) return 0;
                    else if (prev.Value.Cluster < node.Value.Cluster) return 1;
                    else if (prev.Value.Cluster == node.Value.Cluster) return 0;
                    else return -1;
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
            if (!metadataList.Exists(e => e.cluster == cluster)) return false;
            int metadataIndex = metadataList.FindIndex(e => e.cluster == cluster);

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
            if (!metadataList.Exists(e => e.cluster == cluster)) return;
            int metadataIndex = metadataList.FindIndex(e => e.cluster == cluster);

            metadataList[metadataIndex].absolutePitch = -1;
            HasImplemented = false;
        }
    }

    /// <summary>
    /// 음표.
    /// 리듬 패턴의 음표 목록에 들어갈 구조체입니다.
    /// 음표의 시작 위치 정보와 음 높이 변화 정보를 포함합니다.
    /// </summary>
    public class RhythmPatternNote
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
        public float Cluster
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

        public RhythmPatternNote(int index, float cluster)
        {
            OnsetPosition = index;
            Cluster = cluster;
            pitchVariance = -2;
        }
    }

    /// <summary>
    /// 메타데이터.
    /// 리듬 패턴의 클러스터 정보와
    /// 이 클러스터에 할당된 절대적인 음 높이 정보를 포함합니다.
    /// 리듬 패턴을 악보로 구체화(implement)할 때 사용됩니다.
    /// </summary>
    public class RhythmPatternMetadata
    {
        /// <summary>
        /// 클러스터 번호
        /// </summary>
        public int cluster;

        /// <summary>
        /// 이 클러스터 번호에 속해 있는 음표들의 시작 위치 목록
        /// </summary>
        public List<int> noteIndices = new List<int>();

        /// <summary>
        /// 이 클러스터에 할당된 절대적인 음 높이.
        /// RhythmPattern.SetAbsolutePitch()로 값을 지정할 수 있습니다.
        /// 지정되지 않은 경우 -1을 가지고 있습니다.
        /// </summary>
        public int absolutePitch;

        public RhythmPatternMetadata(int cluster, params int[] noteIndices)
        {
            this.cluster = cluster;
            this.noteIndices = noteIndices.ToList();
            absolutePitch = -1;
        }
    }
}
