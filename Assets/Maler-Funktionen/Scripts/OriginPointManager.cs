using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minerva
{
    public class OriginPointManager : MonoBehaviour
    {

        public static OriginPointManager Instance { get; private set; }

        [SerializeField] private Transform origin;
        [SerializeField] private Transform referenceObject;

        private bool movingOrigin = false;

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
            if (!movingOrigin) return;

            origin.SetPositionAndRotation(new Vector3(referenceObject.position.x, 0f, referenceObject.position.z), new Quaternion(0f, referenceObject.rotation.y, 0f, referenceObject.rotation.w));
        }

        public virtual void StartSetOrigin()
        {
            movingOrigin = true;
        }

        public virtual void FinishSetOrigin()
        {
            movingOrigin = false;
            PaintTestStart.Instance.OnOriginSet();
        }

    }
}
