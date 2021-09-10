using System;
using UnityEngine;

namespace MOVEMENT
{
    [CreateAssetMenu(fileName = "AnimationComp", menuName = "AnimationComp/Animation", order = 1)]
    public class AnimationScriptable : ScriptableObject
    {
        public Sprite[] towards = new Sprite[0];
        public Sprite[] away = new Sprite[0];
        public Sprite[] left = new Sprite[0];
        public Sprite[] right = new Sprite[0];

        private Facing lastFacing;
        private int lastFrame = 0;
        private float lastSpeed = 0.0f;
        [Range(0, 1)]public float lastMaxSpeed = 0.0f;
        private Sprite lastSprite = null;

        public Sprite TickAnimFromFacing(Facing currentFacing)
        {
            if (lastFacing != currentFacing)
            {
                lastFrame = -1;
                lastFacing = currentFacing;
                lastSpeed = 0;

                // switch (currentFacing)
                // {
                //     case Facing.TOWARDS: lastSpeed = towardsSpeed;
                //         lastMaxSpeed = towardsSpeed;
                //         break;
                //     case Facing.AWAY: lastSpeed = awaySpeed;
                //         lastMaxSpeed = awaySpeed;
                //         break;
                //     case Facing.LEFT: lastSpeed = leftSpeed;
                //         lastMaxSpeed = leftSpeed;
                //         break;
                //     case Facing.RIGHT: lastSpeed = rightSpeed;
                //         lastMaxSpeed = rightSpeed;
                //         break;
                // }
            }

            Sprite selected = lastSprite;
            
            lastSpeed -= Time.deltaTime;
            if (lastSpeed <= 0) lastSpeed = lastMaxSpeed;
            else
            {
                if (lastFacing != currentFacing)
                    switch (currentFacing)
                    {
                        case Facing.TOWARDS:
                            selected = towards[0];
                            break;
                    
                        case Facing.AWAY:
                            selected = away[0];
                            break;
                    
                        case Facing.LEFT:
                            selected = left[0];
                            break;
                    
                        case Facing.RIGHT:
                            selected = right[0];
                            break;
                    }
                
                return selected;
            }
            
            lastFrame++;
            
            switch (currentFacing)
            {
                case Facing.TOWARDS:
                    if (lastFrame >= towards.Length) lastFrame = 0;
                    selected = towards[lastFrame];
                    break;
                
                case Facing.AWAY:
                    if (lastFrame >= away.Length) lastFrame = 0;
                    selected = away[lastFrame];
                    break;
                
                case Facing.LEFT:
                    if (lastFrame >= left.Length) lastFrame = 0;
                    selected = left[lastFrame];
                    break;
                
                case Facing.RIGHT:
                    if (lastFrame >= right.Length) lastFrame = 0;
                    selected = right[lastFrame];
                    break;
            }


            lastSprite = selected;
            return selected;
        }
    }
}
