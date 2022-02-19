using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using System.Threading.Tasks;
using Unity.Services.Lobbies.Models;

public class GameManager : MonoBehaviour
{
    private string _lobbyId;
    public async void FindLobby()
    {
        Debug.Log("Looking for a lobby...");

        try
        {
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions();

            Lobby lobby = await Lobbies.Instance.QuickJoinLobbyAsync(options);
            Debug.Log("Joined lobby: " + lobby.Id);
            Debug.Log("Lobby Players: " + lobby.Players.Count);
        }
        catch(LobbyServiceException e)
        {
            Debug.Log("Cannot find a lobby: " + e);
            CreateLobby();
        }
    }

    private async void CreateLobby()
    {
        Debug.Log("Creating a new lobby");

        try
        {
            string lobbyName = "adinslobby";
            int maxPlayers = 2;
            CreateLobbyOptions options = new CreateLobbyOptions();
            options.IsPrivate = false;

            var lobby = await Lobbies.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            _lobbyId = lobby.Id;
            Debug.Log("Created lobby: " + lobby.Id);

            //We have to heartbeat the lobby or it shuts down
            StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15));
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTImeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTImeSeconds);
        while (true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            Debug.Log("Lobby Heartbeat sent");
            yield return delay;
        }
    }

    private void OnDestroy()
    {
        Lobbies.Instance.DeleteLobbyAsync(_lobbyId);

    }
    #region auth
    // Start is called before the first frame update
    async void Start()
    {
        await UnityServices.InitializeAsync();

        SetupEvents();

        await SignInAnonymouslyAsync();

    }

    // Update is called once per frame
    void SetupEvents()
    {
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log($"PlayerId: {AuthenticationService.Instance.PlayerId}");
            Debug.Log($"AccessToken: {AuthenticationService.Instance.AccessToken}");
        };

        AuthenticationService.Instance.SignInFailed += (error) =>
        {
            Debug.LogError(error);
        };

        AuthenticationService.Instance.SignedOut += () =>
        {
            Debug.Log("Player signed out.");
        };
    }

    async Task SignInAnonymouslyAsync()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Sign in anonymously succeeded!");

            // Shows how to get the playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException exception)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(exception);
        }
    }
    #endregion
}
