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

            cooldown = 1.75f;
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
                PhysicsEngine.AgentHitScan ahs = ph.agentHitscan(p.position + new Vector3(0, 60, 0) + p.getDirectionVector() * 10, p.getDirectionVector(), l);
                if (ahs != null) {
                    ahs.agent.health -= damage;
                    makeLaser(p, ahs.ray, Vector3.Distance(ahs.ray.Position, ahs.collisionPoint), 10, 10, "Railgun");
                }
                else {
                    PhysicsEngine.HitScan hs = ph.hitscan(p.position + new Vector3(0, 60, 0) + p.getDirectionVector() * 10, p.getDirectionVector(), null);
                    if (hs != null) {
                        makeLaser(p, hs.ray, Vector3.Distance(hs.ray.Position, hs.collisionPoint), 10, 10, "Railgun");
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