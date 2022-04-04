using System;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Collections.Generic;

/// <summary>
/// A simple sample showing how to use the Relay Allocation package. As the host, you can authenticate, request a relay allocation, get a join code and join the allocation.
/// </summary>
/// <remarks>
/// The sample is limited to calling the Relay Allocation Service and does not cover connecting the host game client to the relay using Unity Transport Protocol.
/// This will cause the allocation to be reclaimed about 10 seconds after creating it.
/// </remarks>
public class SimpleRelay : MonoBehaviour
{
    /// <summary>
    /// The textbox displaying the Player Id.
    /// </summary>
    public Text PlayerIdText;

    /// <summary>
    /// The dropdown displaying the region.
    /// </summary>
    public Dropdown RegionsDropdown;

    /// <summary>
    /// The textbox displaying the Allocation Id.
    /// </summary>
    public Text HostAllocationIdText;

    /// <summary>
    /// The textbox displaying the Join Code.
    /// </summary>
    public Text JoinCodeText;

    /// <summary>
    /// The textbox displaying the Allocation Id of the joined allocation.
    /// </summary>
    public Text PlayerAllocationIdText;

    private Guid hostAllocationId;
    private Guid playerAllocationId;
    private string joinCode = "n/a";
    private string playerId = "Not signed in";
    private List<Region> regions = new List<Region>();
    private List<string> regionOptions = new List<string>();

    async void Start()
    {
        await UnityServices.InitializeAsync();

        UpdateUI();
    }

    void UpdateUI()
    {
        PlayerIdText.text = playerId;
        RegionsDropdown.interactable = regions.Count > 0;
        RegionsDropdown.options?.Clear();
        RegionsDropdown.AddOptions(regionOptions);
        HostAllocationIdText.text = hostAllocationId.ToString();
        JoinCodeText.text = joinCode;
        PlayerAllocationIdText.text = playerAllocationId.ToString();
    }

    /// <summary>
    /// Event handler for when the Sign In button is clicked.
    /// </summary>
    public async void OnSignIn()
    {
        Debug.Log("Signing On");

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        playerId = AuthenticationService.Instance.PlayerId;

        UpdateUI();
    }

    /// <summary>
    /// Event handler for when the Get Regions button is clicked.
    /// </summary>
    public async void OnRegion()
    {
        Debug.Log("Host - Getting regions.");
        var allRegions = await Relay.Instance.ListRegionsAsync();
        regions.Clear();
        foreach (var region in allRegions)
        {
            Debug.Log(region.Id + ": " + region.Description);
            regionOptions.Add(region.Id);
            regions.Add(region);
        }
        UpdateUI();
    }

    /// <summary>
    /// Event handler for when the Allocate button is clicked.
    /// </summary>
    public async void OnAllocate()
    {
        Debug.Log("Host - Creating an allocation.");

        // Important: Once the allocation is created, you have ten seconds to BIND
        Allocation allocation = await Relay.Instance.CreateAllocationAsync(4, regions.Count > 0 ? regions[RegionsDropdown.value].Id : null);
        hostAllocationId = allocation.AllocationId;

        Debug.Log("Host Allocation ID: " + hostAllocationId.ToString());

        UpdateUI();
    }

    /// <summary>
    /// Event handler for when the Get Join Code button is clicked.
    /// </summary>
    public async void OnJoinCode()
    {
        Debug.Log("Host - Getting a join code for my allocation. I would share that join code with the other players so they can join my session.");

        try
        {
            joinCode = await Relay.Instance.GetJoinCodeAsync(hostAllocationId);
            Debug.Log("Host - Got join code: " + joinCode);
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex.Message + "\n" + ex.StackTrace);
        }

        UpdateUI();
    }

    /// <summary>
    /// Event handler for when the Join button is clicked.
    /// </summary>
    public async void OnJoin()
    {
        Debug.Log("Client - Joining host allocation using join code.");

        try
        {
            var joinAllocation = await Relay.Instance.JoinAllocationAsync(joinCode);
            playerAllocationId = joinAllocation.AllocationId;
            Debug.Log("Client Allocation ID: " + playerAllocationId.ToString());
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex.Message + "\n" + ex.StackTrace);
        }

        UpdateUI();
    }
}