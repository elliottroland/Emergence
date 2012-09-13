using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Emergence.Pickup
{
    public class PickUp
    {
        
        public enum PickUpType { AMMO, HEALTH, LEFT, RIGHT };
        public PickUpType type;
        public Vector3 pos;
        public float rotation;

        public PickUp(Vector3 p, PickUpType t) {

            pos = p;
            type = t;
        
        }

    }
}
