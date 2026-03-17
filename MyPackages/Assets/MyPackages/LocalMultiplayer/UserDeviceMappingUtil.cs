using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;


public interface IGeneratedInputActionAsset
{
    public IInputActionCollection2 CreateNewGeneratedInputActionAsset();
}

public readonly struct InputActionAssetAndUserData
{
    public InputActionAsset UserInputActions { get; }

    public InputUserData UserData { get; }

    public InputActionAssetAndUserData(InputActionAsset userInputActions, InputUserData userData)
    {
        UserInputActions = userInputActions;
        UserData = userData;
    }
}

public readonly struct InputActionCollectionAndUserData
{
    public IInputActionCollection2 UserInputActions { get; }

    public InputUserData UserData { get; }

    public InputActionCollectionAndUserData(IInputActionCollection2 userInputActions, InputUserData userData)
    {
        UserInputActions = userInputActions;
        UserData = userData;
    }
}

public class InputUserData
{
    /// <summary>
    /// Dynamic index representing the current order of users (reorders when someone leaves).
    /// </summary>
    public int QueueIndex { get; }

    /// <summary>
    /// Fixed player slot (e.g., Player 1, Player 2, etc.), reused when empty.
    /// </summary>
    public int SlotIndex { get; }

    public int Id { get; }

    public InputDevice Device { get; }

    public InputUserData(int queueIndex, int slotIndex, int id, InputDevice device)
    {
        QueueIndex = queueIndex;
        SlotIndex = slotIndex;
        Id = id;
        Device = device;
    }
}

//This script handles the creation of a local multiplayer lobby with 1 keyboard and mouse and any number of controllers.

//NOTE: I have only tested this with KBM and Gamepads, it is unknown how many devices this will work for.
public static class UserDeviceMappingUtil
{
    static List<InputDevice> inputDevicesPairedWithUsers = new();

    static Dictionary<int, int> userSlots = new();

    /// <summary>
    /// This method creates a new user, binds the user to the most recently used device and then assigns a unique input action asset to that user.
    /// </summary>
    public static bool TryCreateUser(InputDevice device, InputActionAsset inputActionAsset, out InputActionAssetAndUserData inputActionsAndUser)
    {
        var inputDevices = new List<InputDevice>();

        if (inputDevicesPairedWithUsers.Contains(device))
        {
            inputActionsAndUser = new(null, default);
            return false;
        }
            
        var controlScheme = ControlSchemeSetup(inputActionAsset.controlSchemes, device, inputDevices);

        var user = InputUser.CreateUserWithoutPairedDevices();

        foreach (var inputDevice in inputDevices)
        {
            InputUser.PerformPairingWithDevice(inputDevice, user);
            inputDevicesPairedWithUsers.Add(inputDevice);
        }

        var userInputActions = InputActionAsset.FromJson(inputActionAsset.ToJson());

        user.AssociateActionsWithUser(userInputActions);

        user.ActivateControlScheme(controlScheme.Value.name);
                
        userInputActions.Enable();

        var slot = AssignUserSlot((int)user.id);

        inputActionsAndUser = new(userInputActions, new InputUserData(user.index, slot, (int)user.id, device));
        
        return true;
    }


    /// <summary>
    /// This method creates a new user, binds the user to the most recently used device and then accepts a list of input actions to bind to.
    /// </summary>
    public static bool TryCreateUser(InputDevice device, IGeneratedInputActionAsset generatedInputActionAsset, out InputActionCollectionAndUserData inputActionsAndUser)
    {
        var inputDevices = new List<InputDevice>();

        if (inputDevicesPairedWithUsers.Contains(device))
        {
            Debug.Log("failed to get device");

            inputActionsAndUser = new(null, default);

            return false;
        }

        var userInputActions = generatedInputActionAsset.CreateNewGeneratedInputActionAsset();

        var controlScheme = ControlSchemeSetup(userInputActions.controlSchemes, device, inputDevices);

        var user = InputUser.CreateUserWithoutPairedDevices();

        foreach (var inputDevice in inputDevices)
        {
            InputUser.PerformPairingWithDevice(inputDevice, user);
            inputDevicesPairedWithUsers.Add(inputDevice);
        }

        user.AssociateActionsWithUser(userInputActions);

        user.ActivateControlScheme(controlScheme.Value.name);

        userInputActions.Enable();

        var slot = AssignUserSlot((int)user.id);

        inputActionsAndUser = new(userInputActions, new InputUserData(user.index, slot, (int)user.id, device));

        return true;
    }

