using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.InputSystem;

public class RebindOperationUtilityTests
{
    private InputActionMap actionMap;
    private InputAction moveAction;
    private InputAction jumpAction;
    private Keyboard keyboard;

    [SetUp]
    public void SetUp()
    {
        actionMap = new InputActionMap("TestMap");

        moveAction = actionMap.AddAction("Move");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/d");

        jumpAction = actionMap.AddAction("Jump", binding: "<Keyboard>/space");

        keyboard = InputSystem.AddDevice<Keyboard>();
    }

    [TearDown]
    public void TearDown()
    {
        InputSystem.RemoveDevice(keyboard);
        actionMap.Dispose();
    }

    [Test]
    public void CollectCompositeParts_GroupsBindingsByPartInOrder()
    {
        List<CompositePartBinding> parts = new List<CompositePartBinding>();

        RebindOperationUtility.CollectCompositeParts(moveAction, 0, parts);

        Assert.AreEqual(4, parts.Count);
        Assert.AreEqual("up", parts[0].PartName);
        Assert.AreEqual("down", parts[1].PartName);
        Assert.AreEqual("left", parts[2].PartName);
        Assert.AreEqual("right", parts[3].PartName);
        CollectionAssert.AreEqual(new[] { 3, 4 }, parts[2].BindingIndices);
    }

    [Test]
    public void IsControlUsedInMap_DetectsControlBoundToAnotherAction()
    {
        List<int> excludedIndices = new List<int> { 0 };

        Assert.IsTrue(RebindOperationUtility.IsControlUsedInMap(keyboard.wKey, jumpAction, excludedIndices));
    }

    [Test]
    public void IsControlUsedInMap_IgnoresExcludedBindingsOfTheSameAction()
    {
        List<int> excludedIndices = new List<int> { 0 };

        Assert.IsFalse(RebindOperationUtility.IsControlUsedInMap(keyboard.spaceKey, jumpAction, excludedIndices));
    }

    [Test]
    public void IsControlUsedInMap_ReturnsFalseForFreeControl()
    {
        List<int> excludedIndices = new List<int> { 0 };

        Assert.IsFalse(RebindOperationUtility.IsControlUsedInMap(keyboard.qKey, jumpAction, excludedIndices));
    }

    [Test]
    public void PathMatchesControl_RecognizesDeviceSpecificPathsOnTheSameLayout()
    {
        Keyboard legacyKeyboard = InputSystem.AddDevice<Keyboard>("LegacyKeyboard");

        try
        {
            Assert.IsTrue(RebindOperationUtility.PathMatchesControl("/LegacyKeyboard/w", keyboard.wKey));
            Assert.IsTrue(RebindOperationUtility.PathMatchesControl("/Keyboard/w", keyboard.wKey));
            Assert.IsFalse(RebindOperationUtility.PathMatchesControl("/LegacyKeyboard/s", keyboard.wKey));
            Assert.IsFalse(RebindOperationUtility.PathMatchesControl("/Mouse/leftButton", keyboard.wKey));
        }
        finally
        {
            InputSystem.RemoveDevice(legacyKeyboard);
        }
    }

    [Test]
    public void IsControlUsedInMap_DetectsLegacyDeviceSpecificOverrides()
    {
        moveAction.ApplyBindingOverride(1, "/Keyboard/w");
        List<int> excludedIndices = new List<int> { 0 };

        Assert.IsTrue(RebindOperationUtility.IsControlUsedInMap(keyboard.wKey, jumpAction, excludedIndices));
    }

    [Test]
    public void MatchesBinding_UsesGroupsAndGamepadPaths()
    {
        InputBinding keyboardBinding = new InputBinding("<Keyboard>/space", groups: "Keyboard&Mouse");
        InputBinding gamepadBinding = new InputBinding("<Gamepad>/buttonSouth");

        Assert.IsTrue(DeviceTypeUtility.MatchesBinding(keyboardBinding, DeviceType.KeyboardMouse));
        Assert.IsFalse(DeviceTypeUtility.MatchesBinding(keyboardBinding, DeviceType.Gamepad));
        Assert.IsTrue(DeviceTypeUtility.MatchesBinding(gamepadBinding, DeviceType.Gamepad));
        Assert.IsTrue(DeviceTypeUtility.MatchesBinding(gamepadBinding, DeviceType.Any));
    }
}
