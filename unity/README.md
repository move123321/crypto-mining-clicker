# Crypto Mining Clicker - Unity Port

이 폴더는 웹 버전과 별도로 동작하는 Unity 버전입니다. 기존 index.html은 그대로 유지됩니다.

실행 방법
1. Unity Hub에서 이 저장소의 unity 폴더를 프로젝트로 추가합니다.
2. Unity 2022.3 LTS 이상으로 엽니다. Unity 6에서도 업그레이드 후 사용 가능합니다.
3. 처음 열면 Editor 스크립트가 Assets/Scenes/Main.unity를 자동 생성합니다.
4. Main.unity를 열고 Play를 누릅니다.

프로젝트 방향
- 2D 전용 구성
- Canvas/UI 및 2D Sprite 기반
- 3D 카메라, Directional Light, 3D 오브젝트는 게임 구성에 사용하지 않음
- Unity Scene View도 자동으로 2D 모드로 설정

현재 Unity 포트 목표
- 웹과 같은 8칸 GPU 랙
- GPU 클릭/자동 채굴
- 9단계 GPU 성장
- 멀티 코인과 GPU 보유 등급 기반 코인 해금
- 랜덤 시세 이벤트
- 코인 판매와 현금 경제
- 발열, GPU별 쿨링, 전기요금
- 오버클럭과 FEVER
- 계약
- FORK와 환생
- JSON 저장
- 첫 실행 컷신과 튜토리얼
- 모바일형 런타임 UI

웹판과 Unity판의 저장 데이터는 서로 별도입니다.

앞으로 웹 기능을 수정할 때 Unity 폴더도 같은 규칙으로 함께 업데이트할 수 있습니다.
