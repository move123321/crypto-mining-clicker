# 갤럭시에서 실행하기

Unity 6000.0.73f1과 Android Build Support, Android SDK & NDK Tools, OpenJDK가 필요합니다.

Unity 메뉴의 **Crypto Mining → Build Galaxy APK**를 선택하면
`Builds/Android/CryptoMiningClicker.apk`를 만듭니다.
빌드 설정은 세로 화면, ARM64, IL2CPP이며 Main 씬이 포함됩니다.
이 APK는 직접 설치해서 테스트하는 용도이며 기본 디버그 서명을 사용합니다.
스토어 배포에는 별도의 서명 키와 배포 설정이 필요합니다.

APK를 갤럭시로 옮겨 파일을 열고 해당 파일 앱의 설치 허용 안내를 따르면 됩니다.
PC의 게임 진행 상황은 휴대폰으로 자동 복사되지 않습니다.

## 한글 폰트

폰트는 앱에 포함되므로 휴대폰에 설치된 폰트와 관계없이 한글을 표시합니다.

- Noto Sans CJK KR Regular: https://github.com/notofonts/noto-cjk/blob/main/Sans/OTF/Korean/NotoSansCJKkr-Regular.otf
- SIL Open Font License 1.1: `Assets/Resources/CryptoMining/Fonts/OFL.txt`
- 원본 배포처: https://github.com/notofonts/noto-cjk

TextMeshPro가 포함된 폰트에서 필요한 글자의 SDF를 생성합니다.
