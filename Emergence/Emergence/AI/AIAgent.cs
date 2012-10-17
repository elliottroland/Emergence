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

        MeshNode previousTarget = null;
        Agent agentTarget = null;           //the agent we're currently targetting
        float lastShotTimeLimit = 5;        //the seconds that we give up after not having shot at them
        float timeSinceLastShot = 5;        //the time since we last shot at the agent because we could see them

        float checkTime = 0.1f, curCheckTime = 0;

        public AIAgent(CoreEngine c, Vector3 position, Vector2 direction)
            : base(c, position, direction) {
            speed = 200f;
        }
        public AIAgent(CoreEngine c, Vector3 position)
            : this(c, position, new Vector2(1, 0)) { }

        protected MeshNode findClosestMeshNode(List<MeshNode> ignore) {
            return core.aiEngine.findClosestMeshNode(position, 0, ignore);
        }

        //perform A* from the closest node to the target node
        public void setPathTo(MeshNode target, List<MeshNode> ignore) {
            //previousTarget = null;
            targetAquisitionDuration = 0;
            path.Clear();
            MeshNode start = findClosestMeshNode(ignore);
            if (start == null)  return;

            path = pathFrom(start, target, ignore);
        }

        public void addToPath(MeshNode target, List<MeshNode> ignore) {
            if (path.Count == 0) {
                Console.WriteLine("setting path");
                setPathTo(target, ignore);
            }
            else if (path[path.Count - 1] == target) {
                Console.WriteLine("nevermind");
                return;
            }
            else if (path.Contains(target)) {
                Console.WriteLine("truncating path");
                int index = path.IndexOf(target);
                path.RemoveRange(index + 1, path.Count - index - 1);
            }
            else {
                Console.WriteLine("adding path");
                List<MeshNode> pathAdd = pathFrom(path[path.Count - 1], target, ignore);
                if (pathAdd.Count > 0) {
                    Console.WriteLine("added");
                    pathAdd.RemoveAt(0);
                    path.AddRange(pathAdd);
                }
            }
        }

        protected List<MeshNode> pathFrom(MeshNode start, MeshNode target, List<MeshNode> ignore) {
            if (ignore == null) ignore = new List<MeshNode>();
            List<MeshNode> path = new List<MeshNode>();
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
            return path;
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

        public override void Update(GameTime gameTime) {
            /* If we're dead then wait until we can spawn ourselves
             * again.
             */
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

            //update the weapon
            equipped.Update(gameTime);

            //check if we can look for and find an enemy agent
            //if we aren't already targetting an agent, or it's time to give up, find a new target, if we can
            if (timeSinceLastShot >= lastShotTimeLimit) {
                agentTarget = null;
                path.Clear();
                timeSinceLastShot = 0;
                Console.WriteLine("giving up");
            }
            else if(agentTarget != null)
                timeSinceLastShot += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (agentTarget == null) {
                agentTarget = null;
                float closestFeasibleTargetDist = float.MaxValue;
                Vector3 eye = getEyePosition(),
                    dir = getDirectionVector();
                foreach (Agent a in core.allAgents()) {
                    if (a.spawnTime > 0 || a == this) continue;  //if the agent is not in the level, then you can't see them
                    Vector3 aCent = a.getCenter();
                    float dist = Vector3.Distance(eye, aCent);
                    if ( dist < closestFeasibleTargetDist    //we're only concerned in closer enemies
                            && Vector3.Dot(dir, Vector3.Normalize(aCent - eye)) > 0.2) {   //if they're infront of us
                        PhysicsEngine.HitScan hs = core.physicsEngine.hitscan(eye, aCent - eye, null);
                        if (hs == null || hs.Distance() > dist) {
                            agentTarget = a;
                            closestFeasibleTargetDist = Vector3.Distance(eye, aCent);
                            /*direction = getDirectionFromVector(Vector3.Normalize(aCent - eye));
                            direction += new Vector2((float)((core.aiEngine.random.NextDouble() * 2 - 1) * (MathHelper.PiOver4 / 4)), (float)((core.aiEngine.random.NextDouble() * 2 - 1) * (MathHelper.PiOver4 / 8)));
                            clampDirection();
                            equipped.fire(this, core.physicsEngine);*/
                        }
                    }
                }
                if (agentTarget != null) {
                    //plot a path towards the target, the actual movement of this will be taken care of later
                    ignore.Clear();
                    setPathTo(core.aiEngine.findClosestMeshNode(agentTarget.position, 100, ignore), ignore);
                    Console.WriteLine("setting path");
                }
            }
            else if(path.Count == 0) {  //if we're here we need to try shoot at the target
                //setPathTo(core.aiEngine.findClosestMeshNode(agentTarget.position, 100, ignore), ignore);
                //Console.WriteLine("adding to path");
                //addToPath(core.aiEngine.findClosestMeshNode(agentTarget.position, 100, ignore), ignore);
                setPathTo(core.aiEngine.findClosestMeshNode(agentTarget.position, 100, ignore), ignore);
            }


            //if we're not moving towards something then stop ignoring nodes and find a pickup to go to
            if (path.Count == 0) {
                Console.WriteLine("here");
                ignore.Clear();     //should we clear ignore here?
                setPathTo(core.aiEngine.pickupNodes[core.aiEngine.random.Next(core.aiEngine.pickupNodes.Count)], ignore);
                if (path.Count == 0) {
                    Console.WriteLine("no path");
                    return;
                }
            }

            if (path.Count > 0) {

                //try move to the latest point in the path
                MeshNode target = path[0];
                /* if we're sufficiently close to the target switch it
                 * find the target's position relative to the position
                 */
                Vector2 tpos = new Vector2(Math.Abs(target.position.X - position.X),
                                            Math.Abs(target.position.Z - position.Z));
                if (tpos.X < size.X / 6 && tpos.Y < size.Y / 6) {
                    ignore.Clear();
                    popFromPath();
                    targetAquisitionDuration = 0;
                    //if (path.Count == 0 && agentTarget == null)setPathTo(core.aiEngine.pickupNodes[core.aiEngine.random.Next(core.aiEngine.pickupNodes.Count)], ignore);
                    if (path.Count == 0)
                        return;
                    target = path[0];
                }

                if (agentVelocities.persistentVelocity != Vector3.Zero)
                    previousTarget = null;

                //calculate direction to target -- we need this for the model, this will probably change, but the principles are right
                Vector3 velocity = target.position - position;
                direction = getDirectionFromVector(velocity);

                /* now calculate the move and actually move
                 * depending on whether we're on the mesh or not, we don't need collision detection
                 * if we've gotten here and there's a previous target then we're on the path
                 */
                if (true || previousTarget != null) {
                    velocity.Normalize();
                    core.physicsEngine.applySimpleMovement(gameTime, this, velocity * speed);
                }
                //otherwise we need to take care of things the expensive way
                else {
                    velocity.Y = 0;
                    velocity.Normalize();
                    core.physicsEngine.applyMovement(gameTime, this, speed * velocity);
                }

                /* the targetAquisitionDuration variable helps us keep track of the time taken
                 * to move between two nodes. if it's taking too long the we give up on that node
                 * and try a different path to our target node (path[-1])
                 */
                targetAquisitionDuration += gameTime.ElapsedGameTime.TotalSeconds;
                if (targetAquisitionDuration >= timeout) {
                    ignore.Add(target);
                    targetAquisitionDuration = 0;
                    if (path.Count > 0)
                        setPathTo(path[path.Count - 1], ignore);
                }

                core.physicsEngine.updateCollisionCellsFor(this);
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
