using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChordingCoding
{
    class WordTracker
    {
        // TODO 사전과 악보 저장 기능 (Dictionary 사용)
        // 사전에 저장하는 단어는 일단 전부 영어로(한글의 경우 두벌식 키보드 기준 영어로 친 그대로) 하고, Shift는 누르지 않은 상태로 저장한다.
        // InterceptKeys.cs의 콜백 함수에서 새 일반 문자를 입력할 때마다 상태 문자열이 사전에 있는지 검색해야 한다.
        // 검색 함수에서 단어를 찾으면, 해당 악보를 재생해야 한다.
        // 악보를 재생하는 동안 주 멜로디 및 자동 반주의 음량을 상당히 줄여야 한다. (20 ~ 50% 수준)
        // 위의 기능들을 완성하면 InterceptKeys.cs의 디버깅 코드들은 주석처리하자.

        // 나중에 사용자가 코드 수정 없이 직접 단어를 추가할 수 있게 하려면,
        // Properties.Settings 변수 또는 별도의 저장 파일을 만들어서 단어 목록을 저장해야 한다.
    }
}
