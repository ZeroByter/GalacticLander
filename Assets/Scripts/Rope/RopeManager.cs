using SourceConsole;
using System.Collections.Generic;
using UnityEngine;

namespace Rope
{
    public static class RopeManager
    {
        private static List<RopeController> Ropes = new List<RopeController>();
        private static int NextRopeId = 0;

        public static void ResetRopeId()
        {
            NextRopeId = 0;
        }

        public static RopeController GetRopeById(int id)
        {
            foreach(var rope in Ropes)
            {
                if (rope.ropeId == id) return rope;
            }

            return null;
        }

        public static void DeleteRopeById(int id)
        {
            int removeIndex = -1;

            for (int i = 0; i < Ropes.Count; i++)
            {
                RopeController rope = Ropes[i];
                if (rope.ropeId == id)
                {
                    removeIndex = i;
                    Object.Destroy(rope.gameObject);

                    break;
                }
            }

            if (removeIndex > -1) Ropes.RemoveAt(removeIndex);
        }

        private static RopePartController GetRopePrefab()
        {
            return Resources.Load<RopePartController>("Rope/Rope");
        }

        public static int CreateRope(Rigidbody2D start, Rigidbody2D end, bool endUseHinged = true)
        {
            var ropePrefab = GetRopePrefab();
            var collider = ropePrefab.GetComponent<BoxCollider2D>();

            float length = 0.6f;
            float distance = Vector2.Distance(start.position, end.position) - length * 2;

            int requiredRopeParts = Mathf.CeilToInt(distance / length) * 2;

            Vector2 direction = Vector2.ClampMagnitude((end.position - start.position).normalized, length);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            Vector2 startPos = start.position + direction / 2;

            RopePartController[] ropeParts = new RopePartController[requiredRopeParts];

            GameObject parent = new GameObject("Rope");
            var controller = parent.AddComponent<RopeController>();
            controller.ropeId = ++NextRopeId;

            Ropes.Add(controller);

            for (int i = 0; i < requiredRopeParts; i++)
            {
                ropeParts[i] = Object.Instantiate(ropePrefab, startPos + direction * i, Quaternion.Euler(direction), parent.transform);

                ropeParts[i].transform.rotation = Quaternion.Euler(0, 0, angle);

                bool isFirst = i == 0;
                bool isLast = i == requiredRopeParts - 1;

                if (isFirst) //first rope part
                {
                    if (endUseHinged)
                    {
                        ropeParts[i].AttachBack(start);
                    }
                    else
                    {
                        ropeParts[i].AttachBackFixed(start);
                    }
                }
                else
                {
                    var lastRopePart = ropeParts[i - 1];
                    ropeParts[i].AttachBack(lastRopePart.GetRigidbody());
                }
                if (isLast) //last rope part
                {
                    if (endUseHinged)
                    {
                        ropeParts[i].AttachFront(end);
                    }
                    else
                    {
                        ropeParts[i].AttachFrontFixed(end);
                    }
                }
            }

            return controller.ropeId;
        }

        /// <summary>
        /// Used just for testing...
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /*private static void CreateRope(Vector2 start, Vector2 end)
        {
            var ropePrefab = GetRopePrefab();
            var collider = ropePrefab.GetComponent<BoxCollider2D>();

            float length = 0.6f;
            float distance = Vector2.Distance(start, end) - length * 2;

            int requiredRopeParts = Mathf.CeilToInt(distance / length) * 2;

            Vector2 direction = Vector2.ClampMagnitude((end - start).normalized, length);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            start += direction / 2;

            RopePartController[] ropeParts = new RopePartController[requiredRopeParts];

            GameObject parent = new GameObject("Rope");
            var controller = parent.AddComponent<RopeController>();
            controller.ropeId = ++NextRopeId;

            Ropes.Add(controller);

            for (int i = 0; i < requiredRopeParts; i++)
            {
                ropeParts[i] = Object.Instantiate(ropePrefab, start + direction * i, Quaternion.Euler(direction), parent.transform);

                ropeParts[i].transform.rotation = Quaternion.Euler(0, 0, angle);

                bool isFirst = i == 0;
                bool isLast = i == requiredRopeParts - 1;

                if (isFirst) //first rope part
                {
                    ropeParts[i].AttachBack(start);
                }
                else
                {
                    var lastRopePart = ropeParts[i - 1];
                    ropeParts[i].AttachBack(lastRopePart.GetRigidbody());
                }
                if (isLast) //last rope part
                {
                    ropeParts[i].AttachFront(end);
                }
            }
        }

        //test_createrope 0 4 3 4
        [ConCommand]
        public static void Test_CreateRope(float startX, float startY, float endX, float endY)
        {
            CreateRope(new Vector2(startX, startY), new Vector2(endX, endY));
        }*/
    }
}