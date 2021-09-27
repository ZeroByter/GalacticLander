using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rope
{
    public class RopePartController : MonoBehaviour
    {
        private new Rigidbody2D rigidbody;
        private new BoxCollider2D collider;

        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody2D>();
            collider = GetComponent<BoxCollider2D>();
        }

        public BoxCollider2D GetCollider()
        {
            return collider;
        }

        public Rigidbody2D GetRigidbody()
        {
            return rigidbody;
        }

        public void AttachBack(Vector2 attachTo)
        {
            var hinge = gameObject.AddComponent<HingeJoint2D>();
            hinge.anchor = new Vector3(-0.3f, 0);
            hinge.connectedAnchor = attachTo;
        }

        public void AttachBack(Rigidbody2D attachTo)
        {
            var hinge = gameObject.AddComponent<HingeJoint2D>();
            hinge.anchor = new Vector3(-0.3f, 0);
            hinge.connectedBody = attachTo;
        }

        public void AttachBackFixed(Rigidbody2D attachTo)
        {
            var hinge = gameObject.AddComponent<FixedJoint2D>();
            hinge.anchor = new Vector3(-0.3f, 0);
            hinge.connectedBody = attachTo;
        }

        public void AttachFront(Vector2 attachTo)
        {
            var hinge = gameObject.AddComponent<HingeJoint2D>();
            hinge.anchor = new Vector3(0.3f, 0);
            hinge.connectedAnchor = attachTo;
        }

        public void AttachFront(Rigidbody2D attachTo)
        {
            var hinge = gameObject.AddComponent<HingeJoint2D>();
            hinge.anchor = new Vector3(0.3f, 0);
            hinge.connectedBody = attachTo;
        }

        public void AttachFrontFixed(Rigidbody2D attachTo)
        {
            var hinge = gameObject.AddComponent<FixedJoint2D>();
            hinge.anchor = new Vector3(0.3f, 0);
            hinge.connectedBody = attachTo;
        }
    }
}