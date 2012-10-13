﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace Emergence.AI {
    public class AIAgent : Agent {
        public List<MeshNode> path = new List<MeshNode>();
        float timeout = 2;          //seconds before node timeout
        double targetAquisitionDuration = 0;
        List<MeshNode> ignore = new List<MeshNode>();

        public AIAgent(CoreEngine c, Vector3 position, Vector2 direction)
            : base(c, position, direction) { }
        public AIAgent(CoreEngine c, Vector3 position)
            : base(c, position, new Vector2(1, 0)) { }

        protected MeshNode findClosestMeshNode(List<MeshNode> ignore) {
            if (ignore == null)
                ignore = new List<MeshNode>();
            MeshNode closest = null;
            float dist = 0;
            foreach (MeshNode m in core.aiEngine.mesh) {
                if (ignore.Contains(m))
                    continue;
                if (Vector3.Distance(m.position, position) < dist || closest == null) {
                    closest = m;
                    dist = Vector3.Distance(m.position, position);
                }
            }
            return closest;
        }

        //perform A* from the closest node to the target node
        public void setPathTo(MeshNode target, List<MeshNode> ignore) {
            path.Clear();
            MeshNode start = findClosestMeshNode(ignore);
            if (start == null)  return;
            
            AStarHeap q = new AStarHeap();
            //{node: (path length, previous node)}
            Dictionary<MeshNode, KeyValuePair<float, MeshNode>> distances = new Dictionary<MeshNode, KeyValuePair<float, MeshNode>>();
            q.add(new AStarHeapNode(start, 0, Vector3.Distance(start.position, target.position)));
            distances.Add(start, new KeyValuePair<float, MeshNode>(0, null));
            while (!q.empty()) {
                AStarHeapNode n = q.pop();
                if (n.node == target)
                    break;
                
                //try and expand from this node to shortest paths
                foreach (MeshNode m in n.node.neighbours) {
                    if (ignore.Contains(m))
                        continue;
                    float pathLength = n.pathLength + Vector3.Distance(m.position, n.node.position);
                    //see if we don't already have a shorter path to this node
                    if (distances.ContainsKey(m) && distances[m].Key <= pathLength)
                        continue;
                    //otherwise this is promising path, so add it to the queue
                    q.add(new AStarHeapNode(m, pathLength, pathLength + Vector3.Distance(m.position, target.position)));
                    distances.Remove(m);
                    distances.Add(m, new KeyValuePair<float, MeshNode>(pathLength, n.node));
                }
            }

            //if we've found a path
            if (distances.ContainsKey(target)) {
                //backtrack the entire path and add it to the path list
                MeshNode m = target;
                while (m != null) {
                    path.Insert(0, m);
                    m = distances[m].Value;
                }
            }
        }

        public void findRoamingPath() {

        }

        public override void Update(GameTime gameTime) {
            if (path.Count == 0) {
                ignore.Clear();     //should we clear ignore here?
                targetAquisitionDuration = 0;
                setPathTo(core.aiEngine.mesh[core.aiEngine.random.Next(core.aiEngine.mesh.Count)], ignore);
                if (path.Count == 0)
                    return;
            }

            MeshNode target = path[0];
            //if we're sufficiently close to the target switch it
            //find the target's position relative to the position
            Vector2 tpos = new Vector2(Math.Abs(target.position.X - position.X),
                                        Math.Abs(target.position.Z - position.Z));
            if (tpos.X < size.X / 3 && tpos.Y < size.Y / 3) {
                ignore.Clear();
                path.RemoveAt(0);
                targetAquisitionDuration = 0;
                if (path.Count == 0)
                    setPathTo(core.aiEngine.mesh[core.aiEngine.random.Next(core.aiEngine.mesh.Count)], ignore);
                if (path.Count == 0)
                    return;
                target = path[0];
            }

            //calculate direction to target -- we need this for the model, this will probably change, but the principles are right
            Vector3 velocity = target.position - position;
            direction = getDirectionFromVector(velocity);

            //now calculate the move and actually move
            velocity.Y = 0;
            velocity.Normalize();

            Vector3 jumpVelocity = Vector3.Zero;
            if (core.aiEngine.random.Next(0, 100) < 1)
                jumpVelocity.Y = jump;

            position = core.physicsEngine.applyMovement(gameTime, this, speed * velocity + jumpVelocity);

            targetAquisitionDuration += gameTime.ElapsedGameTime.TotalSeconds;
            if (targetAquisitionDuration >= timeout) {
                ignore.Add(target);
                targetAquisitionDuration = 0;
                if(path.Count > 0)
                    setPathTo(path[path.Count - 1], ignore);
            }
        }
    }

    //a minheap where parent <= children
    class AStarHeap {
        List<AStarHeapNode> heap = new List<AStarHeapNode>();
        public void add(AStarHeapNode a) {
            heap.Add(a);
            int i = heap.Count - 1;
            while (i > 0) {
                int parentI = (i - 1) / 2;
                if (heap[parentI].cost > heap[i].cost)
                    swap(parentI, i);
                else
                    break;
                i = parentI;
            }
        }

        public AStarHeapNode pop() {
            if (heap.Count == 0)
                return null;
            AStarHeapNode root = heap[0];
            heap.RemoveAt(0);
            if (heap.Count <= 1)
                return root;
            heap.Insert(0, heap[heap.Count-1]);
            heap.RemoveAt(heap.Count - 1);
            int i = 0;
            while (i < heap.Count) {
                int minChild = -1, left = i*2+1, right = i*2+2;
                if (left < heap.Count)
                    minChild = left;
                if (right < heap.Count && (minChild == -1 || heap[right].cost < heap[minChild].cost))
                    minChild = right;
                if (minChild == -1)
                    break;
                swap(i, minChild);
                i = minChild;
            }
            return root;
        }

        public void swap(int i, int j) {
            AStarHeapNode temp = heap[i];
            heap[i] = heap[j];
            heap[j] = temp;
        }

        public bool empty() {
            return heap.Count == 0;
        }
    }

    class AStarHeapNode {
        public float pathLength, cost;
        public MeshNode node;
        public AStarHeapNode(MeshNode m, float distSoFar, float hDist)  {
            node = m;
            pathLength = distSoFar;
            cost = hDist;
        }
    }
}