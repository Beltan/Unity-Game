using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Completed;

    //Enemy inherits from MovingObject, our base class for objects that can move, Player also inherits from this.
    public class Cell : MonoBehaviour
    {

        public float moveTime = 0.01f;           //Time it will take object to move, in seconds.
        public BoxCollider2D boxCollider;      //The BoxCollider2D component attached to this object.
        public Rigidbody2D rb2D;               //The Rigidbody2D component attached to this object.
        
        public LayerMask blockingLayer;
        public LayerMask items;
        public int range;
        public int wallMultiplier;
        public int wallDivider;
        public int enemyOffset;
        public int enemyMultiplier;
        public int enemyDivider;
        public int foodOffset;
        public int foodMultiplier;
        public int foodDivider;
        public int cellMultiplier;
        public int cellDivider;
        public int foodCount = 0;
        public int starvationCount = 0;

        //Start overrides the virtual Start function of the base class.
        void Start ()
        {
            items = LayerMask.GetMask("Items"); 
            //Register this enemy with our instance of GameManager by adding it to a list of Enemy objects. 
            //This allows the GameManager to issue movement commands.
            GameManager.instance.AddCellToList(this);

            //Get a component reference to this object's BoxCollider2D
            boxCollider = GetComponent <BoxCollider2D> ();
            
            //Get a component reference to this object's Rigidbody2D
            rb2D = GetComponent <Rigidbody2D> ();
        }
        
        
        //MoveEnemy is called by the GameManger each turn to tell each Enemy to try to move towards the player.
        public void MoveCell()
        {
            Vector2 myPosition = transform.position;
            List <Vector2> possibleMoves = GetMoves(myPosition);

            List <GameObject> seenObjects = SeenObjects();

            Boolean destroyed = false;

            float bestPositionScore = Heuristic(myPosition, seenObjects);
            Vector2 bestPosition = myPosition;

            for (int i = 0; i < possibleMoves.Count; i++) {
                float aspirantPositionScore = Heuristic(possibleMoves[i], seenObjects);
                if (aspirantPositionScore > bestPositionScore) {
                  bestPosition = possibleMoves[i];
                  bestPositionScore = aspirantPositionScore;
                }
            }
            RaycastHit2D hit = Physics2D.Linecast(bestPosition, bestPosition, items);
            GameObject target = null;
            if (hit.transform != null) target = hit.transform.gameObject;
            if (target != null && target.name == "Food(Clone)") {
                foodCount++;
                starvationCount = 0;
                Destroy(target);
            }
            else if (target != null && target.name == "Enemy(Clone)") {
                Destroy(target);
                destroyed = true;
            }
            if (destroyed || starvationCount == 12) {
                hit = Physics2D.Linecast(myPosition, myPosition, blockingLayer);
                target = hit.transform.gameObject;
                Destroy(target);
            } else {
                starvationCount++;
                Move(bestPosition);
            }
        }

        private List <Vector2> GetMoves(Vector2 myPosition) {
            List <Vector2> possibleMoves = new List <Vector2>();
            Vector2 end = new Vector2 (0, 0);

            boxCollider.enabled = false;

            for (int i = 0; i < 4; i++) {
                switch (i) {
                    case 0:
                        end = myPosition + new Vector2 (0.4f, 0);
                        break;
                    case 1:
                        end = myPosition + new Vector2 (-0.4f, 0);
                        break;
                    case 2:
                        end = myPosition + new Vector2 (0, 0.4f);
                        break;
                    case 3:
                        end = myPosition + new Vector2 (0, -0.4f);
                        break;
                }

                RaycastHit2D hit = Physics2D.Linecast(myPosition, end, blockingLayer);

                //Check if anything was hit
                if(hit.transform == null)
                {
                    possibleMoves.Add(end);
                }
            }

            boxCollider.enabled = true;

            return possibleMoves;
        }

        private List <GameObject> SeenObjects() {
            List <GameObject> seenObjects = new List <GameObject>();
            Vector2 myPosition = transform.position;
            for (int i = -range; i <= range; i++) {
                for (int j = -range; j <= range; j++) {
                    if (Math.Abs(i) + Math.Abs(j) <= range) {
                        Vector2 start = new Vector2 (myPosition.x + i * 0.4f, myPosition.y + j * 0.4f);
                        Vector2 end = new Vector2 (myPosition.x + i * 0.4f, myPosition.y + j * 0.4f);
                        RaycastHit2D hit = Physics2D.Linecast(start, end, items);
                        if (hit.transform == null) {
                            hit = Physics2D.Linecast(start, end, blockingLayer);
                        }
                        if (hit.transform != null) {
                            seenObjects.Add(hit.transform.gameObject);
                        }
                    }
                }
            }
            return seenObjects;
        }
        
        private float Heuristic(Vector2 position, List <GameObject> seenObjects) {
            float score = 0;
            for (int i = 0; i < seenObjects.Count; i++) {
                float x = Math.Abs(seenObjects[i].transform.position.x - position.x) / 0.4f;
                float y = Math.Abs(seenObjects[i].transform.position.y - position.y) / 0.4f;
                float distance = Mathf.Round(x + y);
                switch (seenObjects[i].name) {
                    case "Wall(Clone)":
                        //score += wallMultiplier * distance + wallDivider / distance;
                        break;
                    case "Enemy(Clone)":
                        if (distance == 0) score += enemyOffset;
                        else score += enemyMultiplier * distance + enemyDivider / distance;
                        break;
                    case "Food(Clone)":
                        if (distance == 0) score += foodOffset;
                        else score += foodMultiplier * distance + foodDivider / distance;
                        break;
                    case "Cell(Clone)":
                        //score += cellMultiplier * distance + cellDivider / distance;
                        break;
                }
            }
            return score;
        }

        void Move(Vector2 end)
        {            
            rb2D.MovePosition(end);
        }
    }