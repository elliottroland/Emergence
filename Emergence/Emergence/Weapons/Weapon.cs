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
    public abstract class Weapon
    {

        public int damage;
        public int ammoUsed;
        public int cooldown;
        public int curCooldown;

        public Weapon()
        {
        }

        public virtual void Update(GameTime gameTime)
        {
            if (curCooldown > 0)
                curCooldown--;
        }

        public virtual void fire(Player p, PhysicsEngine ph){

            if (curCooldown > 0 || p.ammo < ammoUsed)
                return;

            p.ammo -= ammoUsed;
            curCooldown = cooldown;

        }

        //Traverse the weapon by the left branch
        public abstract Weapon upgradeLeft();

        //Traverse the weapon by the right branch
        public abstract Weapon upgradeRight();

        //Revert to more basic weapon
        public abstract Weapon upgradeDown();

        public void makeNormalBullet(Player p){

            if (curCooldown == cooldown)
            {
                //Do weapon specific things
                Bullet b = new Bullet();
                b.pos = p.position + p.getDirectionVector() + new Vector3(0, 60, 0);
                b.dir = p.getDirectionVector();
                b.timeLeft = 600;
                p.bullets.Add(b);
            }
        
        }

        public void makeLaser(Player p, Ray r, float distance, int width, int height){

            if (curCooldown == cooldown)
            {
                Laser l = new Laser();
                l.horizVerts = new VertexPositionNormalTexture[4];
                l.vertVerts = new VertexPositionNormalTexture[4];
                l.indices = new int[6];
                l.revIndices = new int[6];

                // Set the index buffer for each vertex
                l.indices[0] = 0;
                l.indices[1] = 1;
                l.indices[2] = 2;
                l.indices[3] = 2;
                l.indices[4] = 1;
                l.indices[5] = 3;

                l.revIndices[0] = 2;
                l.revIndices[1] = 1;
                l.revIndices[2] = 0;
                l.revIndices[3] = 3;
                l.revIndices[4] = 1;
                l.revIndices[5] = 2;

                //texture coordinates
                Vector2 textureUpperLeft = new Vector2(0.0f, 0.0f);
                Vector2 textureUpperRight = new Vector2(1.0f, 0.0f);
                Vector2 textureLowerLeft = new Vector2(0.0f, 1.0f);
                Vector2 textureLowerRight = new Vector2(1.0f, 1.0f);

                Vector3 orig = r.Position + r.Direction * distance / 2;

                //normal for vertical quads
                Vector3 vertNorm = Vector3.Cross(Vector3.Up, r.Direction);
                vertNorm.Normalize();

                //normal for horizontal quads
                Vector3 horizNorm = Vector3.Cross(vertNorm, r.Direction);
                horizNorm.Normalize();

                // normals for vertices
                for (int i = 0; i < l.horizVerts.Length; i++)
                {
                    l.horizVerts[i].Normal = horizNorm;
                    l.vertVerts[i].Normal = vertNorm;
                }

                //get horiz corners
                Vector3 uppercenter = (r.Direction * distance / 2) + orig;
                Vector3 UpperLeft = uppercenter + (vertNorm * width / 2);
                Vector3 UpperRight = uppercenter - (vertNorm * width / 2);
                Vector3 LowerLeft = UpperLeft - (r.Direction * distance);
                Vector3 LowerRight = UpperRight - (r.Direction * distance);

                // Set horiz position and texture coordinates
                l.horizVerts[0].Position = LowerLeft;
                l.horizVerts[0].TextureCoordinate = textureLowerLeft;
                l.horizVerts[1].Position = UpperLeft;
                l.horizVerts[1].TextureCoordinate = textureUpperLeft;
                l.horizVerts[2].Position = LowerRight;
                l.horizVerts[2].TextureCoordinate = textureLowerRight;
                l.horizVerts[3].Position = UpperRight;
                l.horizVerts[3].TextureCoordinate = textureUpperRight;

                //get vert corners
                uppercenter = (horizNorm * height / 2) + orig;
                UpperLeft = uppercenter + (r.Direction * distance / 2);
                UpperRight = uppercenter - (r.Direction * distance / 2);
                LowerLeft = UpperLeft - (horizNorm * height);
                LowerRight = UpperRight - (horizNorm * height);

                // Set vert position and texture coordinates
                l.vertVerts[0].Position = LowerLeft;
                l.vertVerts[0].TextureCoordinate = textureLowerLeft;
                l.vertVerts[1].Position = UpperLeft;
                l.vertVerts[1].TextureCoordinate = textureUpperLeft;
                l.vertVerts[2].Position = LowerRight;
                l.vertVerts[2].TextureCoordinate = textureLowerRight;
                l.vertVerts[3].Position = UpperRight;
                l.vertVerts[3].TextureCoordinate = textureUpperRight;

                l.timeLeft = 600;

                p.lasers.Add(l);
            }
        
        }

    }
}