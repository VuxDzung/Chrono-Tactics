using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class Authentication 
{
    public Action<string> OnSingedInWithId;

    private UnitySDK context;

    public string PlayerId => AuthenticationService.Instance.PlayerId;

    public Authentication(UnitySDK context, bool useLocalProfile, string profileId)
    {
        this.context = context;
        Initialized(context, useLocalProfile, profileId);
    }

    private async void Initialized(UnitySDK context, bool useLocalProfile, string profileId)
    {
        try
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync();

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    if (useLocalProfile) AuthenticationService.Instance.SwitchProfile(profileId);

                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    OnSingedInWithId?.Invoke(AuthenticationService.Instance.PlayerId);
                    Debug.Log($"Player Signed In: {AuthenticationService.Instance.PlayerId}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }
}
