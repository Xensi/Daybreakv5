using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimatorDefines
{
    public static int loadingID = Animator.StringToHash("loading");
    public static int movingID = Animator.StringToHash("moving");

    public static int attackingID = Animator.StringToHash("attacking");
    public static int walkingID = Animator.StringToHash("walking");
    public static int deployedID = Animator.StringToHash("deployed");
    public static int speedID = Animator.StringToHash("speed");
    public static int aliveID = Animator.StringToHash("alive");
    public static int damagedID = Animator.StringToHash("damaged");
    public static int randomIdleID = Animator.StringToHash("randomIdle");
    public static int idleID = Animator.StringToHash("idle");
    public static int rowID = Animator.StringToHash("row");
    public static int randomAttackID = Animator.StringToHash("randomAttack");
    public static int meleeID = Animator.StringToHash("melee");
    public static int angleID = Animator.StringToHash("angle");
    public static int ammoID = Animator.StringToHash("ammo");
    public static int knockedDownID = Animator.StringToHash("knockedDown");
}
