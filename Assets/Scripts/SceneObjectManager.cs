using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minerva
{
    public class SceneObjectManager : MonoBehaviour
    {

        public static SceneObjectManager Instance { get; private set; }

        [SerializeField] private GameObject joinCodeKeypad;
        [SerializeField] private GameObject meshScanUI;
        [SerializeField] private GameObject originUI;
        [SerializeField] private GameObject singleSessionConnect;
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

        }

        public virtual void ShowJoinCodeKeypad() => joinCodeKeypad.SetActive(true);

        public virtual void HideJoinCodeKeypad() => joinCodeKeypad.SetActive(false);

        public virtual void ShowOrganizationSetup()
        {
            throw new System.NotImplementedException();
        }

        public virtual void HideOrganizationSetup()
        {
            throw new System.NotImplementedException();
        }

        public virtual void ShowMeshScanUI() => meshScanUI.SetActive(true);

        public virtual void HideMeshScanUI() => meshScanUI.SetActive(false);

        public virtual void ShowOriginUI() => originUI.SetActive(true);

        public virtual void HideOriginUI() => originUI.SetActive(false);

        public virtual void ShowSingleConnectUI() => singleSessionConnect.SetActive(true);

        public virtual void HideSingleConnectUI() => singleSessionConnect.SetActive(false);

    }
}
