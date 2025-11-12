# Data Model: 플레이어 이동 및 상호작용

이 문서는 '플레이어 이동 및 상호작용' 기능 구현에 필요한 주요 C# 클래스의 데이터 모델을 정의합니다.

## 1. `PlayerMovement.cs`

플레이어의 입력을 받아 움직임, 점프, 상호작용을 처리하는 핵심 컴포넌트입니다.

-   **Fields (Public or `[SerializeField]`)**:
    -   `float moveSpeed`: 플레이어의 좌우 이동 속도.
    -   `float jumpForce`: 플레이어의 점프 힘.
    -   `LayerMask groundLayer`: 바닥으로 인식할 레이어. 점프 가능 여부 판단에 사용.
    -   `Transform groundCheck`: 플레이어의 발밑에 위치하여 바닥을 감지하는 지점.
-   **Properties (Public)**:
    -   `bool isGrounded { get; }`: 플레이어가 땅에 닿아있는지 여부.
-   **Methods (Public)**:
    -   `void Move(float direction)`: 주어진 방향으로 플레이어를 이동시킵니다.
    -   `void Jump()`: 플레이어를 점프시킵니다.
    -   `void Interact()`: 주변의 상호작용 가능한 오브젝트와 상호작용을 시도합니다.

## 2. `IInteractable.cs` (Interface)

상호작용 가능한 모든 오브젝트가 구현해야 하는 인터페이스입니다.

-   **Methods**:
    -   `void Interact()`: 상호작용 시 실행될 로직을 정의합니다.

## 3. `GravitySwitch.cs`

`IInteractable` 인터페이스를 구현하며, 활성화 시 게임 월드의 중력을 변경합니다.

-   **Fields (Public or `[SerializeField]`)**:
    -   `Vector2 gravityDirection`: 활성화 시 적용될 중력 방향.
-   **State**:
    -   `bool isActivated`: 스위치의 현재 활성화 상태.
-   **Methods**:
    -   `public void Interact()`: 스위치의 활성화 상태를 토글하고, `Physics2D.gravity` 값을 변경합니다.

## 4. `Cube.cs`

`IInteractable` 인터페이스를 구현하며, 특정 조건 하에 플레이어가 밀고 당길 수 있는 오브젝트입니다.

-   **State**:
    -   `bool canBeMoved`: 중력 스위치에 의해 이동 가능 상태가 결정됩니다.
-   **Methods**:
    -   `public void Interact()`: 플레이어가 상호작용을 시도할 때 호출됩니다. `canBeMoved` 상태일 경우, 플레이어에 자신을 '붙여' 함께 움직이도록 처리하는 로직이 포함될 수 있습니다. (구체적인 구현은 개발 단계에서 결정)
