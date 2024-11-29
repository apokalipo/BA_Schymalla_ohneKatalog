using Minerva;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Minerva
{
    public class PaintTestStart : MonoBehaviour
    {
        public GameObject meshScanUi;
        public GameObject originUi;
        public static PaintTestStart Instance { get; private set; }

        public UnityEvent ApplicationStartEvent;


        void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
                Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {

            //SceneObjectManager.Instance.ShowMeshScanUI();
            meshScanUi.SetActive(true);
            RoomMeshScanManager.Instance.StartMeshScan();

            OnApplicationStart();
        }

        // Update is called once per frame
        void Update()
        {
            Debug.developerConsoleVisible = false;
        }

        public virtual void OnApplicationStart()
        {


            ApplicationStartEvent?.Invoke();
        }



        public virtual void OnMeshScanComplete()
        {
            // Only thing left to do is set the origin
            //SceneObjectManager.Instance.HideMeshScanUI();
            meshScanUi.SetActive(false);
            originUi.SetActive(true);
            //SceneObjectManager.Instance.ShowOriginUI();

            OriginPointManager.Instance.StartSetOrigin();
        }

        public virtual void OnOriginSet()
        {
            // All done now
            //SceneObjectManager.Instance.HideOriginUI();
            originUi.SetActive(false);
            PaintManager.Instance.toggleColorMenu();
        }
    }
}