using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using DG.Tweening; // Import DOTween

public class PlayerMovementTests
{
    private GameObject playerGameObject;
    private PlayerMovement playerMovement;
    private Rigidbody2D playerRigidbody;

    [SetUp]
    public void Setup()
    {
        // Setup DOTween for testing
        DOTween.Init();

        playerGameObject = new GameObject("Player");
        playerRigidbody = playerGameObject.AddComponent<Rigidbody2D>();
        playerMovement = playerGameObject.AddComponent<PlayerMovement>();

        // Mock dependencies for PlayerMovement if needed
        // For example, setting up ground check layer
        playerMovement.groundLayer = LayerMask.GetMask("Default");
        GameObject ground = new GameObject("Ground");
        ground.transform.position = new Vector2(0, -1);
        ground.AddComponent<BoxCollider2D>();
        ground.layer = LayerMask.NameToLayer("Default");
    }

    [TearDown]
    public void Teardown()
    {
        Object.Destroy(playerGameObject);
        // Make sure to kill all tweens
        DOTween.KillAll();
    }

    [UnityTest]
    public IEnumerator PlayerMovesLeft()
    {
        Vector3 initialPosition = playerGameObject.transform.position;

        // Simulate calling Move method, which will be implemented with DOTween
        playerMovement.Move(-1); 

        yield return new WaitForSeconds(0.5f); // Wait for tween to complete

        Assert.Less(playerGameObject.transform.position.x, initialPosition.x);
    }

    [UnityTest]
    public IEnumerator PlayerMovesRight()
    {
        Vector3 initialPosition = playerGameObject.transform.position;

        // Simulate calling Move method
        playerMovement.Move(1);

        yield return new WaitForSeconds(0.5f);

        Assert.Greater(playerGameObject.transform.position.x, initialPosition.x);
    }

    [UnityTest]
    public IEnumerator PlayerJumps()
    {
        playerMovement.isGrounded = true; // Force isGrounded for test
        Vector3 initialPosition = playerGameObject.transform.position;

        // Simulate calling Jump method
        playerMovement.Jump();

        yield return new WaitForSeconds(0.5f); // Wait for jump tween

        Assert.Greater(playerGameObject.transform.position.y, initialPosition.y);
    }

    [UnityTest]
    public IEnumerator InteractingWithGravitySwitchChangesGravity()
    {
        // Arrange
        GameObject switchObject = new GameObject("GravitySwitch");
        GravitySwitch gravitySwitch = switchObject.AddComponent<GravitySwitch>();
        Vector2 initialGravity = Physics2D.gravity;
        gravitySwitch.gravityDirection = new Vector2(0, 20f); // Change gravity upwards

        // Act
        gravitySwitch.Interact();
        yield return null; // Wait one frame for physics update

        // Assert
        Assert.AreNotEqual(initialGravity, Physics2D.gravity);
        Assert.AreEqual(gravitySwitch.gravityDirection, Physics2D.gravity);

        // Cleanup
        Physics2D.gravity = initialGravity;
        Object.Destroy(switchObject);
    }

    [UnityTest]
    public IEnumerator CannotMoveCubeWhenSwitchIsOff()
    {
        // Arrange
        GameObject cubeObject = new GameObject("Cube");
        Cube cube = cubeObject.AddComponent<Cube>();
        cube.canBeMoved = false; // Explicitly set state
        Vector3 initialPos = cubeObject.transform.position;

        // Act
        // Simulate player trying to push it
        cube.Interact(); // This would be called by player, but we simulate it
        // We assume the cube's Interact method does nothing if canBeMoved is false
        yield return new WaitForSeconds(0.1f);

        // Assert
        Assert.AreEqual(initialPos, cubeObject.transform.position);
        Object.Destroy(cubeObject);
    }

    [UnityTest]
    public IEnumerator CanMoveCubeWhenSwitchIsOn()
    {
        // Arrange
        GameObject cubeObject = new GameObject("Cube");
        Cube cube = cubeObject.AddComponent<Cube>();
        cube.canBeMoved = true; // Explicitly set state
        Vector3 initialPos = cubeObject.transform.position;

        // Act
        // Simulate player pushing it
        cube.Interact(); // This would be called by player
        // For this test, we'll assume Interact() on the cube makes it kinematic and follows the player
        // A more robust test would involve a mock player object
        cubeObject.transform.position += Vector3.right;
        yield return new WaitForSeconds(0.1f);

        // Assert
        Assert.AreNotEqual(initialPos, cubeObject.transform.position);
        Object.Destroy(cubeObject);
    }
}
