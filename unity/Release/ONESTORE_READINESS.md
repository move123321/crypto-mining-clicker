# 원스토어 1.3.6 출시 준비 기록

작성일: 2026-10-10
개발자: muibeu / seoghunjo08@gmail.com

## 완료
- 음량 바와 개인정보·문의 안내를 포함한 1.3.6 / versionCode 10 구현.
- Unity 회귀 검사, 설정 경계값과 음소거·값 재읽기, 20개 화면·크기 조합 검사 통과. 글자 넘침 0.
- RSA 3072 정식 키로 서명한 비개발 APK 생성. ARM64 / 최소 API 23 / 대상 API 36.
- APK 서명 및 zipalign 검증 성공. Galaxy A90 5G (SM-A908N)에 설치 성공, 프로세스 실행 및 조회한 게임 오류 로그에서 오류 없음.
- 스토어 소개문, 개발자 연락처, 512×512 기존 앱 아이콘, 개인정보처리방침 게시용 HTML/Markdown 초안 준비.

## 남은 출시 절차
- 판매자 계정 로그인·정보 확인, 앱 등록 및 판매 가격 결정.
- 연령등급 설문 작성과 심사. 전체이용가 등 등급을 임의로 확정하지 않음.
- 개인정보처리방침 초안과 문의 정보 삭제 운영을 확인하고 시행일을 확정, 공개 웹 주소에 게시.
- 실제 화면 스크린샷 확보 및 등록 화면의 이미지 규격 최종 확인.
- 갤럭시 잠금 해제 후 슬라이더 터치·재실행·저장 유지 검사. 잠금 상태에서는 해당 검사를 통과로 간주하지 않음.
- APK 심사 요청 및 출시. 현재 원스토어에 업로드·게시하지 않음.

## 서명과 보관
정식 서명키 SHA-256: e201753a11c8f9bf003123aec05e9ea1a0eb4033eb71fb29f826889d580e4977
키와 비밀번호는 Git 저장소와 제출 폴더 밖의 사용자 전용 접근권한 폴더에 보관. 외부 안전한 위치에 별도 백업 필요.
후속 업데이트는 반드시 같은 서명키를 사용. 예전 테스트 서명 APK와 정식 서명 APK는 서명이 달라 직접 업데이트되지 않음. 저장 데이터를 지우기 위해 기존 앱을 임의로 삭제하지 말 것.

## 재빌드
Unity 메뉴: Crypto Mining → Build ONE store APK
배치 실행: -executeMethod AndroidApkBuild.BuildOneStore -apkOutputPath <출력 APK 절대경로>
환경 변수: CRYPTO_UPLOAD_KEYSTORE, CRYPTO_UPLOAD_ALIAS, CRYPTO_UPLOAD_PASSWORD, CRYPTO_UPLOAD_KEY_PASSWORD
비밀번호는 명령줄·소스·Git·공개 로그에 넣지 말 것. 키 설정 누락 시 빌드를 중단하도록 구현.

## 한계
구글 로그인·클라우드 저장은 미구현. 출시용 1.3.6의 장시간 발열·배터리·60FPS 성능은 별도 검사가 필요.
이전 1.3.4 30분 보고서는 별도 개발용 앱·30FPS 조건의 결과이며 이번 버전 전체 품질 보증으로 사용하지 않음.

## 공식 안내
- APK/AAB 지원: https://onestore-dev.gitbook.io/dev/docs/apps
- 등급·개인정보·검증: https://onestore-dev.gitbook.io/dev/help/faq/review
- 스크린샷 등록 안내: https://onestore-dev.gitbook.io/dev/help/faq/apps
