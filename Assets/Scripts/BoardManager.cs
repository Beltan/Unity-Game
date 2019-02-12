using UnityEngine;
using System;
using System.Collections.Generic;       //Allows us to use Lists.
using Random = UnityEngine.Random;      //Tells Random to use the Unity Engine random number generator.

namespace Completed {
    
    public class BoardManager : MonoBehaviour {
        // Using Serializable allows us to embed a class with sub properties in the inspector.
        [Serializable]
        public class Count {
            public int minimum;             //Minimum value for our Count class.
            public int maximum;             //Maximum value for our Count class.
            
            //Assignment constructor.
            public Count (int min, int max) {
                minimum = min;
                maximum = max;
            }
        }
        public LayerMask items;
        public LayerMask blockingLayer;
        
        public int columns;                                         //Number of columns in our game board.
        public int rows;                                            //Number of rows in our game board.
        public GameObject[] foodTiles;                                  //Array of food prefabs.
        public GameObject[] enemyTiles;                                 //Array of enemy prefabs.
        public GameObject[] floorTiles;                                 //Array of floor prefabs.
        public GameObject[] outerWallTiles;                             //Array of outer tile prefabs.
        public GameObject[] cellTiles;                                  //Array of cell prefabs.

        private Transform boardHolder;                                  //A variable to store a reference to the transform of our Board object.
        private Transform boardItemsHolder;                                  //A variable to store a reference to the transform of our Board object.
        private List <Vector3> gridPositions = new List <Vector3> ();   //A list of possible locations to place tiles.
        

        void Start ()
        {
            items = LayerMask.GetMask("Items"); 
        }
        
        //Clears our list gridPositions and prepares it to generate a new board.
        void InitialiseList () {
            //Clear our list gridPositions.
            gridPositions.Clear ();
            
            //Loop through x axis (columns).
            for(int x = 1; x < columns - 1; x++) {
                //Within each column, loop through y axis (rows).
                for(int y = 1; y < rows - 1; y++) {
                    //At each index add a new Vector3 to our list with the x and y coordinates of that position.
                    gridPositions.Add (new Vector3(x, y, 0f));
                }
            }
        }
        
        
        //Sets up the outer walls and floor (background) of the game board.
        void BoardSetup () {
            //Instantiate Board and set boardHolder to its transform.
            boardHolder = new GameObject ("Board").transform;
            boardItemsHolder = new GameObject ("Board").transform;
            
            //Loop along x axis, starting from -1 (to fill corner) with floor or outerwall edge tiles.
            for(int x = 0; x < columns; x++) {
                //Loop along y axis, starting from -1 to place floor or outerwall tiles.
                for(int y = 0; y < rows; y++) {
                    //Choose a random tile from our array of floor tile prefabs and prepare to instantiate it.
                    GameObject toInstantiate = floorTiles[Random.Range (0,floorTiles.Length)];

                    if (x == 0 || x == columns - 1 || y == 0 || y == rows - 1) toInstantiate = outerWallTiles [Random.Range (0, outerWallTiles.Length)];
                    
                    //Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
                    GameObject instance = Instantiate (toInstantiate, new Vector3 (x, y, 0f), Quaternion.identity) as GameObject;
                    
                    //Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
                    instance.transform.SetParent (boardHolder);
                }
            }
        }
        
        
        //RandomPosition returns a random position from our list gridPositions.
        Vector3 RandomPosition () {
            //Declare an integer randomIndex, set it's value to a random number between 0 and the count of items in our List gridPositions.
            int randomIndex = Random.Range (0, gridPositions.Count);
            
            //Declare a variable of type Vector3 called randomPosition, set it's value to the entry at randomIndex from our List gridPositions.
            Vector3 randomPosition = gridPositions[randomIndex];
            
            //Remove the entry at randomIndex from the list so that it can't be re-used.
            gridPositions.RemoveAt (randomIndex);
            
            //Return the randomly selected Vector3 position.
            return randomPosition;
        }
        
        
        //LayoutObjectAtRandom accepts an array of game objects to choose from along with a minimum and maximum range for the number of objects to create.
        void LayoutObjectAtRandom (GameObject[] tileArray, int minimum, int maximum) {
            //Choose a random number of objects to instantiate within the minimum and maximum limits
            int objectCount = Random.Range (minimum, maximum+1);
            
            //Instantiate objects until the randomly chosen limit objectCount is reached
            for(int i = 0; i < objectCount; i++) {
                //Choose a position for randomPosition by getting a random position from our list of available Vector3s stored in gridPosition
                Vector3 randomPosition = RandomPosition();
                
                //Choose a random tile from tileArray and assign it to tileChoice
                GameObject tileChoice = tileArray[Random.Range (0, tileArray.Length)];
                
                //Instantiate tileChoice at the position returned by RandomPosition with no change in rotation
                GameObject instance = Instantiate(tileChoice, randomPosition, Quaternion.identity);

                if (tileChoice.name == "Cell") {
                                          Debug.Log("Init");
                    initGenes(instance);
                }

                //Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
                instance.transform.SetParent(boardItemsHolder);
            }
        }

