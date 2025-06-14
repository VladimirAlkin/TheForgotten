using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateList : MonoBehaviour
{
    public bool jumping = false;
    public bool dashing = false;
    public bool recoilingX, recoilingY;
    public bool lookingRight; //in case recoiling on X is needed
    public bool invincible = false;
    public bool casting;
    public bool alive = true;
    public bool cutscene = false;
}
