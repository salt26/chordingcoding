# ChordingCoding
![ChordingCoding Logo](https://raw.githubusercontent.com/salt26/chordingcoding/v.1.6/ChordingCoding/Resources/Logos/Logo.gif)

With ChordingCoding, typing becomes composing music!

(ChordingCoding과 함께라면, 타이핑이 곧 작곡이 됩니다!)

## Description
![ChordingCoding](https://raw.githubusercontent.com/salt26/chordingcoding/master/ChordingCoding/Resources/Title.png)

ChordingCoding은 코딩과 문서 작업에 지친 사람들의 감성을 채워주기 위해 만들었습니다.

이 프로그램을 켜면 키보드로 타이핑을 할 때마다 아름다운 소리가 생겨납니다.

또한 사용자의 작업 맥락을 자동으로 추적하여 기록합니다.

Windows Forms로 개발된 응용 프로그램으로, 64-bit Windows에서 사용 가능합니다.

## How To Use
1. 커맨드라인에서 `git clone https://github.com/salt26/chordingcoding`을 실행하세요.
   * 만약 git에 친숙하지 않은 분이라면 [여기](https://github.com/salt26/chordingcoding/archive/master.zip)를 눌러 최신 버전의 파일을 다운로드받으세요.
 
2. Builds 폴더에 있는 "ChordingCoding.exe"를 실행합니다.
   * 바로 가기(.lnk)를 만들면 실행하기 편리합니다.
   * 만약 프로그램의 경로를 옮기고 싶다면 Builds 폴더 내의 모든 파일을 통째로 옮겨주세요.

3. 작업 표시줄에 있는 시스템 트레이에 음표 모양 ChordingCoding 아이콘(![Tray](https://raw.githubusercontent.com/salt26/chordingcoding/master/ChordingCoding/Resources/Tray.ico))이 있는 것을 볼 수 있습니다. 이 아이콘을 클릭하면 메뉴가 나타납니다.

4. 메뉴에서 테마를 변경할 수 있습니다. 테마에 따라 악기, 화음, 자동 반주 패턴 등이 달라집니다.

5. 메뉴에서 단위 리듬, 음량 및 자동 반주 재생 여부를 변경할 수 있습니다.
   * 예를 들어, 16분음표의 리듬에 맞춰 연주하고 싶다면 단위 리듬을 "16분음표"로 놓으면 되겠죠.
   * 자동 반주를 켜면 타이핑에 반응하여 소리가 나는 것은 물론, 타이핑하지 않을 때에도 자동으로 반주가 생성됩니다.
     * 글을 읽을 때 자동 반주를 켜보시는 건 어떨까요?

6. 설정이 완료되면 소리를 켜고 키보드로 타이핑을 하면 됩니다.
   * 스페이스 등의 공백 문자를 입력할 때마다 화음이 달라집니다.
   * 일반 글자를 입력할 때마다 화음에 맞는 음이 무작위로 생성됩니다.

7. ChordingCoding을 사용하는 동안 사용자의 작업 맥락을 추적하여 기록합니다.
   * 사용했던 프로그램 이름, 키보드 입력, 마우스 클릭 및 스크롤 입력이 기록됩니다.
   * 종료 후 Builds 폴더 안에 "WorkingContext.csv" 파일이 생성됩니다. 이것의 이름을 바꾸고 원하는 경로에 옮겨서 보관하세요.
   * 기록한 데이터는 사용자의 컴퓨터에만 남으며, 외부로는 전송되지 않습니다.
   * 기록한 데이터를 [Working Context Visualization](https://github.com/salt26/working-context-visualization)를 통해 분석하고 살펴볼 수도 있습니다.
  
## Update Log
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
* 시각 효과 제거
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
