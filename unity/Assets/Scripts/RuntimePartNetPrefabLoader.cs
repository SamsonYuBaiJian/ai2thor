using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.Assertions;

// this script manages the spawning/placing of PartNet prefabs in the scene
public class RuntimePartNetPrefabLoader : MonoBehaviour
{
    // path of all PartNet prefabs
    static string unityPath = "/home/samson/Documents/github/allenai/ai2thor/unity";
    static string partNetPath = unityPath + "/Assets/Resources/PartNet/";

    // List of objects
    private List<string> partNetPrefabList = new List<string>();

    void Awake()
    {
        // Remove AI2-THOR objects that are of the 'Chair', 'Laptop' and 'Display' classes during initial spawn
        // Also remove AI2-THOR objects that look similar, like 'ArmChair', 'Ottoman', 'TVStand', 'Television' and 'Footstool'
        GameObject[] gos = (GameObject[])FindObjectsOfType(typeof(GameObject));
        foreach (GameObject o in gos)
        {
            if (o.name.Contains("Chair") | o.name.Contains("Footstool") | o.name.Contains("Laptop") | o.name.Contains("Display") | o.name.Contains("ArmChair") | o.name.Contains("Ottoman") | o.name.Contains("TVStand") | o.name.Contains("Television")){
                Destroy(o);
            }
        }

        // initialise important things
        PhysicsSceneManager physicsSceneManager = GameObject.Find("PhysicsSceneManager").GetComponent<PhysicsSceneManager>();
        GameObject topObject = GameObject.Find("Objects");

        // ********** set how many PartNet objects to spawn
        int objectNo = 5;

        System.Random randMat = new System.Random();
        System.Random randObj = new System.Random();
        System.Random randPos = new System.Random();

        // get all PartNet objects
        string[] partNetSubDirectories = Directory.GetDirectories(partNetPath);
        foreach(string sd in partNetSubDirectories){
            string[] partNetFiles = Directory.GetFiles(sd);
            foreach(string f in partNetFiles){
                if (!f.Contains(".meta")){
                    string final = f.Replace(unityPath + "/Assets/Resources/","");
                    final = final.Replace(".prefab","");
                    Debug.Log(final);
                    partNetPrefabList.Add(final);
                }
            }
        }

        List<int> usedObjectsIndex = new List<int>();

        // make sure number of PartNet objects more or equals to number of PartNet objects set to spawn
        Assert.IsTrue(partNetPrefabList.Count >= objectNo);

        // Randomise locations of PartNet prefab spawns at runtime
        for (int i = 0; i < objectNo; i++)
        {
            // get random PartNet object
            int randObjIndex = randObj.Next(0, partNetPrefabList.Count);
            while (usedObjectsIndex.Contains(randObjIndex)){
                randObjIndex = randObj.Next(0, partNetPrefabList.Count);
            }
            usedObjectsIndex.Add(randObjIndex);
            Debug.Log(partNetPrefabList[randObjIndex]);

            // instantiate PartNet prefab without collision
            GameObject go = Instantiate (Resources.Load (partNetPrefabList[randObjIndex])) as GameObject;

            // set prefab instance as child of "Objects"
            go.transform.SetParent(topObject.transform);

            // set prefab instance to a random location
            Vector3 [] possiblePosArray = getReachablePositionsforPartNetObjects(go);
            int randPosIndex = randPos.Next(0, possiblePosArray.Length);
            Vector3 possiblePos = possiblePosArray[randPosIndex];
            go.transform.position = possiblePos;

            // set random material for components
            foreach (Transform child in go.transform){
                MeshRenderer rend = child.gameObject.GetComponent<MeshRenderer>();
                SimObjPhysics childSop = child.gameObject.GetComponent<SimObjPhysics>();
                string[] materialList = new string [] {
                    unityPath + "/Assets/Resources/Materials/BLUE.mat",
                    unityPath + "/Assets/Resources/Materials/GREEN.mat",
                    unityPath + "/Assets/Resources/Materials/RED.mat"
                };
                string target;

                if (childSop != null){
                    if (childSop.Type == SimObjType.ChairHead){
                        target = unityPath + "/Assets/Resources/Materials/Chair/Head";
                        materialList = Directory.GetFiles(target);
                    }
                    else if (childSop.Type == SimObjType.ChairBack){
                        target = unityPath + "/Assets/Resources/Materials/Chair/Back";
                        materialList = Directory.GetFiles(target);
                    }
                    else if (childSop.Type == SimObjType.ChairArm){
                        target = unityPath + "/Assets/Resources/Materials/Chair/Arm";
                        materialList = Directory.GetFiles(target);
                    }
                    else if (childSop.Type == SimObjType.ChairBase){
                        target = unityPath + "/Assets/Resources/Materials/Chair/Base";
                        materialList = Directory.GetFiles(target);
                    }
                    else if (childSop.Type == SimObjType.ChairFootrest){
                        target = unityPath + "/Assets/Resources/Materials/Chair/Footrest";
                        materialList = Directory.GetFiles(target);
                    }
                    else if (childSop.Type == SimObjType.ChairSeat){
                        target = unityPath + "/Assets/Resources/Materials/Chair/Seat";
                        materialList = Directory.GetFiles(target);
                    }
                    else if (childSop.Type == SimObjType.LaptopScreenSide){
                        target = unityPath + "/Assets/Resources/Materials/Laptop/ScreenSide";
                        materialList = Directory.GetFiles(target);
                    }
                    else if (childSop.Type == SimObjType.LaptopBaseSide){
                        target = unityPath + "/Assets/Resources/Materials/Laptop/BaseSide";
                        materialList = Directory.GetFiles(target);
                    }
                    else if (childSop.Type == SimObjType.DisplayScreen){
                        target = unityPath + "/Assets/Resources/Materials/Display/Screen";
                        materialList = Directory.GetFiles(target);
                    }
                    else if (childSop.Type == SimObjType.DisplayBase){
                        target = unityPath + "/Assets/Resources/Materials/Display/Base";
                        materialList = Directory.GetFiles(target);
                    }

                    if (rend != null){
                        int randMatIndex = randMat.Next(0, materialList.Length);

                        string mat = materialList[randMatIndex];

                        mat = mat.Replace(unityPath + "/Assets/Resources/","");
                        mat = mat.Replace(".mat","");
                        mat = mat.Replace(".meta", "");

                        rend.material = Resources.Load(mat, typeof(Material)) as Material;
                    }

                    // make sure prefabs have unique IDs
                    physicsSceneManager.AddToObjectsInScene(childSop);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // reachable position functions
    protected Collider[] objectsCollidingWithTargetObject(GameObject go) {
        int layerMask = 1 << 8;

        Transform col = go.transform.Find("Colliders").Find("Col");
        BoxCollider bc = col.gameObject.GetComponent<BoxCollider>();

        Vector3 halfExtents = bc.size / 2.0f;
        Quaternion orientation = bc.transform.rotation;

        return Physics.OverlapBox(go.transform.position, halfExtents, orientation, layerMask, QueryTriggerInteraction.Ignore);
    }

    public RaycastHit[] boxCastAllForPartNetObjects(
            BoxCollider bc,
            float bufferWidth,
            Vector3 center,
            Vector3 dir,
            float moveMagnitude,
            int layerMask
            ) {
            Vector3 halfExtents = bc.size / 2.0f;
            Quaternion orientation = bc.transform.rotation;
            // float radius = bc.radius + bufferWidth;
            // float innerHeight = bc.height / 2.0f - radius;
            // Vector3 point1 = new Vector3(startPosition.x, center.y + innerHeight, startPosition.z);
            // Vector3 point2 = new Vector3(startPosition.x, center.y - innerHeight + skinWidth, startPosition.z);
            return Physics.BoxCastAll(
                center,
                halfExtents,
                dir,
                orientation, // orientation --> rotation of box
                moveMagnitude,
                layerMask,
                QueryTriggerInteraction.Ignore
            );
    }

    private bool ancestorHasName(GameObject go, string name) {
        if (go.name == name) {
            return true;
        } else if (go.transform.parent != null) {
            return ancestorHasName(go.transform.parent.gameObject, name);
        } else {
            return false;
        }
    }

    public Vector3[] getReachablePositionsforPartNetObjects(GameObject go, float gridMultiplier = 0.2f, float gridSize = 0.5f) {
        Bounds sceneBounds = new Bounds(Vector3.zero, Vector3.zero);
        Renderer[] renderers = (Renderer[]) FindObjectsOfType(typeof(Renderer));
        sceneBounds = renderers[0].bounds;
        foreach (Renderer r in renderers){
            sceneBounds.Encapsulate(r.bounds);
        }

        Transform col = go.transform.Find("Colliders").Find("Col");
        GameObject colObj = col.gameObject;
        
        BoxCollider bc = colObj.GetComponent<BoxCollider>();

        float bufferWidth = 0.01f;

        //float dirSkinWidthMultiplier = 1.0f + sw;
        Vector3[] directions = {
            new Vector3(1.0f, 0.0f, 0.0f),
            new Vector3(0.0f, 0.0f, 1.0f),
            new Vector3(-1.0f, 0.0f, 0.0f),
            new Vector3(0.0f, 0.0f, -1.0f)
        };

        Vector3 initialPos;

        int layerMask = 1 << 8;

        // make sure initial location is valid and not colliding with any other objects
        int directionCount = 0;
        HashSet<Collider> objectsAlreadyColliding = new HashSet<Collider>(objectsCollidingWithTargetObject(go));
        while (true){
            // randomise initial position
            directionCount = 0;
            initialPos = new Vector3(
                UnityEngine.Random.Range(sceneBounds.min.x + (bc.size.x/2.0f), sceneBounds.max.x - (bc.size.x/2.0f)),
                UnityEngine.Random.Range(0.01f + (bc.size.y/2.0f), sceneBounds.max.y - (bc.size.y/2.0f)),
                UnityEngine.Random.Range(sceneBounds.min.z + (bc.size.z/2.0f), sceneBounds.max.z - (bc.size.z/2.0f))
            );

            objectsAlreadyColliding = new HashSet<Collider>(objectsCollidingWithTargetObject(go));
            foreach (Vector3 d in directions) {
                if (directionCount > 0){
                    break;
                }

                go.transform.position = initialPos;

                RaycastHit[] hits = boxCastAllForPartNetObjects(
                    bc,
                    bufferWidth,
                    initialPos,
                    d,
                    0,
                    layerMask
                );

                // check if any point collides with any object that is not the floor
                foreach (RaycastHit hit in hits) {
                    if (hit.transform.gameObject.name != "Floor" &&
                        !ancestorHasName(hit.transform.gameObject, go.name) &&
                        !objectsAlreadyColliding.Contains(hit.collider)
                    ) {
                        directionCount++;
                        break;
                    }
                }
            }

            // if object does not collide with anything in all 4 directions
            if (directionCount == 0){
                break;
            }
        }
        
        Queue<Vector3> pointsQueue = new Queue<Vector3>();
        pointsQueue.Enqueue(initialPos);

        Vector3 p = new Vector3();

        HashSet<Vector3> goodPoints = new HashSet<Vector3>();
        int stepsTaken = 0;
        while (pointsQueue.Count != 0) {
            stepsTaken += 1;
            p = pointsQueue.Dequeue();
            if (!goodPoints.Contains(p)) {
                goodPoints.Add(p);
                objectsAlreadyColliding = new HashSet<Collider>(objectsCollidingWithTargetObject(go));
                foreach (Vector3 d in directions) {
                    RaycastHit[] hits = boxCastAllForPartNetObjects(
                        bc,
                        bufferWidth,
                        p,
                        d,
                        (gridSize * gridMultiplier),
                        layerMask
                    );

                    bool shouldEnqueue = true;
                    // check if any point collides with any object that is not the floor
                    foreach (RaycastHit hit in hits) {
                        if (hit.transform.gameObject.name != "Floor" &&
                            !ancestorHasName(hit.transform.gameObject, go.name) &&
                            !objectsAlreadyColliding.Contains(hit.collider)
                        ) {
                            shouldEnqueue = false;
                            break;
                        }
                    }
                    Vector3 newPosition = p + d * gridSize * gridMultiplier;
                    bool inBounds = sceneBounds.Contains(newPosition);

                    shouldEnqueue = shouldEnqueue && inBounds;
                    if (shouldEnqueue) {
                        pointsQueue.Enqueue(newPosition);
                    }
                }
            }
            // too many steps taken
            if (stepsTaken > 10000) {
                break;
            }
        }

        Vector3[] reachablePos = new Vector3[goodPoints.Count];
        goodPoints.CopyTo(reachablePos);

        return reachablePos;
    }
}
