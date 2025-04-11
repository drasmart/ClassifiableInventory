using System;
using UnityEngine;
using UnityEngine.Assertions;

#nullable enable

public class LibraryKeeper : MonoBehaviour
{
    public PlayerInventory? playerInventory;
    public Skill[] skills = Array.Empty<Skill>();

    public void OnEnable()
    {
        InstallSkills();
    }

    public void ReloadSkills()
    {
        Assert.IsNotNull(playerInventory);
        InstallSkills();
        playerInventory!.onDataUpdate?.Invoke();
    }

    private void InstallSkills()
    {
        Assert.IsNotNull(playerInventory);
        skills = playerInventory!.knownSkills.ToArray();
    }
}
