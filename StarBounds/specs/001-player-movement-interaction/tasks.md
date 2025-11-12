# Tasks: 플레이어 이동 및 상호작용

**Input**: Design documents from `/specs/001-player-movement-interaction/`
**Prerequisites**: plan.md, spec.md, data-model.md

**Tests**: Play Mode tests will be created for this feature as defined in the plan.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel
- **[Story]**: Which user story this task belongs to (e.g., US1, US2)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic script structure.

- [X] T001 [P] Create folder `Assets/Scripts/Player`
- [X] T002 [P] Create folder `Assets/Scripts/Interactables`
- [X] T003 [P] Create folder `Tests/PlayMode`
- [X] T004 [P] Create empty C# script `Assets/Scripts/Player/PlayerMovement.cs`
- [X] T005 [P] Create empty C# script `Assets/Scripts/Interactables/GravitySwitch.cs`
- [X] T006 [P] Create empty C# script `Assets/Scripts/Interactables/Cube.cs`
- [X] T007 [P] Create empty C# Test script `Tests/PlayMode/PlayerMovementTests.cs`
- [ ] T007a [P] Import DOTween asset into the project (e.g., from Unity Asset Store).

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core interface that other scripts will depend on.

- [X] T008 Create C# interface `Assets/Scripts/Interactables/IInteractable.cs` with an `Interact()` method signature.

---

## Phase 3: User Story 1 - 기본 이동 및 점프 (Priority: P1) 🎯 MVP

**Goal**: Implement basic player movement (left, right) and jumping.

**Independent Test**: A player character in a test scene can move left and right and jump. This can be verified without any interaction objects.

### Tests for User Story 1 ⚠️

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T009 [US1] In `Tests/PlayMode/PlayerMovementTests.cs`, write a test to verify that the player moves left when 'A' is pressed.
- [X] T010 [US1] In `Tests/PlayMode/PlayerMovementTests.cs`, write a test to verify that the player moves right when 'D' is pressed.
- [X] T011 [US1] In `Tests/PlayMode/PlayerMovementTests.cs`, write a test to verify that the player jumps when 'Spacebar' or 'W' is pressed.

### Implementation for User Story 1

- [X] T012 [US1] In `Assets/Scripts/Player/PlayerMovement.cs`, add fields for `moveSpeed`, `jumpForce`, `groundLayer`, and `groundCheck` from `data-model.md`.
- [X] T013 [US1] In `Assets/Scripts/Player/PlayerMovement.cs`, implement the logic to read 'A' and 'D' key inputs and use `transform.DOMoveX()` for horizontal movement.
- [X] T014 [US1] In `Assets/Scripts/Player/PlayerMovement.cs`, implement the `isGrounded` check using `Physics2D.OverlapCircle` with `groundCheck` and `groundLayer`.
- [X] T015 [US1] In `Assets/Scripts/Player/PlayerMovement.cs`, implement the jump logic to use `transform.DOJump()` when 'Spacebar' or 'W' is pressed and `isGrounded` is true.

**Checkpoint**: At this point, User Story 1 should be fully functional and all its tests should pass.

---

## Phase 4: User Story 2 - 오브젝트 상호작용 (Priority: P2)

**Goal**: Implement player interaction with `GravitySwitch` and `Cube` objects.

**Independent Test**: In a test scene with a switch and a cube, the player can activate the switch, and can then push/pull the cube.

### Tests for User Story 2 ⚠️

- [X] T016 [P] [US2] In `Tests/PlayMode/PlayerMovementTests.cs`, write a test to verify that interacting with a `GravitySwitch` changes `Physics2D.gravity`.
- [X] T017 [P] [US2] In `Tests/PlayMode/PlayerMovementTests.cs`, write a test to verify that the player cannot move a `Cube` when the switch is off.
- [X] T018 [P] [US2] In `Tests/PlayMode/PlayerMovementTests.cs`, write a test to verify that the player can push/pull a `Cube` when the switch is on.

### Implementation for User Story 2

- [X] T019 [US2] In `Assets/Scripts/Interactables/GravitySwitch.cs`, implement the `IInteractable` interface. The `Interact()` method should toggle its state and change `Physics2D.gravity`.
- [X] T019a [US2] Create a new script `Assets/Scripts/GameEvents.cs` for a static event system to manage communication between objects like the switch and the cube.
- [X] T020 [US2] In `Assets/Scripts/Interactables/Cube.cs`, implement the `IInteractable` interface. The `Interact()` method will manage its `canBeMoved` state based on notifications from the `GravitySwitch`.
- [X] T021 [US2] In `Assets/Scripts/Player/PlayerMovement.cs`, implement the interaction logic. In `Update()`, detect nearby `IInteractable` objects (e.g., using `Physics2D.OverlapCircle`).
- [X] T022 [US2] In `Assets/Scripts/Player/PlayerMovement.cs`, when 'Shift' is pressed, call the `Interact()` method on the detected nearby object.
- [X] T023 [US2] In `Assets/Scripts/Player/PlayerMovement.cs`, implement the push/pull logic for the cube when interaction is active.

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently and pass all tests.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Final cleanup and validation.

- [X] T024 [P] Add comments to complex logic in `PlayerMovement.cs` and `GravitySwitch.cs` as per the constitution.
- [X] T025 Refactor code for clarity and to remove any magic numbers or hardcoded values.
- [X] T026 Perform final validation by following all steps in `specs/001-player-movement-interaction/quickstart.md`.

---

## Dependencies & Execution Order

- **Phase 1 (Setup)** must be completed first.
- **Phase 2 (Foundational)** depends on Phase 1.
- **Phase 3 (US1)** depends on Phase 2.
- **Phase 4 (US2)** depends on Phase 3, as the player needs to be able to move to the objects to interact with them.
- **Phase 5 (Polish)** depends on all previous phases.

Within each user story, tests should be written first, followed by implementation.
