# 1.3.0 출시 준비 상태

## 반영 및 검사
- 정상 백업과 중단된 임시 저장 파일 복구, 손상 원본 별도 보존, 모든 파일이 손상됐으면 자동 저장/진행 차단.
- 긴급 냉각은 20°C 감소, 접속 중 120초 대기. 강화 포크 적용 시 25°C. 재접속으로 대기시간을 초기화할 수 없음.
- 안전 영역 안에 UI 배치, 뒤로 가기로 팝업 닫기, 음악/효과음 설정, 구매·업그레이드·계약 알림음.
- 계약/인증/확장/환생 목표 안내 및 게임 전용 아이콘.
- ReleaseChecks.Run은 기존 게임/부동산 검사와 저장 손상·냉각·안전 영역 검사를 함께 실행.

## 출시용 AAB
Unity 메뉴 Crypto Mining → Build Store AAB 또는 -executeMethod StoreBundleBuild.Build 사용.
환경 변수 CRYPTO_UPLOAD_KEYSTORE, CRYPTO_UPLOAD_ALIAS, CRYPTO_UPLOAD_PASSWORD, CRYPTO_UPLOAD_KEY_PASSWORD가 필요합니다.
서명 키가 없으면 빌드를 거부합니다. 키/비밀번호를 Git에 넣지 마세요. 키는 별도로 안전하게 보관해야 합니다.
이 작업에서 정식 키를 생성하거나 Google Play에 앱을 게시하지 않았습니다. 제공 APK는 기존 테스트 서명으로 업데이트 가능한 테스트 파일입니다.

## 실제 출시 전 남은 작업
- USB 연결 기기가 없어 갤럭시 설치/업데이트, 30~60분 발열·배터리, 화면 껐다 켜기, 강제 종료 후 복구 및 실제 성장 속도 검증은 미완료.
- 개발자명과 문의 이메일은 사용자가 미정으로 답변함. 개인정보처리방침은 초안이며 공개하지 않음.
- 출시 서명 키, Play Console 계정, 스토어 이미지, 데이터 보안·콘텐츠 등급 양식, 해당 계정의 비공개 테스트 및 심사 필요.
- 공식 요건 확인: https://support.google.com/googleplay/android-developer/answer/14151465?hl=ko
