using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using Microsoft.MixedReality.Toolkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Microsoft.MixedReality.Toolkit.Experimental.SpatialAwareness;
using Microsoft.MixedReality.Toolkit.SpatialAwareness.Processing;

namespace Minerva
{
    public class RoomMeshScanManager : MonoBehaviour
    {

        public static RoomMeshScanManager Instance { get; private set; }

        public Material _TestMaterial;

        public byte[] RoomMeshData { get { return roomMeshData; } }

        [SerializeField] private Material meshVisualizingMaterial;

        private IMixedRealitySpatialAwarenessMeshObserver spatialMeshObserver;

        private List<MeshFilter> spatialMeshFilters;

        public List<MeshFilter> roomMeshFilters;

        public Mesh combinedMesh;

        private byte[] roomMeshData = new byte[1];

        private List<SpatialAwarenessMeshObject> meshObjects2;

        private Dictionary<int, Mesh> savedMeshes;
        private Dictionary<int, Matrix4x4> savedMatrix;

        private bool running = false;

        private float elapsed = 0f;

        private List<PlaneFinding.MeshData> meshData;


        //[SerializeField] private GameObject roomMeshObject;


        void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
                Instance = this;
        }

        void Start()
        {
            
        }

        void Update()
        {
            if (!running) return;
            if ((elapsed += Time.deltaTime) < 5f) return;
            elapsed = 0f;

            AddNewMeshes();

        }
        
        void AddNewMeshes()
        {
            Debug.Log("saved vs current: " + savedMeshes.Count + " - " + spatialMeshObserver.Meshes.Values.Count());
            List<SpatialAwarenessMeshObject> tmp = new List<SpatialAwarenessMeshObject>();
            List<SpatialAwarenessMeshObject> current = spatialMeshObserver.Meshes.Values.ToList();
            SpatialAwarenessMeshObject obj = null;
            for (int i = 0; i < current.Count; i++)
            {
                Mesh m = new Mesh();
                m.vertices = current[i].Filter.mesh.vertices;
                m.triangles = current[i].Filter.mesh.triangles;
                m.normals = current[i].Filter.mesh.normals;
                m.uv = current[i].Filter.mesh.uv;

                savedMeshes[current[i].Id] = m;
                savedMatrix[current[i].Id] = current[i].Filter.transform.localToWorldMatrix;

                //roomMeshFilters.Add(current[i].Filter);
                /*
                // Currently saved mesh objects do not contain new mesh
                if ((obj = meshObjects.Find(n => n.Id == current[i].Id)) == null)
                {
                    tmp.Add(current[i]);
                }
                else
                {
                    // Currently saved mehs objects contain id, but differ
                    if (!obj.Equals(current[i]))
                        tmp.Add(current[i]);
                }
                */
            }
            List<PlaneFinding.MeshData> meshData = new List<PlaneFinding.MeshData>();

            var spatialAwarenessSystem = CoreServices.SpatialAwarenessSystem;
            if (spatialAwarenessSystem != null)
            {
                GameObject parentObject = spatialAwarenessSystem.SpatialAwarenessObjectParent;

                // Get mesh filters from SpatialAwareness Mesh Observer
                foreach (MeshFilter filter in parentObject.GetComponentsInChildren<MeshFilter>())
                {
                    
                    roomMeshFilters.Add(filter);
                }
            }


            for (int index = 0; index < roomMeshFilters.Count; index++)
            {
                MeshFilter filter = roomMeshFilters[index];
                if (filter != null && filter.sharedMesh != null)
                {
                    // fix surface mesh normals so we can get correct plane orientation.
                    filter.mesh.RecalculateNormals();
                    meshData.Add(new PlaneFinding.MeshData(filter));
                    //
                    /*
                    GameObject _meshScanObject = new GameObject("_meshScanObject");
                    _meshScanObject.transform.parent = roomMeshObject.transform;
                    _meshScanObject.AddComponent<MeshFilter>();
                    _meshScanObject.GetComponent<MeshFilter>().mesh = filter.mesh;
                    */
                }
            }


            /*
            // add the remaining meshes
            for (int i = 0; i < meshObjects.Count; i++)
            {
                if (tmp.Find(n => n.Id == meshObjects[i].Id) == null)
                    tmp.Add(meshObjects[i]);
            }
            meshObjects = tmp;
            */
        }

        public virtual void StartMeshScan()
        {
            spatialMeshObserver = CoreServices.GetSpatialAwarenessSystemDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();

            spatialMeshFilters = new List<MeshFilter>();
            roomMeshFilters = new List<MeshFilter>();

            // Will not delete the captured meshes from the device, this can only be done by the user in the settings of the HoloLens
            spatialMeshObserver.ClearObservations();

            spatialMeshObserver.DisplayOption = SpatialAwarenessMeshDisplayOptions.Visible;
            spatialMeshObserver.VisibleMaterial = meshVisualizingMaterial;

            //meshObjects = new List<SpatialAwarenessMeshObject>();
            savedMeshes = new Dictionary<int, Mesh>();
            savedMatrix = new Dictionary<int, Matrix4x4>();

            running = true;
        }

        public virtual void OnFinishMeshScan()
        {

            running = false;

            AddNewMeshes();

            spatialMeshObserver.DisplayOption = SpatialAwarenessMeshDisplayOptions.None;
            spatialMeshObserver.VisibleMaterial = null;
            spatialMeshFilters.Clear();

            //foreach (SpatialAwarenessMeshObject meshObject in spatialMeshObserver.Meshes.Values)
            //spatialMeshFilters.Add(meshObject.Filter);

            combinedMesh = CombineCapturedMeshes2();

            PaintManager.Instance.roomMesh = combinedMesh;
            //PaintManager.Instance.spatialMeshFilters = roomMeshFilters;
            PaintManager.Instance.meshData = meshData;

            byte[] serialized = SimpleMeshSerializer.Serialize(new List<Mesh>() { combinedMesh });

            roomMeshData = serialized;

            /*
            GameObject obj = new GameObject();
            MeshRenderer r = obj.AddComponent<MeshRenderer>();
            MeshFilter f = obj.AddComponent<MeshFilter>();
            f.mesh = combinedMesh;
            */



            //Client.Instance.Send(MessageBuilder.CreateMessage(serialized, MessageType.RoomMesh));

            PaintTestStart.Instance.OnMeshScanComplete();
        }

        private Mesh CombineCapturedMeshes2()
        {
            Mesh singleMesh = new Mesh();

            CombineInstance[] combined = new CombineInstance[savedMeshes.Count];
            int i = 0;
            foreach(var key in savedMeshes.Keys)
            {
                combined[i].mesh = savedMeshes[key];
                combined[i].transform = savedMatrix[key];
                i++;
            }
            singleMesh.CombineMeshes(combined, true);
            return singleMesh;
        }

        private Mesh CombineCapturedMeshes()
        {
            Mesh singleMesh = new Mesh();

            CombineInstance[] combined = new CombineInstance[spatialMeshFilters.Count];
            for (var i = 0; i < spatialMeshFilters.Count; i++)
            {
                if (spatialMeshFilters[i] == null || spatialMeshFilters[i].sharedMesh == null) continue;

                combined[i].mesh = spatialMeshFilters[i].sharedMesh;
                combined[i].transform = spatialMeshFilters[i].transform.localToWorldMatrix;
            }

            singleMesh.CombineMeshes(combined, true);
            return singleMesh;
        }

        public Mesh GetSingleMesh()
        {
            return combinedMesh;
        }

    }
}