    /// <summary>
    /// Delete the paired user of the most recently used device.
    /// </summary>
    public static bool TryDeleteUser(InputDevice device, out InputUserData userData)
    {
        var userToRemove = InputUser.FindUserPairedToDevice(device).Value;

        if (userToRemove == null)
        {
            Debug.LogError($"No paired user was found for the following device: {device}");
            userData = null;
            return false;
        }

        if (!userToRemove.valid)
        {
            Debug.LogError($"The user paired with the device {device} is invalid.");
            userData = null;
            return false;
        }

        var slot = userSlots.FirstOrDefault(x => x.Value == (int)userToRemove.id).Key;

        userData = new InputUserData(userToRemove.index, slot, (int)userToRemove.id, device);

        userSlots[slot] = -1;

        userToRemove.actions.Disable();
        userToRemove.UnpairDevicesAndRemoveUser();


        for (int i = inputDevicesPairedWithUsers.Count - 1; i >= 0; i--) 
        {
            var pairedDevice = inputDevicesPairedWithUsers[i];

            if ((device is Mouse || device is Keyboard) && (pairedDevice is Mouse || pairedDevice is Keyboard))
            {
                inputDevicesPairedWithUsers.Remove(pairedDevice);
            }
            else if (pairedDevice == device)
            {
                inputDevicesPairedWithUsers.Remove(pairedDevice);
            }
        }

        return true;
    }

    public static void DeleteAllUsers()
    {
        // Get all current users
        var allUsers = InputUser.all;

        // Loop through and remove each user
        for (int i = allUsers.Count - 1; i >= 0; i--)
        {
            var user = allUsers[i];

            // Check if the user is valid
            if (user.valid)
            {
                // Disable user actions before removal
                user.actions.Disable();

                // Unpair devices and remove the user
                user.UnpairDevicesAndRemoveUser();
            }
            else
            {
                Debug.LogWarning($"Attempted to remove an invalid user: {user}");
            }
        }

        inputDevicesPairedWithUsers.Clear();
        userSlots.Clear();
    }

    /// <summary>
    /// Returns true if device is paired with user.
    /// </summary>
    public static bool IsDevicePairedWithUser(InputDevice device)
    {
        return inputDevicesPairedWithUsers.Contains(device);
    }

    
    /// <summary>
    /// Assign a user to a user slot.
    /// </summary>
    private static int AssignUserSlot(int newUserId)
    {
        // Look for an empty slot
        foreach (var kvp in userSlots)
        {
            if (kvp.Value == -1)
            {
                userSlots[kvp.Key] = newUserId;
                return kvp.Key;
            }
        }

        // No empty slot found — create a new one
        int newSlot = userSlots.Count;
        userSlots[newSlot] = newUserId;

        return newSlot;
    }

    /// <summary>
    /// Searches the input action maps for control schemes and if the device exists, maps it to the control scheme.
    /// </summary>
    private static InputControlScheme? ControlSchemeSetup(IReadOnlyList<InputControlScheme> inputActions, InputDevice device, List<InputDevice> inputDevices) 
    {

        foreach (var controlScheme in inputActions)
        {
            // Check each device requirement in the control scheme
            bool deviceMatches = controlScheme.deviceRequirements.Any(requirement =>
                InputControlPath.Matches(requirement.controlPath, device));

            if (deviceMatches)
            {
                if (device is Mouse || device is Keyboard)
                {
                    inputDevices.Add(Keyboard.current);
                    inputDevices.Add(Mouse.current);
                }
                else
                {
                    inputDevices.Add(device);
                }

                return controlScheme; // Return the matching control scheme
            }
        }

        // Return null if no matching control scheme is found
        return null;
    }   

}
