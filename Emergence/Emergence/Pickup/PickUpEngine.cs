using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Emergence.Pickup
{
    public class PickUpEngine
    {

        public PickUpGen[] gens;

        public PickUpEngine(PickUpGen[] g){

            gens = g;
        
        }

        public void Update(GameTime gameTime) {

            foreach (PickUpGen g in gens)
                g.update(gameTime);
        
        }

    }
}
