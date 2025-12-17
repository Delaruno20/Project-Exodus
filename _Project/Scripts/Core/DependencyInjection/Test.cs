using System;
using UnityEngine;

public class Test : MonoBehaviour
{
   private UpdateMovementAnimationActivity _updateAnimationActivity;


   private void Start()
   {
      _updateAnimationActivity = DependencyInjectionManager.Instance.Container.Resolve<UpdateMovementAnimationActivity>();
   }

   private void Update()
   {
      _updateAnimationActivity?.Update();
   }
}

public interface IAnimatorService
{
   Animator Animator { get; }
}

public class AnimatorService : IAnimatorService
{
   public Animator Animator { get; }

   public AnimatorService(Animator animator)
   {
      Animator = animator;
   }
}