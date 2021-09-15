using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/*
namespace DestructibleEnvironmentLevelEditing
{

}
*/

public class LevelEditor : MonoBehaviour
{
    [Header("Other classes required for this to work.")]
    public FirstPersonController movementController;
    public LevelEditorHUD headsUpDisplay;
    public EditorControllerStateHandler stateHandler;
    
    [Header("Block altering data")]
    public BlockInteraction interactionData;
    public int blockDamage = 1;
    public Explosion explosionStats;
    public LevelEditorHUD hud;
    BlockData blockToPlace;
    int blockIndex;

    // Save files
    DirectoryInfo chunkFolder;
    public FileInfo[] SaveFiles
    {
        get
        {
            return chunkFolder.GetFiles();
        }
    }
    public static string FilePath
    {
        get
        {
            return Application.dataPath + "/Resources/Level Chunks/";
        }
    }

    
    
    void Awake()
    {
        chunkFolder = new DirectoryInfo(FilePath);

        headsUpDisplay.controller = GetComponentInParent<LevelEditor>();
        blockToPlace = BlockData.AllBlocks[blockIndex];
        hud.currentBlock.text = blockToPlace.name;
    }
    

    // Update is called once per frame
    void Update()
    {
        if (Input.mouseScrollDelta.magnitude != 0)
        {
            float f = Input.mouseScrollDelta.y * (1 / Input.mouseScrollDelta.y);
            int indexChange = Mathf.RoundToInt(f);
            blockIndex += indexChange;
            blockIndex = MiscMath.InverseClamp(blockIndex, 0, BlockData.AllBlocks.Length - 1);
            blockToPlace = BlockData.AllBlocks[blockIndex];
            hud.currentBlock.text = blockToPlace.name;
        }
        
        if (interactionData.TryCheckBlock(transform.position, transform.forward))
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Destroy block
                interactionData.TargetedChunk.DamageBlock(interactionData.TargetedBlockCoords, blockDamage);
            }

            if (Input.GetMouseButtonDown(1))
            {
                
                // Place block
                Vector3Int placePosition = Vector3Int.RoundToInt(interactionData.ColliderDetected.normal) + interactionData.TargetedBlockCoords;
                interactionData.TargetedChunk.TryReplaceBlock(placePosition, blockToPlace);
                // This doesn't work if the face you're placing the block onto is on the edge of the chunk. I will need to add an adjacent chunk check to fix this.
            }

            if (Input.GetMouseButtonDown(2))
            {
                explosionStats.Detonate(interactionData.ColliderDetected.point);
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                // Open file window
                Debug.Log("Opening save window");
                headsUpDisplay.OpenSearchWindow();
            }
            /*
            if (Input.GetKeyDown(KeyCode.F))
            {
                // Save file
                Debug.Log("Saving");
                interactionData.TargetedChunk.SaveData();
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                // Load file
                //interactionData.TargetedChunk.LoadData()
                Debug.Log("Loading");
                interactionData.TargetedChunk.LoadData(interactionData.TargetedChunk.name);
            }
            */
        }
    }






    public void SaveLevel(string fileName)
    {
        interactionData.TargetedChunk.name = fileName;
        interactionData.TargetedChunk.SaveData();
    }
    public void OverwriteLevel(int fileIndex)
    {
        interactionData.TargetedChunk.name = SaveFiles[fileIndex].Name;
        interactionData.TargetedChunk.SaveData();
    }
    public void LoadLevel(int fileIndex)
    {
        interactionData.TargetedChunk.LoadData(SaveFiles[fileIndex].Name);
    }
    public void DeleteLevel(int fileIndex)
    {
        SaveFiles[fileIndex].Delete();
    }

}
