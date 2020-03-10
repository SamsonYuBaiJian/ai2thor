#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;


public class CreatePartNetPrefabFromAsset : MonoBehaviour
{

	[MenuItem("Extras/Create Prefabs From PartNet Objects")]
	static void CreatePartNetPrefab()
	{
        // source and target paths of all PartNet assets
        string sourcePath = "Assets/PartNet/";
        string targetPath = "Assets/Resources/PartNet/";

        // create target directory if target directory does not exist
        Directory.CreateDirectory(targetPath);

        // get directories of all PartNet assets
        string [] partNetAssetPaths = Directory.GetDirectories(sourcePath);

        foreach (string path in partNetAssetPaths){
            // set category
            string [] splitPath = path.Split('/');
            string category = splitPath[splitPath.Length-1];

            // create class directory if class directory does not exist
            Directory.CreateDirectory(targetPath + category);

            // load each asset file
            foreach (string filepath in Directory.GetFiles(path)){
                // ignore .meta files
                if (filepath.EndsWith("obj")){
                    // load asset file and assign loaded asset file to empty prefab
                    Transform transform = (Transform) AssetDatabase.LoadAssetAtPath(filepath, typeof(Transform));
                    Transform prefabInstance = (Transform) PrefabUtility.InstantiatePrefab(transform);

                    // scale prefabs down to fit into scene
                    prefabInstance.localScale = new Vector3 (0.5f, 0.5f, 0.5f);

                    // set main object's metadata to follow AI2-THOR
                    prefabInstance.gameObject.AddComponent<SimObjPhysics>();
                    prefabInstance.gameObject.layer = LayerMask.NameToLayer("SimObjVisible");
                    prefabInstance.gameObject.tag = "SimObjPhysics";
                    prefabInstance.gameObject.AddComponent<Rigidbody>();
                    Rigidbody rb = prefabInstance.GetComponent<Rigidbody>();
                    rb.isKinematic = false;
                    SimObjPhysics mainSop = prefabInstance.GetComponent<SimObjPhysics>();

                    // if object is chair, set appropriate metadata
                    if (category == "Chair"){
                        mainSop.Type = SimObjType.Chair;
                        mainSop.PrimaryProperty = SimObjPrimaryProperty.Moveable;
                        // set salientMaterials
                        mainSop.salientMaterials = new ObjectMetadata.ObjectSalientMaterial[5];
                        mainSop.salientMaterials[0] = ObjectMetadata.ObjectSalientMaterial.Metal;
                        mainSop.salientMaterials[1] = ObjectMetadata.ObjectSalientMaterial.Wood;
                        mainSop.salientMaterials[2] = ObjectMetadata.ObjectSalientMaterial.Plastic;
                        mainSop.salientMaterials[3] = ObjectMetadata.ObjectSalientMaterial.Ceramic;
                        mainSop.salientMaterials[4] = ObjectMetadata.ObjectSalientMaterial.Stone;

                        // set each component's metadata to follow AI2-THOR
                        foreach (Transform child in prefabInstance){
                            child.gameObject.AddComponent<SimObjPhysics>();
                            SimObjPhysics childSop = child.GetComponent<SimObjPhysics>();
                            if (child.name.Contains("chair_head")){
                                childSop.Type = SimObjType.ChairHead;
                            }
                            else if (child.name.Contains("chair_back")){
                                childSop.Type = SimObjType.ChairBack;
                            }
                            else if (child.name.Contains("chair_arm")){
                                childSop.Type = SimObjType.ChairArm;
                            }
                            else if (child.name.Contains("chair_base")){
                                childSop.Type = SimObjType.ChairBase;
                            }
                            else if (child.name.Contains("chair_seat")){
                                childSop.Type = SimObjType.ChairSeat;
                            }
                            else if (child.name.Contains("footrest")){
                                childSop.Type = SimObjType.ChairFootrest;
                            }

                            child.gameObject.layer = LayerMask.NameToLayer("SimObjVisible");
                            child.gameObject.tag = "SimObjPhysics";
                            child.gameObject.AddComponent<BoxCollider>();

                            // add VisibilityPoints to component
                            GameObject VisibilityPointsChild = new GameObject();
                            VisibilityPointsChild.name = "VisibilityPoints";
                            VisibilityPointsChild.transform.localScale = new Vector3 (0.5f, 0.5f, 0.5f);
                            VisibilityPointsChild.transform.parent = child;
                            GameObject vPointChild = new GameObject();
                            vPointChild.name = "vPoint";
                            vPointChild.transform.localScale = new Vector3 (0.5f, 0.5f, 0.5f);
                            vPointChild.layer = LayerMask.NameToLayer("SimObjVisible");
                            vPointChild.transform.parent = VisibilityPointsChild.transform;
                            childSop.VisibilityPoints = new Transform[1];
                            childSop.VisibilityPoints[0] = vPointChild.GetComponent<Transform>();
                        }
                    }
                    else if (category == "Laptop"){
                        mainSop.Type = SimObjType.Laptop;
                        mainSop.PrimaryProperty = SimObjPrimaryProperty.CanPickup;
                        // set salientMaterials
                        mainSop.salientMaterials = new ObjectMetadata.ObjectSalientMaterial[3];
                        mainSop.salientMaterials[0] = ObjectMetadata.ObjectSalientMaterial.Metal;
                        mainSop.salientMaterials[1] = ObjectMetadata.ObjectSalientMaterial.Plastic;
                        mainSop.salientMaterials[2] = ObjectMetadata.ObjectSalientMaterial.Glass;

                        // set each component's metadata to follow AI2-THOR
                        foreach (Transform child in prefabInstance){
                            child.gameObject.AddComponent<SimObjPhysics>();
                            SimObjPhysics childSop = child.GetComponent<SimObjPhysics>();
                            if (child.name.Contains("screen_side")){
                                childSop.Type = SimObjType.LaptopScreenSide;
                            }
                            else if (child.name.Contains("base_side")){
                                childSop.Type = SimObjType.LaptopBaseSide;
                            }

                            child.gameObject.layer = LayerMask.NameToLayer("SimObjVisible");
                            child.gameObject.tag = "SimObjPhysics";
                            child.gameObject.AddComponent<BoxCollider>();

                            // add VisibilityPoints to component
                            GameObject VisibilityPointsChild = new GameObject();
                            VisibilityPointsChild.name = "VisibilityPoints";
                            VisibilityPointsChild.transform.localScale = new Vector3 (0.5f, 0.5f, 0.5f);
                            VisibilityPointsChild.transform.parent = child;
                            GameObject vPointChild = new GameObject();
                            vPointChild.name = "vPoint";
                            vPointChild.transform.localScale = new Vector3 (0.5f, 0.5f, 0.5f);
                            vPointChild.layer = LayerMask.NameToLayer("SimObjVisible");
                            vPointChild.transform.parent = VisibilityPointsChild.transform;
                            childSop.VisibilityPoints = new Transform[1];
                            childSop.VisibilityPoints[0] = vPointChild.GetComponent<Transform>();
                        }
                    }
                    else if (category == "Display"){
                        mainSop.Type = SimObjType.Display;
                        mainSop.PrimaryProperty = SimObjPrimaryProperty.CanPickup;
                        // set salientMaterials
                        mainSop.salientMaterials = new ObjectMetadata.ObjectSalientMaterial[3];
                        mainSop.salientMaterials[0] = ObjectMetadata.ObjectSalientMaterial.Metal;
                        mainSop.salientMaterials[1] = ObjectMetadata.ObjectSalientMaterial.Plastic;
                        mainSop.salientMaterials[2] = ObjectMetadata.ObjectSalientMaterial.Glass;

                        // set each component's metadata to follow AI2-THOR
                        foreach (Transform child in prefabInstance){
                            child.gameObject.AddComponent<SimObjPhysics>();
                            SimObjPhysics childSop = child.GetComponent<SimObjPhysics>();
                            if (child.name.Contains("display_screen")){
                                childSop.Type = SimObjType.DisplayScreen;
                            }
                            else if (child.name.Contains("base")){
                                childSop.Type = SimObjType.DisplayBase;
                            }

                            child.gameObject.layer = LayerMask.NameToLayer("SimObjVisible");
                            child.gameObject.tag = "SimObjPhysics";
                            child.gameObject.AddComponent<BoxCollider>();

                            // add VisibilityPoints to component
                            GameObject VisibilityPointsChild = new GameObject();
                            VisibilityPointsChild.name = "VisibilityPoints";
                            VisibilityPointsChild.transform.localScale = new Vector3 (0.5f, 0.5f, 0.5f);
                            VisibilityPointsChild.transform.parent = child;
                            GameObject vPointChild = new GameObject();
                            vPointChild.name = "vPoint";
                            vPointChild.transform.localScale = new Vector3 (0.5f, 0.5f, 0.5f);
                            vPointChild.layer = LayerMask.NameToLayer("SimObjVisible");
                            vPointChild.transform.parent = VisibilityPointsChild.transform;
                            childSop.VisibilityPoints = new Transform[1];
                            childSop.VisibilityPoints[0] = vPointChild.GetComponent<Transform>();
                        }
                    }

                    // set main object's metadata to follow AI2-THOR
                    // add combined mesh to main object
                    Renderer[] rr = prefabInstance.GetComponentsInChildren<Renderer>();
                    Bounds b = rr[0].bounds;
                    foreach ( Renderer r in rr ) {
                        b.Encapsulate(r.bounds);
                    }
                    // add Colliders to main object
                    GameObject Colliders = new GameObject();
                    Colliders.name = "Colliders";
                    Colliders.transform.localScale = new Vector3 (0.5f, 0.5f, 0.5f);
                    Colliders.transform.parent = prefabInstance;
                    GameObject Col = new GameObject();
                    Col.name = "Col";
                    BoxCollider col = Col.AddComponent<BoxCollider>();
                    col.center = b.center;
                    col.size = b.size;
                    Col.layer = LayerMask.NameToLayer("SimObjVisible");
                    Col.tag = "SimObjPhysics";
                    Col.transform.parent = Colliders.transform;
                    // add VisibilityPoints to main object
                    GameObject VisibilityPoints = new GameObject();
                    VisibilityPoints.name = "VisibilityPoints";
                    VisibilityPoints.transform.localScale = new Vector3 (0.5f, 0.5f, 0.5f);
                    VisibilityPoints.transform.parent = prefabInstance;
                    GameObject vPoint = new GameObject();
                    vPoint.name = "vPoint";
                    vPoint.transform.localScale = new Vector3 (0.5f, 0.5f, 0.5f);
                    vPoint.layer = LayerMask.NameToLayer("SimObjVisible");
                    vPoint.transform.parent = VisibilityPoints.transform;
                    // add BoundingBox to main object
                    GameObject BoundingBox = new GameObject();
                    BoundingBox.name = "BoundingBox";
                    BoundingBox.transform.parent = prefabInstance;
                    BoundingBox.AddComponent<BoxCollider>();
                    BoundingBox.layer = LayerMask.NameToLayer("SimObjInvisible");
                    BoxCollider bBBoxCollider = BoundingBox.GetComponent<BoxCollider>();
                    bBBoxCollider.enabled = false;
                    bBBoxCollider.center = b.center;
                    bBBoxCollider.size = b.size;
                    // link main object's SimObjPhysics script to main object's components
                    mainSop.BoundingBox = BoundingBox;
                    mainSop.VisibilityPoints = new Transform[1];
                    mainSop.VisibilityPoints[0] = vPoint.GetComponent<Transform>();
                    mainSop.MyColliders = new Collider[1];
                    mainSop.MyColliders[0] = Col.GetComponent<BoxCollider>();

                    // save prefab
                    PrefabUtility.SaveAsPrefabAsset(prefabInstance.gameObject, targetPath + category + "/" + category + "_" + prefabInstance.gameObject.name + ".prefab");
                    
                    // destroy instantiated prefab
                    DestroyImmediate(prefabInstance.gameObject);
                }
            }
        }
	}
}
#endif