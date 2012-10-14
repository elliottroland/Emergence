using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Emergence.Pickup
{
    public class PickUpEngine
    {

        public List<PickUpGen> gens;
        CoreEngine core;

        public PickUpEngine(CoreEngine c)
        {
            gens = new List<PickUpGen>();
            core = c;
        }

        public PickUpEngine(CoreEngine c, PickUpGen[] g) : this(c){
            foreach (PickUpGen gg in g)
                gens.Add(gg);
        }

        public void Update(GameTime gameTime) {

            foreach (PickUpGen g in gens)
                g.update(gameTime);
        
        }

    }
}
