﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Emergence.Pickup
{
    public class PickUpGen
    {

        public PickUp.PickUpType itemType;
        public Vector3 pos;
        int genTime;
        public PickUp held;
        CoreEngine core;

        public PickUpGen(CoreEngine c, Vector3 p, PickUp.PickUpType t)
        {
            core = c;
            pos = p;
            itemType = t;
            held = null;
            genTime = 300;
        
        }

        public void update(GameTime gameTime) {

            if (held == null)               //if no pickup is held by this gen
            {
                if (genTime == 0)           //ready to generate next pickup
                {
                    genPickUp(itemType);
                    genTime = 600;
                }
                else if (genTime > 0)       //countdown to next generation
                    --genTime;
            }
            else                            //spin held pickup
            {
                held.rotation += 0.05f;
                if (held.rotation > 2 * MathHelper.Pi)
                    held.rotation -= 2 * MathHelper.Pi;
            }
        }

        public void genPickUp(PickUp.PickUpType type)
        {

            held = new PickUp(pos + new Vector3(0, 50, 0), itemType, this);
            core.physicsEngine.updateCollisionCellsFor(held);
        }

        public void removePickUp() {
            if (held != null)
                core.physicsEngine.removeFromCollisionGrid(held);
            held = null;
        }

    }
}