        void LayoutObjectAtRandomNoCollision (GameObject[] tileArray, int minimum, int maximum, float xscale, float yscale) {
            //Choose a random number of objects to instantiate within the minimum and maximum limits
            int objectCount = Random.Range (minimum, maximum+1);
            
            //Instantiate objects until the randomly chosen limit objectCount is reached
            for(int i = 0; i < objectCount; i++) {
                //Choose a position for randomPosition by getting a random position from our list of available Vector3s stored in gridPosition
                Vector3 randomPosition = RandomPosition();
                Vector2 position = new Vector2(randomPosition.x * 0.4f, randomPosition.y * 0.4f);
                RaycastHit2D hit = Physics2D.Linecast(position, position, items);
                if (hit.transform == null) {
                    hit = Physics2D.Linecast(position, position, blockingLayer);
                }
                if (hit.transform == null) {
                    //Choose a random tile from tileArray and assign it to tileChoice
                    GameObject tileChoice = tileArray[Random.Range (0, tileArray.Length)];
                    
                    //Instantiate tileChoice at the position returned by RandomPosition with no change in rotation
                    GameObject instance = Instantiate(tileChoice, position, Quaternion.identity);
                    

                    instance.transform.localScale = new Vector3(0.4f * xscale, 0.4f * yscale, 0);

                    //Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
                    instance.transform.SetParent(boardItemsHolder);
                }
            }
        }

        void LayoutCellAtRandomNoCollision (GameObject cell, int minimum, int maximum, float xscale, float yscale) {
            
            //Choose a position for randomPosition by getting a random position from our list of available Vector3s stored in gridPosition
            Vector3 randomPosition = RandomPosition();
            Vector2 position = new Vector2(randomPosition.x * 0.4f, randomPosition.y * 0.4f);
            RaycastHit2D hit = Physics2D.Linecast(position, position, items);
            if (hit.transform == null) {
                hit = Physics2D.Linecast(position, position, blockingLayer);
            }
            if (hit.transform == null) {
                //Choose a random tile from tileArray and assign it to tileChoice
                GameObject tileChoice = cellTiles[Random.Range (0, cellTiles.Length)];
                
                //Instantiate tileChoice at the position returned by RandomPosition with no change in rotation
                GameObject instance = Instantiate(cell, position, Quaternion.identity);

                instance.transform.localScale = new Vector3(0.4f * xscale, 0.4f * yscale, 0);

                instance.name = "Cell(Clone)";

                setGenes(instance);

                //Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
                instance.transform.SetParent(boardItemsHolder);
            }
        }

        void setGenes(GameObject instance) {
            if (mutation()) modifyGen(ref instance.GetComponent<Cell>().range);
            if (instance.GetComponent<Cell>().range < 1) instance.GetComponent<Cell>().range = 1;
            if (mutation()) modifyGen(ref instance.GetComponent<Cell>().wallMultiplier);
            if (mutation()) modifyGen(ref instance.GetComponent<Cell>().wallDivider);
            if (mutation()) modifyGen(ref instance.GetComponent<Cell>().enemyOffset);
            if (mutation()) modifyGen(ref instance.GetComponent<Cell>().enemyMultiplier);
            if (mutation()) modifyGen(ref instance.GetComponent<Cell>().enemyDivider);
            if (mutation()) modifyGen(ref instance.GetComponent<Cell>().foodOffset);
            if (mutation()) modifyGen(ref instance.GetComponent<Cell>().foodMultiplier);
            if (mutation()) modifyGen(ref instance.GetComponent<Cell>().foodDivider);
            if (mutation()) modifyGen(ref instance.GetComponent<Cell>().cellMultiplier);
            if (mutation()) modifyGen(ref instance.GetComponent<Cell>().cellDivider);
        }

        void initGenes(GameObject instance) {
            instance.GetComponent<Cell>().range = Random.Range(2, 5);
            instance.GetComponent<Cell>().wallMultiplier = Random.Range(-2, 2);
            instance.GetComponent<Cell>().wallDivider = Random.Range(-10, 10);
            instance.GetComponent<Cell>().enemyOffset = Random.Range(-2, 2);
            instance.GetComponent<Cell>().enemyMultiplier = Random.Range(-2, 2);
            instance.GetComponent<Cell>().enemyDivider = Random.Range(-10, 10);
            instance.GetComponent<Cell>().foodOffset = Random.Range(-2, 2);
            instance.GetComponent<Cell>().foodMultiplier = Random.Range(-2, 2);
            instance.GetComponent<Cell>().foodDivider = Random.Range(-10, 10);
            instance.GetComponent<Cell>().cellMultiplier = Random.Range(-2, 2);
            instance.GetComponent<Cell>().cellDivider = Random.Range(-10, 10);
        }

        Boolean mutation() {
            int randomNumber = Random.Range(0, 100);
            if (randomNumber >= 50) return true;
            return false;
        }

        void modifyGen(ref int property) {
            int randomNumber = Random.Range(0, 100);
            if (randomNumber >= 50) property++;
            else property--;
        }

        void Scale() {
            boardHolder.transform.localScale = new Vector3(0.4F, 0.4F, 0);
            boardItemsHolder.transform.localScale = new Vector3(0.4F, 0.4F, 0);
        }
        
        
        //SetupScene initializes our level and calls the previous functions to lay out the game board
        public void SetupScene() {
            //Creates the grid list
            InitialiseList();

            //Creates the floor.
            BoardSetup ();

            //Instantiate a random number of enemies based on minimum and maximum, at randomized positions.
            LayoutObjectAtRandom (enemyTiles, 20, 20);

            LayoutObjectAtRandom (foodTiles, 20, 20);

            LayoutObjectAtRandom (cellTiles, 20, 20);

            //Scales down the map size
            Scale();
        }

        public void RespawnFood() {
            InitialiseList();

            LayoutObjectAtRandomNoCollision (foodTiles, 5, 5, 0.7453125f, 0.9f);
        }

        public void RespawnEnemies() {
            InitialiseList();

            if(GameObject.FindGameObjectsWithTag("Enemy").Length < 20) {
              LayoutObjectAtRandomNoCollision (enemyTiles, 1, 1, 0.7f, 0.7f);
            }
        }

        public void spawnCell(GameObject cell) {
            InitialiseList();

            LayoutCellAtRandomNoCollision (cell, 1, 1, 1f, 1f);
        }
    }
}