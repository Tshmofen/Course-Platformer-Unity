﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ThirdParty.QPathFinder.Script.Followers
{
    public class PathFollower : MonoBehaviour
    {
        public float moveSpeed = 10f;
        public bool alignToPath = true;
        public Transform _transform { get; set; }
        protected List<object> _pathToFollow;
        protected int _currentIndex;

        protected Vector3 CastToVec(object ob)
        {
            if (ob is Vector3) return (Vector3)ob; Debug.Assert(false, "Invalid cast"); return Vector3.zero;
        }

        protected Node CastToNode(object ob)
        {
            if (ob is Node) return (Node)ob; Debug.Assert(false, "Invalid cast"); return null;
        }

        protected virtual bool IsOnPoint(int pointIndex)
        {
            Debug.LogError("Override this"); return false; /* override this */
        }

        protected bool IsEndPoint(int pointIndex)
        {
            return pointIndex == EndIndex();
        }

        protected int EndIndex()
        {
            return _pathToFollow.Count - 1;
        }

        protected int GetNextIndex(int currentIndex)
        {
            int nextIndex = -1; if (currentIndex < EndIndex()) nextIndex = currentIndex + 1; return nextIndex;
        }

        protected int StartIndex()
        {
            return 0;
        }

        public void Follow(List<object> pointsToFollow, float moveSpeed, bool autoRotate)
        {
            _pathToFollow = pointsToFollow;
            this.moveSpeed = moveSpeed;
            alignToPath = autoRotate;

            StopFollowing();
            _currentIndex = 0;
            StartCoroutine(FollowPath());
        }

        // follow vertices
        public void Follow(List<Vector3> pointsToFollow, float moveSpeed, bool autoRotate)
        {
            Follow(pointsToFollow.Cast<object>().ToList(), moveSpeed, autoRotate);
        }

        // follow Nodes
        public void Follow(List<Node> pointsToFollow, float moveSpeed, bool autoRotate)
        {
            Follow(pointsToFollow.Cast<object>().ToList(), moveSpeed, autoRotate);
        }

        public void StopFollowing()
        {
            StopAllCoroutines();
        }

        private IEnumerator FollowPath()
        {
            yield return null;
            if (Logger.CanLogInfo) Logger.LogInfo(string.Format("[{0}] Follow(), Speed:{1}", name, moveSpeed));

            while (true)
            {
                _currentIndex = Mathf.Clamp(_currentIndex, 0, _pathToFollow.Count - 1);

                if (IsOnPoint(_currentIndex))
                {
                    if (IsEndPoint(_currentIndex)) break;
                    else _currentIndex = GetNextIndex(_currentIndex);
                }
                else
                {
                    MoveTo(_currentIndex);
                }
                yield return null;
            }

            if (Logger.CanLogInfo) Logger.LogInfo("PathFollower completed!");
        }

        public virtual void MoveTo(int pointIndex)
        {
            /// override this
            Debug.LogError("Override this");
        }
    }
}