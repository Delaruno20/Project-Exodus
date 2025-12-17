using Lightbug.CharacterControllerPro.Core;
using UnityEngine;

public class PlayerComponents
{
    public Camera PlayerCamera { get; }
    public CharacterActor CharacterActor { get; }
    public HierarchicalStateMachine HierarchicalStateMachine { get; }

    public PlayerComponents(CharacterActor characterActor, HierarchicalStateMachine hierarchicalStateMachine, Camera playerCamera)
    {
        CharacterActor = characterActor;
        HierarchicalStateMachine = hierarchicalStateMachine;
        PlayerCamera = playerCamera;
    }
}