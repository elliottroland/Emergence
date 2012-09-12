using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Emergence.Pickup
{
    class PickUpGen
    {

        PickUp.PickUpType itemType;
        Vector3 pos;
        int genTime;
        PickUp held;

        public PickUpGen(Vector3 p, PickUp.PickUpType t)
        {

            pos = p;
            itemType = t;
            held = null;
        
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
                held.rotation += 0.1f;
                if (held.rotation > 2 * MathHelper.Pi)
                    held.rotation -= 2 * MathHelper.Pi;
            }
        }

        public void genPickUp(PickUp.PickUpType type)
        {

            held = new PickUp(pos - new Vector3(0, 0, 20), itemType);
        
        }

    }
}
