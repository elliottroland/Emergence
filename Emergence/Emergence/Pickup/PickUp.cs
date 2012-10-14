using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Emergence.Pickup
{
    public class PickUp : ICollidable
    {
        
        public enum PickUpType { AMMO, HEALTH, LEFT, RIGHT };
        public PickUpType type;
        public Vector3 pos;
        public float rotation;
        public List<CollisionGridCell> collisionCells;
        public PickUpGen pickupGen;

        public PickUp(Vector3 p, PickUpType t, PickUpGen pug) {
            pos = p;
            type = t;
            collisionCells = new List<CollisionGridCell>();
            pickupGen = pug;
        }

        BoundingBox ICollidable.getBoundingBox() {
            return new BoundingBox(pos - new Vector3(16,0,16),
                                    pos + new Vector3(16,16,16));
        }

        List<CollisionGridCell> ICollidable.getCollisionCells() {
            return collisionCells;
        }

        void ICollidable.setCollisionCells(List<CollisionGridCell> cells) {
            collisionCells = cells;
        }

        public void affect(Agent a) {
            if (type == PickUpType.HEALTH)
                a.health += 20;
            else if (type == PickUpType.AMMO)
                a.ammo += 100;
            else if (type == PickUpType.LEFT)
                a.equipped = a.equipped.upgradeLeft();
            else
                a.equipped = a.equipped.upgradeRight();
        }

    }
}
