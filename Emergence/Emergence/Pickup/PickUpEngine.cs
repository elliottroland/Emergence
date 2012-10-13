﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Emergence.Pickup
{
    public class PickUpEngine
    {

        public List<PickUpGen> gens;

        public PickUpEngine()
        {
            gens = new List<PickUpGen>();
        }

        public PickUpEngine(PickUpGen[] g) : this(){
            foreach (PickUpGen gg in g)
                gens.Add(gg);
        }

        public void Update(GameTime gameTime) {

            foreach (PickUpGen g in gens)
                g.update(gameTime);
        
        }

    }
}
