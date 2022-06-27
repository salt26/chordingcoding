# ChordingCoding
![ChordingCoding Logo](https://raw.githubusercontent.com/salt26/chordingcoding/master/ChordingCoding/Resources/Logos/Logo.gif)

With ChordingCoding, typing becomes composing music!

(ChordingCoding과 함께라면, 타이핑이 곧 작곡이 됩니다!)

## Description
![ChordingCoding](https://raw.githubusercontent.com/salt26/chordingcoding/master/ChordingCoding/Resources/Title.png)

ChordingCoding은 코딩과 문서 작업에 지친 사람들의 감성을 채워주기 위해 만들었습니다.

이 프로그램을 켜면 키보드로 타이핑을 할 때마다 아름다운 소리가 생겨납니다.

Windows Forms로 개발된 응용 프로그램으로, 64-bit Windows에서 사용 가능합니다.

## How To Use
1. [여기](https://github.com/salt26/chordingcoding/releases/tag/v.1.8.2)를 클릭하여 파일 `ChordingCoding.v.1.8.2.zip`을 다운로드 받으세요.
 
2. 압축을 풀고 `ChordingCoding.exe`를 실행합니다.
   * `ChordingCoding.exe`의 바로 가기(.lnk)를 만들면 실행하기 편리합니다.
   * 실행이 잘 되지 않으면 [이슈 게시판](https://github.com/salt26/chordingcoding/issues)을 활용해 주세요!

3. 작업 표시줄에 있는 시스템 트레이에 음표 모양 ChordingCoding 아이콘(![Tray](https://raw.githubusercontent.com/salt26/chordingcoding/master/ChordingCoding/Resources/Tray.ico))이 있는 것을 볼 수 있습니다. 이 아이콘을 클릭하면 메뉴가 나타납니다.

4. 메뉴에서 **테마**를 변경할 수 있습니다. 테마에 따라 악기, 자동 반주 패턴 등이 달라집니다.

5. 메뉴에서 단위 리듬, 선법, 음량, 자동 반주 재생 여부 및 반향 적용 여부를 변경할 수 있습니다.
   * 예를 들어, 16분음표의 리듬에 맞춰 연주하고 싶다면 단위 리듬을 "16분음표"로 놓으면 되겠죠.
   * 자동 반주를 켜면 타이핑에 반응하여 소리가 나는 것은 물론, 타이핑하지 않을 때에도 자동으로 반주가 생성됩니다.
     * 집중할 때는 자동 반주를 끄셔도 좋습니다.
   * **반향**을 켜면 울림이 더해져 소리가 더 자연스럽게 들립니다.
     * 성능 문제가 생긴다면 반향을 끄셔도 좋습니다.

6. 설정이 완료되면 소리를 켜고 키보드로 타이핑을 하면 됩니다.
   * 스페이스 등의 공백 문자를 입력할 때마다 화음이 달라집니다.
   * 일반 글자를 입력할 때마다 화음에 맞는 음이 무작위로 생성됩니다.

7. 방금 들은 음악을 오래 간직하고 싶다면 메뉴에서 녹음 버튼을 누르거나 F12 키를 누르세요.
   * 최근에 생성된 음악을 즉시 파일로 저장할 수 있습니다.
   * 1분 이상 사용하지 않아 침묵이 유지되면 그 이전의 음악은 저장되지 않으니 주의 바랍니다.

8. ChordingCoding을 사용하는 동안 사용자의 작업 맥락을 추적하여 기록합니다.
   * 사용했던 프로그램 이름, 키보드 입력, 마우스 클릭, ChordingCoding 설정 상태, 생성된 음악 등이 기록됩니다.
     * 키보드 입력 중 글자는 "Alphabet"으로, 숫자는 "Number"로, 특수문자는 "Symbol" 또는 "SymbolInWord"로 비식별화되어 기록됩니다. **즉, 비밀번호 유출은 걱정하지 않아도 됩니다.**
   * 종료 후 `ChordingCoding.exe`가 있는 폴더 안에 `Logs` 폴더가 생기고, 그 안에 `ChordingCoding_context_log_yyMMdd.csv` 파일들이 생성됩니다. (`yyMMdd`는 오늘 날짜입니다.)
     * 주의: ChordingCoding을 사용할 때에는 오늘 날짜의 `.csv` 파일을 닫아주세요.
   * **기록한 데이터는 사용자의 컴퓨터에만 남으며, 외부로는 전송되지 않습니다.**

## Update Log
### [v.1.8.2](https://github.com/salt26/chordingcoding/tree/v.1.8.2) - 22/06/27
* 사용 도중 음악이 재생되지 않는 버그 수정
* 오래 사용 시 키 입력과 음 생성 사이에 지연이 발생하는 버그 수정
* 오래 사용 시 메모리를 과도하게 잡아먹는 버그 수정
* 사용자 작업 맥락 추적 기능 개선 (날짜 별 기록)

### [v.1.8.1](https://github.com/salt26/chordingcoding/tree/v.1.8.1) - 22/06/14
* 사용자 작업 맥락 추적 기능 활성화 및 개선 (보안 강화)
* 소스코드 라이선스 업데이트
* *버그 존재 -> [v.1.8.2](https://github.com/salt26/chordingcoding/tree/v.1.8.2)로 업데이트할 것을 권장*

### [v.1.8](https://github.com/salt26/chordingcoding/tree/v.1.8) - 22/05/26
* 반향 효과 추가
* 선법 선택 기능 추가 (장조, 단조)
* 테마 추가 및 개선 (골동품, 바람의 속삭임, 파티 타임 추가; 별 헤는 밤, 중세 유적지 악기 변경)
* 입력하는 글의 감성 인식 기능 비활성화
* *버그 존재 -> [v.1.8.2](https://github.com/salt26/chordingcoding/tree/v.1.8.2)로 업데이트할 것을 권장*

### [v.1.7](https://github.com/salt26/chordingcoding/tree/v.1.7) - 22/01/28
* 더 자연스러운 음악(화음 전이) 생성
* 입력하는 글의 감성을 인식하여 이를 반영하도록 음악 생성 (영어와 한글 지원, 감성 인식 수준 조절 가능)
* 녹음 기능 추가 (최근 생성된 음악을 파일로 저장)
* 사용자 작업 맥락 추적 기능 비활성화

### [v.1.6](https://github.com/salt26/chordingcoding/tree/v.1.6) - 21/06/30
* 시작 시 스플래시 화면 추가
* 사용자 작업 맥락 추적 기능 추가

### [v.1.4](https://github.com/salt26/chordingcoding/tree/v.1.4) - 20/02/05
* 테마 변경 (중세 탐방 -> 중세 유적지)
* 전반적인 테마 개선
* 자동 반주가 없던 모든 테마에 자동 반주 추가 (가을 산책, 별 헤는 밤)
* 자동 반주 켜기/끄기 기능 추가

### [v.1.3](https://github.com/salt26/chordingcoding/tree/c88de83e97e2b3d9c0b596ead8346596008a97b6) - 20/01/14
* 전반적인 음질 향상
* 두 종류의 기존 테마 제거 (비 오는 날, 숲 속 아침)
* 64-bit 운영체제에서만 사용 가능하도록 변경

### [v.1.2](https://github.com/salt26/chordingcoding/tree/d142907ad503d0a05afa29292f3c589c41f26535) - 19/12/23
* 새로운 테마 추가 (숲 속 아침, 피아노포르테, 구름 너머, 중세 탐방)
* 자동 반주 기능 추가 (새로 추가된 네 종류의 테마에 적용)
* 더 정확해진 음 재생 간격
* 시각 효과 비활성화
* 아이콘 이미지 변경
* 동시성 문제와 관련한 각종 버그 수정

### [v.1.1](https://github.com/salt26/chordingcoding/tree/e1bbfc8c63e4a041518cc3a9a29d0b716bef0e0f) - 19/10/08
* 단위 리듬 설정 기능 추가
* 해상도 관련 버그 수정
* 코드 리팩토링

### [v.1.0](https://github.com/salt26/chordingcoding/tree/bf916a4bd38ae5c2b004d9f9574b6253dc6fd225) - 19/07/14
* 세 종류의 테마 (가을 산책, 비 오는 날, 별 헤는 밤) 와 시각 효과를 가진 초기 버전

## Contact
질문이나 건의사항이 있다면 [이슈 게시판](https://github.com/salt26/chordingcoding/issues)을 활용해 주세요!
