using UnityEngine;
using System.Collections;
using Completed;

    using System.Collections.Generic;       //Allows us to use Lists. 
    
    public class GameManager : MonoBehaviour
    {

        public static GameManager instance = null;              //Static instance of GameManager which allows it to be accessed by any other script.
        
        private BoardManager boardScript;                       //Store a reference to our BoardManager which will set up the level.
        private List<Cell> cells;                          //List of all Enemy units, used to issue them move commands.
        private bool cellsMoving;                             //Boolean to check if enemies are moving.
        
        
        
        //Awake is always called before any Start functions
        void Awake()
        {
            //Check if instance already exists
            if (instance == null)
                
                //if not, set instance to this
                instance = this;
            
            //If instance already exists and it's not this:
            else if (instance != this)
                
                //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
                Destroy(gameObject);    
            
            //Assign enemies to a new List of Enemy objects.
            cells = new List<Cell>();
            
            //Get a component reference to the attached BoardManager script
            boardScript = GetComponent<BoardManager>();
            
            //Call the InitGame function to initialize the first level 
            InitGame();
        }
        
        //Initializes the game for each level.
        void InitGame()
        {
            
            //Clear any Enemy objects in our List to prepare for next level.
            cells.Clear();
            
            //Call the SetupScene function of the BoardManager script, pass it current level number.
            boardScript.SetupScene();
            
        }
        
        
        //Update is called every frame.
        void Update()
        {
            //Check that playersTurn or enemiesMoving or doingSetup are not currently true.
            if(cellsMoving)
                
                //If any of these are true, return and do not start MoveEnemies.
                return;

            cells.RemoveAll(item => item == null);

            boardScript.RespawnFood();
            boardScript.RespawnEnemies();

            //Start moving enemies.
            StartCoroutine (MoveCells ());
        }
        
        //Call this to add the passed in Enemy to the List of Enemy objects.
        public void AddCellToList(Cell script)
        {
            //Add Enemy to List enemies.
            cells.Add(script);
        }
            
        //Coroutine to move enemies in sequence.
        IEnumerator MoveCells()
        {
            //While enemiesMoving is true player is unable to move.
            cellsMoving = true;
            
            //Loop through List of Enemy objects.
            for (int i = 0; i < cells.Count; i++)
            {
                //Call the MoveEnemy function of Enemy at index i in the enemies List.
                cells[i].MoveCell();
                if (cells[i].foodCount == 5) {
                  cells[i].foodCount = 0;
                  boardScript.spawnCell(cells[i].transform.gameObject);
                }
                
                //Wait for Enemy's moveTime before moving next Enemy, 
                yield return new WaitForSeconds(cells[i].moveTime);
            }
            
            //Enemies are done moving, set enemiesMoving to false.
            cellsMoving = false;
        }
    }