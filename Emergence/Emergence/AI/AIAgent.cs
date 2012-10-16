using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Emergence.Weapons;
//git is awesome
namespace Emergence.AI {
    public class AIAgent : Agent {
        public List<MeshNode> path = new List<MeshNode>();
        float timeout = 2;          //seconds before node timeout
        double targetAquisitionDuration = 0;
        List<MeshNode> ignore = new List<MeshNode>();

        public static double deadReckoningTimeout = 0.5;       //number of seconds to dead reckon for
        double deadReckoningTimeStamp = -deadReckoningTimeout-1;
        Vector3 deadReckoningMove = Vector3.Zero;

        MeshNode previousTarget = null;

        float checkTime = 4f, curCheckTime = 0;

        public AIAgent(CoreEngine c, Vector3 position, Vector2 direction)
            : base(c, position, direction) { }
        public AIAgent(CoreEngine c, Vector3 position)
            : base(c, position, new Vector2(1, 0)) { }

        protected MeshNode findClosestMeshNode(List<MeshNode> ignore) {
            if (ignore == null)
                ignore = new List<MeshNode>();
            MeshNode closest = null;
            float dist = 0;
            Vector3 nodeLift = new Vector3(0, AIEngine.nodeHeight, 0);
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
            previousTarget = null;
            targetAquisitionDuration = 0;
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

        protected void popFromPath() {
            previousTarget = null;
            if(path.Count > 0)  {
                previousTarget = path[0];
                path.RemoveAt(0);
            }
        }

        public void tryJump() {
            if (agentVelocities.persistentVelocity.Y == 0)
                agentVelocities.persistentVelocity.Y = jump;
        }

        public void findRoamingPath() {

        }

        public override void Update(GameTime gameTime) {
            if (spawnTime > 0) {
                spawnTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (spawnTime <= 0) {
                    spawnTime = 0;
                    health = 100;
                    ammo = 200;
                    equipped = new Pistol();
                    core.spawnPlayer(this);
                }
                return;
            }
            else if (health < 0) {
                spawnTime = spawnDelay;
                return;
            }
            equipped.Update(gameTime);
            if (path.Count == 0) {
                ignore.Clear();     //should we clear ignore here?
                setPathTo(core.aiEngine.mesh[core.aiEngine.random.Next(core.aiEngine.mesh.Count)], ignore);
                if (path.Count == 0)
                    return;
            }

            MeshNode target = path[0];
            //if we're sufficiently close to the target switch it
            //find the target's position relative to the position
            Vector2 tpos = new Vector2(Math.Abs(target.position.X - position.X),
                                        Math.Abs(target.position.Z - position.Z));
            if (tpos.X < size.X / 6 && tpos.Y < size.Y / 6) {
                ignore.Clear();
                popFromPath();
                targetAquisitionDuration = 0;
                if (path.Count == 0)
                    setPathTo(core.aiEngine.pickupNodes[core.aiEngine.random.Next(core.aiEngine.pickupNodes.Count)], ignore);
                if (path.Count == 0)
                    return;
                target = path[0];
            }

            if (agentVelocities.persistentVelocity != Vector3.Zero)
                previousTarget = null;

            //calculate direction to target -- we need this for the model, this will probably change, but the principles are right
            Vector3 velocity = target.position - position;
            direction = getDirectionFromVector(velocity);

            //now calculate the move and actually move
            //depending on whether we're on the mesh or not, we don't need collision detection
            //if we've gotten here and there's a previous target then we're on the path
            if (previousTarget != null) {
                velocity.Normalize();
                core.physicsEngine.applySimpleMovement(gameTime, this, velocity * speed);
            }
            //otherwise we need to take care of things the expensive way
            else {
                if (gameTime.TotalGameTime.TotalSeconds - deadReckoningTimeStamp >= deadReckoningTimeout) {
                    velocity.Y = 0;
                    velocity.Normalize();
                    Vector3 oldPos = position;
                    deadReckoningTimeStamp = gameTime.TotalGameTime.TotalSeconds;
                    core.physicsEngine.applyMovement(gameTime, this, speed * velocity);
                    deadReckoningMove = position - oldPos;
                    if (deadReckoningMove.Y != 0)
                        deadReckoningTimeStamp = -deadReckoningTimeout - 1;
                }
                else
                    position += deadReckoningMove;
            }

            targetAquisitionDuration += gameTime.ElapsedGameTime.TotalSeconds;
            if (targetAquisitionDuration >= timeout) {
                ignore.Add(target);
                targetAquisitionDuration = 0;
                if(path.Count > 0)
                    setPathTo(path[path.Count - 1], ignore);
            }

            core.physicsEngine.updateCollisionCellsFor(this);
            curCheckTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (curCheckTime <= 0) {
                curCheckTime = checkTime;
                Vector3 cent = getCenter();
                foreach (Agent a in core.allAgents()) {
                    if (a.spawnTime > 0) continue;
                    Vector3 aCent = a.getCenter();
                    if (Vector3.Dot(cent, aCent) > 0.5 && core.physicsEngine.hitscan(cent, aCent - cent, null) != null) {
                        direction = getDirectionFromVector(Vector3.Normalize(aCent - cent));
                        direction += new Vector2((float)((core.aiEngine.random.NextDouble() * 2 - 1) * (MathHelper.PiOver4 / 4)), (float)((core.aiEngine.random.NextDouble() * 2 - 1) * (MathHelper.PiOver4 / 8)));
                        clampDirection();
                        equipped.fire(this, core.physicsEngine);
                    }
                }
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
