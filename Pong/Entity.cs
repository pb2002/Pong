using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pong
{
    // TODO: Encapsulate!!!!
    public class Entity
    {
        public Box transform;
        public Vector2 velocity;

        public Entity(Box transform)
        {
            this.transform = transform;
            this.velocity = Vector2.Zero;
        }
        public Entity(Vector2 position, Vector2 size)
        {
            this.transform = new Box(position, size);
            this.velocity = Vector2.Zero;
        }
        public Entity(Box transform, Vector2 velocity)
        {
            this.transform = transform;
            this.velocity = velocity;
        }
        public Entity(Vector2 position, Vector2 size, Vector2 velocity)
        {
            this.transform = new Box(position, size);
            this.velocity = velocity;
        }
        public Line[] GenerateCollisionEdges(float dt) {
            float halfSizeX = transform.size.X / 2;
            float halfSizeY = transform.size.Y / 2;
            Vector2[] corners = {
                transform.center + new Vector2(-halfSizeX, -halfSizeY),
                transform.center + new Vector2(halfSizeX, -halfSizeY),
                transform.center + new Vector2(-halfSizeX, halfSizeY),
                transform.center + new Vector2(halfSizeX, halfSizeY),
            };
            Vector2 dx = velocity * dt;

            int quadrant = (velocity.Y >= 0 ? 2 : 0) + (velocity.X >= 0 ? 1 : 0);
            int id = 0, opp = 0, adj1 = 0, adj2 = 0; // identity, opposing, adjecent1 and adjecent2 corner indices

            switch (quadrant)
            {  
                // TODO: make this a lookup table
                case 0: // TL
                    id = 0; opp = 3; adj1 = 1; adj2 = 2;
                    break;
                case 1: // TR
                    id = 1; opp = 2; adj1 = 0; adj2 = 3;
                    break;
                case 2: // BL
                    id = 2; opp = 1; adj1 = 0; adj2 = 3;
                    break;
                case 3: // BR
                    id = 3; opp = 0; adj1 = 1; adj2 = 2;
                    break;
            }

            return new Line[]{ 
                // L shape opposing velocity vector
                new Line(corners[opp], corners[adj1]),
                new Line(corners[opp], corners[adj2]),

                // diagonal pair
                new Line(corners[adj1], corners[adj1] + dx),
                new Line(corners[adj2], corners[adj2] + dx),

                new Line(corners[id] + dx, corners[adj1] + dx),
                new Line(corners[id] + dx, corners[adj2] + dx)};
        }
    }
}
