using UnityEngine;

namespace CORE
{
    [CreateAssetMenu(fileName = "Game Config", menuName = "Game Config")]
    public class GameConfig : ScriptableObject
    {
        // This is basically where we store the important constants in the game so we can
        //    easily change them in one place and the rest of the game will update

        public int levelCap = 99;        //    Max Entity Level
        public int healthCap = 999;      //    Max Entity HP
        public int energyCap = 999;      //    Max Entity AP/SP
        public int statCap = 99;         //    Max Value of a single Stat

        public int inventorySpaces = 32; //    Max Party Inventory spaces (stacks count as one)
        public int walletCap = 9999;     //    The Max amount of money the player can carry at once

        public void DebugPing(object caller)
        {
            Debug.Log((caller.GetType()));
        }
    }
}
