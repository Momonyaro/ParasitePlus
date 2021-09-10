using System.Collections;
using System.Collections.Generic;
using Scriptables;
using UnityEngine;

namespace BattleSystem
{
    public class BattleSystemEnemyField : MonoBehaviour
    {
        //This script manages the spawned graphical components of the enemies. Spawning them, animating them, destroying them... All of it.

        public float[] fieldShading = new float[5];
        public Transform[] fieldPositions = new Transform[5];
        public Transform multiPositionTarget;

        [Space()]
        
        public AnimationCurve enemyShakeCurve = new AnimationCurve();
        public Color enemyDamageColor = Color.red;
        public AnimationCurve enemyFadeoutCurve = new AnimationCurve();
        
        private const string ResourcePath = "Entities/";

        public void PopulateField(EntityScriptable[] enemies)
        {
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] == null) 
                { 
                    SetShadowVisibility(i, false); 
                    continue; 
                }

                GameObject fetched = Resources.Load<GameObject>(ResourcePath + enemies[i].entityId);

                Debug.Log($"[EnemyField] : fetchedIsNull: [{fetched == null}], searchedPath: {ResourcePath + enemies[i].entityId}");
                
                Instantiate(fetched, fieldPositions[i].GetChild(0));
                SetShadowVisibility(i, true);
            }
        }

        public void WipeField()
        {
            for (int i = 0; i < fieldPositions.Length; i++)
            {
                RemoveFieldObject(i);
            }
        }

        public void AddObjectToField(int index, string entityId, bool replaceIfOccupied = false)
        {
            if (PositionOccupied(index))
            {
                if (replaceIfOccupied) RemoveFieldObject(index);
                else
                {
                    return;
                }
            }
            
            GameObject fetched = Resources.Load<GameObject>(ResourcePath + entityId);

            Instantiate(fetched, fieldPositions[index].GetChild(0));
            SetShadowVisibility(index, true);
        }

        public void ShadeEntireField()
        {
            for (int i = 0; i < fieldPositions.Length; i++)
            {
                if (PositionOccupied(i))
                {
                    ShadeObject(i);
                }
            }
        }
        
        public void RemoveFieldObject(int index)
        {
            if (!PositionOccupied(index)) return; // No object at position, return
            
            Destroy(fieldPositions[index].GetChild(0).GetChild(0).gameObject);
            SetShadowVisibility(index, false); 
        }

        public bool PositionOccupied(int index)
        {
            return (fieldPositions[index].GetChild(0).childCount != 0);
        }

        private void SetShadowVisibility(int index, bool visible)
        {
            if (fieldPositions.Length > index)
                fieldPositions[index].GetChild(1).gameObject.SetActive(visible);
        }

        private void ShadeObject(int index)
        {
            if (PositionOccupied(index))
            {
                Color sub = Color.white - new Color(fieldShading[index], fieldShading[index], fieldShading[index], 0);
                GetEnemySpriteObject(index).GetComponent<SpriteRenderer>().color = sub;
            }
        }

        public Vector2 GetEntityPosAsScreenPos(int index, bool multiTargetPos = false)
        {
            if (multiTargetPos)
                return Camera.main.WorldToScreenPoint(multiPositionTarget.position);
                
            if (PositionOccupied(index))
            {
                return Camera.main.WorldToScreenPoint(
                    GetEnemySpriteObject(index).position);
            }
            return Vector2.zero;
        }
        
        public int MoveToNextEnemy(int currentIndex, int movementDelta)
        {
            int searchIndex = currentIndex + movementDelta;
            while (true)
            {
                if (searchIndex >= fieldPositions.Length) searchIndex = searchIndex % fieldPositions.Length;
                if (searchIndex < 0) searchIndex = fieldPositions.Length - 1;

                if (fieldPositions[searchIndex].GetChild(0).childCount > 0)
                {
                    if (GetEnemySpriteObject(searchIndex).gameObject.activeInHierarchy)
                    {
                        return searchIndex;
                    }
                }

                searchIndex += movementDelta; // move it another step in the direction we want if we did not find anything.
            }
        }

        public void EnemyDamageShake(int index, float timeScale, float magnitude)
        {
            if (PositionOccupied(index))
            {
                StartCoroutine(DamageShakeEnumerator(GetEnemySpriteObject(index), timeScale, magnitude, index));
            }
        }

        public void EnemyDeathFade(int index)
        {
            if (PositionOccupied(index))
            {
                StartCoroutine(EnemyDeathFadeout(GetEnemySpriteObject(index).GetComponent<SpriteRenderer>(), index));
                StartCoroutine(DamageShakeEnumerator(GetEnemySpriteObject(index), 2.54f, 0.7f, index));
            }
        }

        private IEnumerator DamageShakeEnumerator(Transform targetTransform, float timeScale, float magnitude, int index)
        {
            Transform parent = targetTransform.parent;
            Vector3 origo = Vector3.zero;
            float timePassed = 0;
            float lerpCurveLastX = enemyShakeCurve.keys[enemyShakeCurve.length - 1].time;
            
            SpriteRenderer graphic = targetTransform.GetComponent<SpriteRenderer>();
            graphic.color = Color.white;
            parent.localPosition = origo;
            
            while (timePassed < lerpCurveLastX)
            {
                Vector3 shakeVec = new Vector3(enemyShakeCurve.Evaluate(timePassed), // Walk x normally
                    enemyShakeCurve.Evaluate(lerpCurveLastX - timePassed),     // Walk width - x, basically from max to 0
                    enemyShakeCurve.Evaluate((timePassed + (lerpCurveLastX * 0.5f) % lerpCurveLastX))); // Walk x normally but start halfway.

                var currentPos = parent.localPosition;
                var currentCol = graphic.color;
                Vector3 lerped = Vector3.Lerp(currentPos, origo + (shakeVec * magnitude), 0.5f);
                Color lerpCol = Color.Lerp(currentCol, enemyDamageColor, Time.deltaTime * 5);
                
                parent.localPosition = lerped;
                graphic.color = lerpCol;
                
                timePassed += Time.deltaTime * timeScale;
                timePassed = Mathf.Clamp(timePassed, 0, lerpCurveLastX);
                yield return new WaitForEndOfFrame();
            }

            parent.localPosition = origo;
            ShadeObject(index);
            
            yield break;
        }

        private IEnumerator EnemyDeathFadeout(SpriteRenderer spriteRenderer, int index)
        {
            Color startCol = spriteRenderer.color;
            SpriteRenderer shadowRenderer = fieldPositions[index].GetChild(1).GetComponent<SpriteRenderer>();

            float timePassed = 0;
            while (startCol.a > 0)
            {
                startCol.a = enemyFadeoutCurve.Evaluate(Mathf.Clamp(timePassed, 0.0f, 1.0f));
                spriteRenderer.color = startCol;
                shadowRenderer.color = startCol;

                timePassed += Time.deltaTime;

                yield return new WaitForEndOfFrame();
            }
            
            RemoveFieldObject(index);
            
            yield break;
        }

        private Transform GetEnemySpriteObject(int index)
        {
            return fieldPositions[index].GetChild(0).GetChild(0).GetChild(0);
        }
    }
}
