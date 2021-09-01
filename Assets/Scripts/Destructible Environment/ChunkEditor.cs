using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Chunk))]
public class ChunkEditor : Editor
{
    //SerializedProperty facePixelDimensions;



    public bool editingEnabled = true;


    public BlockInteraction blockSelection;
    public Chunk chunkBeingEdited = null;
    public int blockToPlaceIndex = 0;
    public Explosion explosionData = new Explosion(true);

    // Dropdown for selecting what to do to a block
    bool showDropdown = false;
    Vector2 blockPopupPosition;
    Vector2 blockPopupDimensions = new Vector2(200, 400);


    // Resetting block
    bool enableResettingFunctions = false;
    Vector3Int newDimensions = Vector3Int.one;
    int chunkTemplateIndex = 0;
    bool confirmReset = false;

    private void OnEnable()
    {
        
        
        //facePixelDimensions = serializedObject.FindProperty("facePixelDimensions");
        Debug.Log(target.name + " is running OnEnable()");
        chunkBeingEdited = (Chunk)serializedObject.targetObject;
        //chunkBeingEdited.Awake();
        //chunkBeingEdited.Refresh(true);
        if (blockSelection == null)
        {
            blockSelection = new BlockInteraction();
        }
    }

    

    public void OnSceneGUI()
    {
        if (editingEnabled == false)
        {
            return;
        }
        
        // This code does not work because Input.mousePosition literally will not change for no discernible reason.
        Camera sceneViewCamera = SceneView.lastActiveSceneView.camera;
        Ray selectionRay = sceneViewCamera.ScreenPointToRay(Event.current.mousePosition);
        Debug.Log(selectionRay.origin + ", " + selectionRay.direction);
        if (Physics.Raycast(selectionRay, out RaycastHit rh, 999999999, ~0))
        {

        }
        Debug.Log(rh.point + ", " + rh.collider);
        if (blockSelection.TryCheckBlock(selectionRay.origin, selectionRay.direction))
        {

            // If showDropdown is false and left mouse is pressed, show dropdown
            if (Input.GetKeyDown(KeyCode.Mouse0) && !showDropdown)
            {
                showDropdown = true;
                blockPopupPosition = Input.mousePosition;
            }
            // If showDropdown is true and right mouse has been pressed, disable dropdown
            if (showDropdown && Input.GetKeyDown(KeyCode.Mouse1))
            {
                showDropdown = false;
            }
            /*
            if (showDropdown)
            {
                BlockData blockToPlace = BlockData.AllBlocks[blockToPlaceIndex];
                Rect optionRect = new Rect(blockPopupPosition, blockPopupDimensions);
                string[] blockOptions = new string[]
                {
                "Destroy this block",
                "Place " + blockToPlace.name + " block on this face",
                "Generate explosion with a width of " + explosionData.radius + " blocks"
                };
                int selectionIndex = EditorGUI.Popup(optionRect, "Make changes to chunk", 0, blockOptions);
                // Check if mouse button is pressed and inside popup rect
                if (Input.GetMouseButtonDown(0) && optionRect.Contains(Input.mousePosition))
                {

                }
            }
            */
        }
        else
        {
            //Debug.Log("Failed to find a chunk");
        }

        
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        serializedObject.Update();



        //EditorGUILayout.PropertyField(facePixelDimensions);
        //EditorGUILayout.HelpBox("Test message", MessageType.None);

        

        editingEnabled = EditorGUILayout.BeginFoldoutHeaderGroup(editingEnabled, "Enable Editing");
        if (editingEnabled)
        {
            #region Altering an existing chunk
            EditorGUILayout.HelpBox("Functions for editing individual chunks.\nClick on a face on a chunk to edit what happens to it.\nIf chunk is empty, reset it below.", MessageType.Info);

            blockSelection.raycastLength = EditorGUILayout.FloatField("Selection Raycast Length", blockSelection.raycastLength);

            List<string> specifiedLayers = new List<string>();
            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (layerName.Length > 0)
                {
                    specifiedLayers.Add(layerName);
                }
            }
            blockSelection.hitDetection = EditorGUILayout.MaskField("Selection Mask", blockSelection.hitDetection, specifiedLayers.ToArray());

            BlockData[] allBlocks = BlockData.AllBlocks;
            string[] blockNames = new string[allBlocks.Length];
            for (int i = 0; i < allBlocks.Length; i++)
            {
                blockNames[i] = allBlocks[i].name;
            }
            blockToPlaceIndex = EditorGUILayout.Popup("Selected Block", blockToPlaceIndex, blockNames);

            explosionData.radius = EditorGUILayout.FloatField("Explosion Radius", explosionData.radius);
            #endregion

            #region Resetting chunk
            EditorGUILayout.Space(20);
            EditorGUILayout.HelpBox("These functions are for resetting the chunk!\nDo not confirm resetting unless you are absolutely sure you want to lose your existing data!", MessageType.Warning);
            enableResettingFunctions = EditorGUILayout.BeginToggleGroup("Enable reset settings", enableResettingFunctions);
            newDimensions = EditorGUILayout.Vector3IntField("New dimensions", newDimensions);
            string[] resetChunkOptions = new string[]
            {
            "Flood",
            "Hollow",
            "Floor Only",
            //"Chequerboard",
            //"Spaced out",
            "Empty",
            };
            chunkTemplateIndex = EditorGUILayout.Popup("Chunk template", chunkTemplateIndex, resetChunkOptions);
            confirmReset = EditorGUILayout.Toggle("CONFIRM", confirmReset);
            if (confirmReset == true && enableResettingFunctions == true)
            {
                Block[,,] newChunkData;
                switch (chunkTemplateIndex)
                {
                    case 0:
                        newChunkData = FillChunk.Flood(newDimensions, BlockData.AllBlocks[blockToPlaceIndex]);
                        break;
                    case 1:
                        newChunkData = FillChunk.Hollow(newDimensions, BlockData.AllBlocks[blockToPlaceIndex]);
                        break;
                    case 2:
                        newChunkData = FillChunk.OnlyFloor(newDimensions, BlockData.AllBlocks[blockToPlaceIndex]);
                        break;
                    case 3:
                        newChunkData = FillChunk.Empty(newDimensions);
                        break;
                    default:
                        newChunkData = FillChunk.Flood(newDimensions, BlockData.AllBlocks[blockToPlaceIndex]);
                        break;
                }
                chunkBeingEdited.Awake();
                chunkBeingEdited.Rewrite(newChunkData);
                confirmReset = false;
                enableResettingFunctions = false;
            }
            EditorGUILayout.EndToggleGroup();
            #endregion
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        serializedObject.ApplyModifiedProperties();
    }
}
