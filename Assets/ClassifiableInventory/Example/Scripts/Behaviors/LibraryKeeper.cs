using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LibraryKeeper : MonoBehaviour
{
    public PlayerInventory playerInventory;
    public Skill[] skills;

    public void OnEnable()
    {
        InstallSkills();
    }

    public void ReloadSkills()
    {
        InstallSkills();
        playerInventory.onDataUpdate?.Invoke();
    }

    private void InstallSkills()
    {
        skills = playerInventory?.knownSkills?.ToArray();
    }
}
