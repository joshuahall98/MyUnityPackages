using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;


public interface IGeneratedInputActionAsset
{
    public IInputActionCollection2 CreateNewGeneratedInputActionAsset();
}

//This script handles the creation of a local multiplayer lobby with 1 keyboard and mouse and any number of controllers.

//NOTE: I have only tested this with KBM and Gamepads, it is unknown how many devices this will work for.
public static class UserDeviceMappingUtil
{
   
    static List<InputDevice> inputDevicesPairedWithUsers = new List<InputDevice>();

    /// <summary>
    /// This method creates a new user, binds the user to the most recently used device and then assigns a unique input action asset to that user.
    /// </summary>
    public static bool TryCreateUser(InputDevice device, InputActionAsset inputActionAsset, out InputActionAsset userInputActions)
    {
        var inputDevices = new List<InputDevice>();

        if (inputDevicesPairedWithUsers.Contains(device))
        {
            userInputActions = null;
            return false;
        }
            
        var controlScheme = ControlSchemeSetup(inputActionAsset.controlSchemes, device, inputDevices);

        var user = InputUser.CreateUserWithoutPairedDevices();

        foreach (var inputDevice in inputDevices)
        {
            InputUser.PerformPairingWithDevice(inputDevice, user);
            inputDevicesPairedWithUsers.Add(inputDevice);
        }

        userInputActions = InputActionAsset.FromJson(inputActionAsset.ToJson());

        user.AssociateActionsWithUser(userInputActions);

        user.ActivateControlScheme(controlScheme.Value.name);
                
        userInputActions.Enable();

        return true;
    }


    /// <summary>
    /// This method creates a new user, binds the user to the most recently used device and then accepts a list of input actions to bind to. the uniqueness of the input actions are defined by the caller.
    /// </summary>
    public static bool TryCreateUser(InputDevice device, IGeneratedInputActionAsset generatedInputActionAsset, out IInputActionCollection2 userInputActions)
    {
        var inputDevices = new List<InputDevice>();

        if (inputDevicesPairedWithUsers.Contains(device))
        {
            userInputActions = null;
            return false;
        }

        userInputActions = generatedInputActionAsset.CreateNewGeneratedInputActionAsset();

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

        return true;
    }

    /// <summary>
    /// Delete the paired user of the most recently used device.
    /// </summary>
    public static bool TryDeleteUser(InputDevice device)
    {

        var userToRemove = InputUser.FindUserPairedToDevice(device).Value;


        if (userToRemove == null)
        {
            Debug.LogError($"No paired user was found for the following device: {device}");
            return false;
        }

        if (!userToRemove.valid)
        {
            Debug.LogError($"The user paired with the device {device} is invalid.");
            return false;
        }

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
