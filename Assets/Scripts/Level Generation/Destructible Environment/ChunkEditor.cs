using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Chunk))]
public class ChunkEditor : Editor
{
    //SerializedProperty facePixelDimensions;


    SerializedProperty blockGrid;


    string saveString;



    public bool editingEnabled = false;


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
        blockGrid = serializedObject.FindProperty("blocks");

        Debug.Log(target.name + " is running OnEnable()");
        chunkBeingEdited = (Chunk)serializedObject.targetObject;

        chunkBeingEdited.Awake();
        chunkBeingEdited.Refresh(true);

        

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

        Camera sceneViewCamera = SceneView.lastActiveSceneView.camera;
        Ray selectionRay = sceneViewCamera.ScreenPointToRay(Event.current.mousePosition);
        //Ray selectionRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);


        float raycastLength = Vector3.Distance(selectionRay.origin, chunkBeingEdited.transform.position) + Vector3.Distance(chunkBeingEdited.terrainMesh.bounds.min, chunkBeingEdited.terrainMesh.bounds.max);
        blockSelection.raycastLength = raycastLength;
        blockSelection.hitDetection = ~0;
        if (blockSelection.TryCheckBlock(selectionRay.origin, selectionRay.direction))
        {
            /*
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
            */
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

        Debug.Log(selectionRay.origin + ", " + selectionRay.direction + ", " + blockSelection.ColliderDetected.point + ", " + blockSelection.ColliderDetected.collider + ", " + raycastLength);
        Debug.DrawLine(sceneViewCamera.transform.position, selectionRay.origin + selectionRay.direction * raycastLength, Color.red, 10);

    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        serializedObject.Update();



        //EditorGUILayout.PropertyField(facePixelDimensions);
        //EditorGUILayout.HelpBox("Test message", MessageType.None);



        /*
        editingEnabled = EditorGUILayout.BeginToggleGroup("Enable editing", editingEnabled);
        saveString = EditorGUILayout.TextField("Save string", saveString);



        EditorGUILayout.EndToggleGroup();

        bool generateString = EditorGUILayout.Toggle(false, "Generate save string");
        if (generateString)
        {

        }

        string s = EditorGUILayout.LabelField("Save data", )
        */
        
        editingEnabled = EditorGUILayout.BeginFoldoutHeaderGroup(editingEnabled, "Enable Editing");
        if (editingEnabled)
        {
            #region Altering an existing chunk
            EditorGUILayout.HelpBox("Functions for editing individual chunks.\nClick on a face on a chunk to edit what happens to it.\nIf chunk is empty, reset it below.", MessageType.Info);

            
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
                        newChunkData = ChunkGenerator.Flood(newDimensions, BlockData.AllBlocks[blockToPlaceIndex]);
                        break;
                    case 1:
                        newChunkData = ChunkGenerator.Hollow(newDimensions, BlockData.AllBlocks[blockToPlaceIndex]);
                        break;
                    case 2:
                        newChunkData = ChunkGenerator.OnlyFloor(newDimensions, BlockData.AllBlocks[blockToPlaceIndex]);
                        break;
                    case 3:
                        newChunkData = ChunkGenerator.Empty(newDimensions);
                        break;
                    default:
                        newChunkData = ChunkGenerator.Flood(newDimensions, BlockData.AllBlocks[blockToPlaceIndex]);
                        break;
                }
                chunkBeingEdited.Awake();
                chunkBeingEdited.Rewrite(newChunkData);
                blockGrid = serializedObject.FindProperty("blocks");
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
