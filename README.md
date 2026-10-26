# BallDodge_R6 — 공 피하기 과제 코드

기본 과제와 응용 v3의 C# 구현을 읽고 검토하기 위한 **코드 전용 공개 저장소**입니다. Unity 6000.6.6f1에서 작성된 프로젝트에서 C# 21개만 선별했습니다.

**완전한 Unity 프로젝트가 아닙니다.** 씬, 프리팹, 재질, .meta, Packages, ProjectSettings는 포함하지 않습니다. 따라서 이 저장소를 복제하는 것만으로 게임을 실행하거나 동일 프로젝트를 재현할 수 없습니다.

## 기본 과제

WASD로 이동하며 낙하 공을 피합니다. 공에 접촉하면 Game Over가 되고 이동과 생존 시간 표시가 멈추는 기본 규칙입니다.

| 파일 | 역할 |
| --- | --- |
| [Ball.cs](Assets/Scripts/Ball.cs) | 공 접촉 시 게임오버 요청 |
| [BallSpawner.cs](Assets/Scripts/BallSpawner.cs) | 일정 간격의 낙하 공 생성 |
| [GameManager.cs](Assets/Scripts/GameManager.cs) | 게임오버 및 생존 시간 관리 |
| [PlayerController.cs](Assets/Scripts/PlayerController.cs) | WASD 이동과 게임오버 시 이동 정지 |
| [Rotator.cs](Assets/Scripts/Rotator.cs) | Inspector에서 조정하는 Y축 회전 |

## 응용 v3

기본 코드와 별도 폴더로 분리했습니다. 바닥 밖 추락이 게임오버 조건이고, 공은 넉백을 줍니다. 점프·관성·폭발 공·이동 및 회전 기둥·측면 밀기판·측면 구체·강타 및 왕복 막대·시간에 따른 난도 증가를 구현합니다. 시작 메뉴, 설정, 게임오버, 재시작 및 메뉴 복귀 흐름을 포함합니다.

기본 조작: WASD 이동, Space 점프, K 설정, R 재시작. 설정 메뉴에서 이동·점프 키를 바꿀 수 있습니다. 실제 작동에는 원래 씬의 컴포넌트 연결과 Unity Input System/uGUI 등 프로젝트 의존성이 필요합니다.

| 파일 | 역할 |
| --- | --- |
| [AppliedBall.cs](Assets/Applied/Scripts/AppliedBall.cs) | 공 충돌·폭발 및 수명 종료 |
| [AppliedBallSpawner.cs](Assets/Applied/Scripts/AppliedBallSpawner.cs) | 낙하 공 생성과 개수 제한 |
| [AppliedDifficulty.cs](Assets/Applied/Scripts/AppliedDifficulty.cs) | 생존 시간에 따른 난도 계산 |
| [AppliedExplosionFx.cs](Assets/Applied/Scripts/AppliedExplosionFx.cs) | 폭발 시각 효과 |
| [AppliedGameManager.cs](Assets/Applied/Scripts/AppliedGameManager.cs) | 메뉴·플레이·설정·게임오버 상태와 재시작 |
| [AppliedKeyBindings.cs](Assets/Applied/Scripts/AppliedKeyBindings.cs) | 이동·점프 키 설정, 중복 교환 및 저장 |
| [AppliedMenuUI.cs](Assets/Applied/Scripts/AppliedMenuUI.cs) | uGUI 메뉴·설정·HUD·게임오버 화면 |
| [AppliedPillarMover.cs](Assets/Applied/Scripts/AppliedPillarMover.cs) | 기둥 이동과 위치 초기화 |
| [AppliedPlayerController.cs](Assets/Applied/Scripts/AppliedPlayerController.cs) | 물리 이동·관성·점프·넉백 |
| [AppliedRandomRotator.cs](Assets/Applied/Scripts/AppliedRandomRotator.cs) | 기둥의 무작위 Y축 회전 |
| [AppliedSideBall.cs](Assets/Applied/Scripts/AppliedSideBall.cs) | 측면 구체 이동·충돌·수명 |
| [AppliedSideBallSpawner.cs](Assets/Applied/Scripts/AppliedSideBallSpawner.cs) | 측면 구체 예고와 생성 제한 |
| [AppliedSidePusher.cs](Assets/Applied/Scripts/AppliedSidePusher.cs) | 측면 밀기판의 이동과 밀기 |
| [AppliedSlamBar.cs](Assets/Applied/Scripts/AppliedSlamBar.cs) | 강타 막대의 예고·스윙·판정 |
| [AppliedSlamBarSpawner.cs](Assets/Applied/Scripts/AppliedSlamBarSpawner.cs) | 강타 기믹 생성 |
| [AppliedSweepBar.cs](Assets/Applied/Scripts/AppliedSweepBar.cs) | 상하 왕복 막대 기믹 |

## 코드 읽기 순서와 학습 포인트

1. 기본 PlayerController → Rotator → BallSpawner → Ball → GameManager 순서로 이동·회전·생성·충돌·상태 흐름을 봅니다.
2. 응용 AppliedGameManager와 AppliedMenuUI에서 상태 전환을 보고 AppliedPlayerController와 개별 기믹으로 이어갑니다.
3. public은 다른 코드에서 접근하는 공개 멤버, private은 클래스 내부 구현입니다. [SerializeField]를 붙인 private 필드는 외부 접근을 제한하면서 Inspector에서 값을 설정할 수 있습니다.
4. static 멤버는 개별 오브젝트 인스턴스에 속하지 않는 공유 상태/기능입니다. 사용 시 씬 전환과 재시작 초기화를 함께 검토해야 합니다.
5. Time.deltaTime은 프레임 간 경과 시간입니다. 초당 이동·회전량에 곱하면 프레임 수 차이의 영향을 줄입니다. 물리 처리는 FixedUpdate와 고정 시간 간격을 함께 고려합니다.

## 검수 범위 및 제외 항목

사용자가 응용 v3를 선택한 시점의 게임 코드 공유본입니다. 이 저장소 게시 자체가 모든 게임 동작이나 웹 배포 성공을 보증하지 않습니다. 사람의 개별 버튼 클릭, 변경 Jump 키의 실제 점프, 기본 버전 회귀 플레이는 별도 검수 근거 없이 PASS로 표시하지 않습니다.

개인 설정·인증정보·MCP/Headroom 설정·내부 지시서·개인 경로·다른 프로젝트·원문 백업·영상·Unity 캐시 및 로그는 게시하지 않습니다. 코드 변경 없이 읽기 공유를 위해 폴더 구조를 유지했습니다.
