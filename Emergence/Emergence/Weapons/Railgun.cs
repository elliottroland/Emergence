using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;


namespace Emergence.Weapons
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Railgun : Weapon
    {
        public Railgun()
        {

            cooldown = 2f;
            damage = 999999;
            ammoUsed = 40;

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void fire(Agent p, PhysicsEngine ph)
        {
            base.fire(p, ph);

            if (curCooldown == cooldown) {
                List<Agent> l = new List<Agent>();
                l.Add(p);
                PhysicsEngine.HitScan hs = ph.hitscan(p.getPosition() + new Vector3(0, 75, 0) + p.getDirectionVector() * 10, p.getDirectionVector(), null);
                PhysicsEngine.AgentHitScan ahs = ph.agentHitscan(p.getPosition() + new Vector3(0, 60, 0) + p.getDirectionVector() * 10, p.getDirectionVector(), l);
                if (hs != null && (ahs == null || hs.Distance() < ahs.Distance()))
                    makeLaser(p, hs.ray, Vector3.Distance(hs.ray.Position, hs.collisionPoint), 10, 10, "Railgun");
                else {
                    if (ahs != null) {
                        ahs.agent.dealDamage((int)damage, p);
                        makeLaser(p, ahs.ray, Vector3.Distance(ahs.ray.Position, ahs.collisionPoint), 10, 10, "Railgun");
                    }
                }
            }
        }

        public override Weapon upgradeLeft()
        {
            return this;
        }

        public override Weapon upgradeRight()
        {
            return this;
        }

        public override Weapon upgradeDown()
        {
            return new Rifle();
        }

    }
}