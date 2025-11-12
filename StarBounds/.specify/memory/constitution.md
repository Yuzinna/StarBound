<!--
---
sync_impact_report:
  version_change: "1.2.0 -> 1.3.0"
  reason: "Added a new principle for Code Quality and Process Standards."
  modified_principles:
    - "Added: IV. 코드 품질 및 프로세스 표준"
  added_sections: []
  removed_sections: []
  templates_updated: []
  todos:
    - "TODO(RATIFICATION_DATE): Set initial project start date."
---
-->
# StarBounds 헌법

## 핵심 원칙

### I. 핵심 게임 경험: 직관적인 중력 조절 플랫포머

주요 목표는 플레이어가 성취감을 느낄 수 있는 고품질 2D 횡스크롤 플랫포머 퍼즐 게임을 개발하는 것입니다.

핵심 메커니즘은 다음과 같습니다:
- **정확한 캐릭터 조작:** 걷기, 점프, 오브젝트 상호작용.
- **환경 상호작용 퍼즐:** 중력 조정 스위치를 활성화하고, 중력 변화에 따라 움직이는 오브젝트를 밀고 당기는 등 환경과 상호작용하여 퍼즐을 해결합니다.

모든 기능은 이 핵심 경험을 지원하고 향상시켜야 합니다. 초기 개발 마일스톤으로, 최소 3개의 완료 가능한 퍼즐 레벨을 구현해야 합니다.

### II. 문서 언어

모든 speckit 관련 문서(예: constitution.md, spec.md, plan.md, tasks.md)는 한국어로 작성되어야 합니다. 이는 모든 한국어 사용 팀원들을 위한 명확한 의사소통과 단일 진실 공급원을 보장합니다.

### III. 개발 환경

게임 개발은 Unity Engine을 사용하며, 주 프로그래밍 언어는 C#입니다.

### IV. 코드 품질 및 프로세스 표준

- **언어:** 모든 코드는 C#으로 작성되어야 하며, 물리 관련 로직은 `FixedUpdate()`를 사용하는 등 유니티의 모범 사례를 따라야 합니다.
- **변수 명명:** 변수명은 `moveSpeed`, `isGrounded`와 같이 카멜 케이스(Camel Case)를 사용해야 합니다.
- **주석:** 복잡한 로직이나 재사용되는 함수는 반드시 주석으로 설명해야 합니다.
- **Spec-Driven 개발:** 모든 주요 기능은 본 헌법과 이를 바탕으로 작성된 Spec 문서에 정의된 내용에 부합해야 합니다.

## 거버넌스

모든 개발 결정은 핵심 원칙에 부합해야 합니다. 제안된 모든 기능 또는 변경 사항은 플레이어의 성취감에 대한 기여도, 문서 및 코드 품질 표준 준수, 그리고 정의된 개발 환경과의 호환성 여부에 따라 평가됩니다.

**버전**: 1.3.0 | **비준일**: TODO(RATIFICATION_DATE): 프로젝트 시작일 설정. | **최종 수정일**: 2025-11-12